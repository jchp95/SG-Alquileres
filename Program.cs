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
using Alquileres.Services;
using Alquileres.Interfaces;
using EmailSender = Alquileres.Services.EmailSender;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

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

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });


// Configura Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 2;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

    // Configuración del token de recuperación de contraseña
    options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultUI()
.AddDefaultTokenProviders();

// Configuración del tiempo de vida del token (forma correcta)
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromHours(1); // 1 hora de duración
});

// Servicios personalizados
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<GeneradorDeCuotasService>();
builder.Services.AddScoped<ChequeaCxCVencidasServices>();
builder.Services.AddScoped<ActualizadorMoraService>();

// En Program.cs, agrega esto en tus servicios 
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<IEmailSender, ConsoleEmailSender>();
}
else
{
    builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
    builder.Services.AddTransient<IEmailSender, EmailSender>();
}

// Configuración de autenticación dual (Cookies + JWT)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    var jwtSecretKey = builder.Configuration["Jwt:SecretKey"];
    if (string.IsNullOrEmpty(jwtSecretKey))
    {
        throw new InvalidOperationException("JWT Secret Key is not configured");
    }

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecretKey)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Query["access_token"];
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        }
    };
});

// Configuración de autorización CORREGIDA con todos los roles
builder.Services.AddAuthorization(options =>
{
    // Políticas basadas en permisos (mantener esto si lo necesitas)
    var permissions = typeof(Permissions).GetNestedTypes()
        .SelectMany(t => t.GetFields(System.Reflection.BindingFlags.Public |
                      System.Reflection.BindingFlags.Static))
        .Where(f => f.FieldType == typeof(string))
        .Select(f => (string)f.GetValue(null));

    foreach (var permission in permissions)
    {
        options.AddPolicy(permission, policy => policy.RequireClaim("Permission", permission));
    }

    // POLÍTICAS BASADAS EN ROLES - AGREGAR ESTO
    options.AddPolicy("RequiereRolAdministrador", policy =>
    {
        policy.RequireRole(AppConstants.AdministratorRole);
        policy.RequireAuthenticatedUser();
    });

    options.AddPolicy("RequiereRolUsuario", policy =>
    {
        policy.RequireRole(AppConstants.UserRole, AppConstants.AdministratorRole); // Usuario, Gerente o Admin
        policy.RequireAuthenticatedUser();
    });

    // Política para cualquier usuario autenticado
    options.AddPolicy("RequiereAutenticacion", policy =>
    {
        policy.RequireAuthenticatedUser();
    });

    // Política para API (JWT)
    options.AddPolicy("ApiPolicy", policy =>
    {
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
    });

    // Política para MVC (Cookies)
    options.AddPolicy("MvcPolicy", policy =>
    {
        policy.AddAuthenticationSchemes(IdentityConstants.ApplicationScheme);
        policy.RequireAuthenticatedUser();
    });
});

// Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .EnableSensitiveDataLogging()
           .EnableDetailedErrors()
           .LogTo(Console.WriteLine, LogLevel.Information);
});

// Configuración de Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API de Alquileres", Version = "v1" });

    // Configuración para JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    // Configuración para CSRF
    c.AddSecurityDefinition("X-RequestVerificationToken", new OpenApiSecurityScheme
    {
        Description = "Token CSRF para protección contra ataques de falsificación.",
        Name = "X-RequestVerificationToken",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    // Registrar el filtro CSRF
    c.OperationFilter<CsrfOperationFilter>();
    c.OperationFilter<AuthResponsesOperationFilter>();
    c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();

    // Configuración de seguridad global
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
            new string[] {}
        }
    });
});

// Configurar Antiforgery
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-RequestVerificationToken";
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

     // Habilitar credenciales para incluir cookies
     c.ConfigObject.AdditionalItems["requestCredentials"] = "include";

     // Inyectar script para manejar CSRF automáticamente
     c.InjectJavascript("/js/swagger-custom.js");
 });
}

app.UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = true // Para servir el .js personalizado
});
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Middleware para redireccionar a login en errores 401 (solo para MVC)
app.Use(async (context, next) =>
{
    await next();

    // Solo manejar 401 para requests no-API
    if (context.Response.StatusCode == 401 &&
        !context.Request.Path.StartsWithSegments("/api"))
    {
        // Verificar si es una petición AJAX
        if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            // Para AJAX, devolver JSON con URL de redirección
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(new
                {
                    redirectUrl = $"/Identity/Account/Login?ReturnUrl={Uri.EscapeDataString(context.Request.Path)}"
                })
            );
        }
        else
        {
            // Para navegación normal, redirigir directamente
            context.Response.Redirect($"/Identity/Account/Login?ReturnUrl={Uri.EscapeDataString(context.Request.Path)}");
        }
    }
});

app.MapStaticAssets();
app.MapControllers();
app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Inicialización de la base de datos
await InitializeDatabase(app);
await app.RunAsync();

async Task InitializeDatabase(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Inicializando base de datos...");

        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
        await SeedData.InitializeAsync(services);

        logger.LogInformation("Base de datos inicializada correctamente.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al inicializar la base de datos");
    }
}