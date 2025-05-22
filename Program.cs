using Alquileres;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Alquileres.Context;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using Alquileres.Models;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Configuración de logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHostedService<GeneradorDeCuotasService>();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// Configura Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 2;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Servicio de permisos
builder.Services.AddScoped<IPermissionService, PermissionService>();

// Configuración de autorización
builder.Services.AddAuthorization(options =>
{
    // Configura todas las políticas de permisos automáticamente
    var permissions = typeof(Permissions).GetNestedTypes()
        .SelectMany(t => t.GetFields(System.Reflection.BindingFlags.Public |
                      System.Reflection.BindingFlags.Static)
        .Where(f => f.FieldType == typeof(string))
        .Select(f => (string)f.GetValue(null)));

    foreach (var permission in permissions)
    {
        options.AddPolicy(permission,
            policy => policy.RequireClaim("Permission", permission));
    }

    // Políticas personalizadas adicionales si son necesarias
    options.AddPolicy("RequiereRolAdministrador",
        policy => policy.RequireRole("Administrador"));
});

// Configura ruta de Login y Access Denied
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});

// Configura EmailSender
builder.Services.AddTransient<IEmailSender, NoOpEmailSender>();

// Configura DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .EnableSensitiveDataLogging()
           .EnableDetailedErrors()
           .LogTo(Console.WriteLine, LogLevel.Information);
});

// Configuración de Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de Alquileres",
        Version = "v1",
        Description = "API para la gestión de alquileres",
    });

    // Configuración de seguridad para Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Construye la aplicación
var app = builder.Build();

// Configuración del pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API de Alquileres v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Mapeo de endpoints
app.MapStaticAssets();
app.MapControllers();
app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Inicialización de la base de datos
await InitializeDatabase(app);
NotifyDuePayments(app);

await app.RunAsync();

// ===============================
// MÉTODOS AUXILIARES
// ===============================

async Task InitializeDatabase(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Inicializando base de datos...");

        // Ejecutar migraciones
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();

        // Sembrar datos iniciales
        await SeedData.InitializeAsync(services);

        logger.LogInformation("Base de datos inicializada correctamente.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al inicializar la base de datos");
    }
}

void NotifyDuePayments(WebApplication app)
{
    // Implementar lógica para notificar pagos vencidos
    // Ejemplo:
    // var paymentService = app.Services.GetRequiredService<IPaymentNotificationService>();
    // paymentService.StartNotificationTask();
}