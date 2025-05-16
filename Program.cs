using Alquileres;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Alquileres.Context;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;

// Asegúrate de usar el espacio de nombres correcto para NoOpEmailSender

var builder = WebApplication.CreateBuilder(args);

// Configuración de logging
builder.Logging.ClearProviders(); // Limpiar proveedores de logging existentes
builder.Logging.AddConsole(); // Agregar el proveedor de consola
builder.Logging.AddDebug(); // Agregar el proveedor de depuración (opcional)

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

// Configura ruta de Login y Access Denied
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login"; // Redirige correctamente al Login de Identity
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Configura EmailSender
builder.Services.AddTransient<IEmailSender, NoOpEmailSender>(); // Asegúrate de que NoOpEmailSender sea el correcto

// Configura DbContext
builder.Services.AddDbContext<ApplicationDbContext>((services, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .EnableSensitiveDataLogging(); // Habilitar el registro de datos sensibles
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de Alquileres",
        Version = "v1",
        Description = "API para la gestión de alquileres",
    });
});


var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API de Alquileres v1");
    c.RoutePrefix = "swagger"; // Disponible en /swagger
});



app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllers();
app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Inicializa base de datos y notifica
await InitializeDatabase(app);
NotifyDuePayments(app);

await app.RunAsync();


// ===============================
// MÉTODOS AUXILIARES
// ===============================

async Task InitializeDatabase(WebApplication app) // Cambiar a método async
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        await SeedData.InitializeAsync(services);
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Base de datos inicializada correctamente.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error occurred seeding the DB.");
    }
}

void NotifyDuePayments(WebApplication app)
{
    // Implementar lógica para notificar pagos vencidos
}