using Microsoft.AspNetCore.Antiforgery;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

public class CsrfOperationFilter : IOperationFilter
{
    private readonly IAntiforgery _antiforgery;

    public CsrfOperationFilter(IAntiforgery antiforgery)
    {
        _antiforgery = antiforgery;
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var path = context.ApiDescription.RelativePath; // Ejemplo: "api/consolidado/123"
        var httpMethod = context.ApiDescription.HttpMethod?.ToUpperInvariant();

        // Solo métodos que modifican
        var isModifyingMethod = new[] { "POST", "PUT", "DELETE", "PATCH" }
            .Contains(httpMethod);

        // OMITIR CSRF en rutas API (que empiezan con "api/")
        if (path != null && path.StartsWith("api/", System.StringComparison.OrdinalIgnoreCase))
        {
            return; // No agregamos el header CSRF en Swagger para estas rutas
        }

        if (isModifyingMethod)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            // Agregar header CSRF
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-RequestVerificationToken",
                In = ParameterLocation.Header,
                Required = true,
                Schema = new OpenApiSchema { Type = "string" }
            });

            if (operation.Security == null)
                operation.Security = new List<OpenApiSecurityRequirement>();

            // Configurar seguridad para CSRF
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "X-RequestVerificationToken"
                        }
                    },
                    new string[] {}
                }
            });
        }
    }
}
