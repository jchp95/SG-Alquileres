using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class AppendAuthorizeToSummaryOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAuthorize = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
            .Union(context.MethodInfo.GetCustomAttributes(true))
            .OfType<AuthorizeAttribute>().Any();

        if (hasAuthorize)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            // Agregar header de Authorization
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Description = "JWT access token",
                Required = true,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Default = new OpenApiString("Bearer")
                }
            });

            // Agregar header X-Source
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Source",
                In = ParameterLocation.Header,
                Description = "Origen de la solicitud (Web, Android, etc.)",
                Required = true, // Puedes hacerlo requerido si siempre es necesario
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Default = new OpenApiString("Web") // Valor por defecto
                }
            });

            if (!string.IsNullOrEmpty(operation.Summary))
                operation.Summary += " (Requires authentication)";
            else
                operation.Summary = "Requires authentication";
        }
    }
}