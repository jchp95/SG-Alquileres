# 📚 Documentación Técnica - SG-Alquileres

<div align="center">

**Documentación completa del Sistema de Gestión de Alquileres**  
*Versión 1.0 - Actualizado: Septiembre 2025*

</div>

---

## 📑 Tabla de Contenido

1. [🏗️ Arquitectura del Sistema](#️-arquitectura-del-sistema)
2. [🗄️ Modelo de Base de Datos](#️-modelo-de-base-de-datos)
3. [🔐 Sistema de Autenticación](#-sistema-de-autenticación)
4. [📋 Módulos Funcionales](#-módulos-funcionales)
5. [🌐 API REST](#-api-rest)
6. [🎨 Frontend y UI](#-frontend-y-ui)
7. [⚙️ Servicios y Lógica de Negocio](#️-servicios-y-lógica-de-negocio)
8. [🔧 Configuración Avanzada](#-configuración-avanzada)
9. [🐛 Debugging y Logging](#-debugging-y-logging)
10. [📈 Performance y Optimización](#-performance-y-optimización)

---

## 🏗️ Arquitectura del Sistema

### **Patrón Arquitectural**
El sistema utiliza **ASP.NET Core MVC** con las siguientes capas:

```
┌─────────────────────────────────────┐
│             PRESENTACIÓN            │
│   Controllers + Views + API         │
├─────────────────────────────────────┤
│              NEGOCIO                │
│        Services + DTOs              │
├─────────────────────────────────────┤
│               DATOS                 │
│    Entity Framework + Context       │
├─────────────────────────────────────┤
│             BASE DE DATOS           │
│            SQL Server               │
└─────────────────────────────────────┘
```

### **Principios de Diseño**
- **SOLID**: Aplicación de principios de diseño sólido
- **DRY**: Don't Repeat Yourself
- **KISS**: Keep It Simple, Stupid
- **Separation of Concerns**: Separación clara de responsabilidades

### **Estructura de Carpetas**
```
📁 Alquileres/
├── 📁 Areas/
│   └── 📁 Identity/           # Área de autenticación ASP.NET
├── 📁 Context/
│   └── ApplicationDbContext.cs # Contexto principal de EF
├── 📁 Controllers/             # Controladores MVC y API
│   ├── HomeController.cs       # Dashboard principal
│   ├── TbInmueblesController.cs # Gestión de inmuebles
│   ├── TbInquilinoesController.cs # Gestión de inquilinos
│   ├── TbPropietariosController.cs # Gestión de propietarios
│   ├── TbCuentasPorCobrarController.cs # Cuentas por cobrar
│   ├── TbCuotasController.cs   # Gestión de cuotas
│   ├── TbCobrosController.cs   # Sistema de cobros
│   ├── TbGastosController.cs   # Gestión de gastos
│   ├── TbUsuariosController.cs # Administración de usuarios
│   ├── ReportesController.cs   # Sistema de reportes
│   ├── AuthApiController.cs    # API de autenticación
│   └── TbCobrosApiController.cs # API de cobros
├── 📁 DTO/                    # Data Transfer Objects
│   ├── CobroRequest.cs         # DTOs para cobros
│   ├── LoginApiRequest.cs      # DTOs para autenticación
│   └── ...                    # Otros DTOs
├── 📁 Enums/
│   └── OrigenCobro.cs         # Enumeraciones del sistema
├── 📁 Filter/                 # Filtros personalizados
│   ├── CsrfOperationFilter.cs  # Filtro CSRF para Swagger
│   └── AuthResponsesOperationFilter.cs # Filtro de respuestas auth
├── 📁 Interfaces/
│   └── ITokenService.cs       # Interfaces de servicios
├── 📁 Models/                 # Modelos y ViewModels
│   ├── 🏠 Entidades principales
│   │   ├── TbInmueble.cs      # Inmuebles
│   │   ├── TbInquilino.cs     # Inquilinos
│   │   ├── TbPropietario.cs   # Propietarios
│   │   ├── TbCxc.cs           # Cuentas por cobrar
│   │   ├── TbCobro.cs         # Cobros
│   │   └── TbUsuario.cs       # Usuarios
│   ├── 📊 ViewModels
│   │   ├── DashboardViewModel.cs # Vista del dashboard
│   │   ├── CobroViewModel.cs   # Vista de cobros
│   │   └── ReporteViewModel.cs # Vista de reportes
│   └── 🔧 Configuración
│       ├── Empresa.cs         # Configuración de empresa
│       └── AppConstants.cs    # Constantes globales
├── 📁 Servicios/              # Servicios de negocio
│   ├── TokenService.cs        # Servicio JWT
│   ├── EmailSender.cs         # Envío de emails
│   ├── GeneradorDeCuotasService.cs # Generación de cuotas
│   └── CalculadorMoraService.cs # Cálculo de mora
├── 📁 Views/                  # Vistas Razor
│   ├── 📁 Home/               # Dashboard
│   ├── 📁 TbInmuebles/        # Vistas de inmuebles
│   ├── 📁 TbCobros/           # Vistas de cobros
│   └── 📁 Shared/             # Vistas compartidas
└── 📁 wwwroot/               # Recursos estáticos
    ├── 📁 css/               # Estilos CSS
    ├── 📁 js/                # JavaScript
    ├── 📁 lib/               # Librerías externas
    └── 📁 images/            # Imágenes
```

---

## 🗄️ Modelo de Base de Datos

### **Diagrama ER Principales**
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   TbPropietario │────│   TbInmueble    │────│   TbInquilino   │
│                 │    │                 │    │                 │
│ - FidPropietario│    │ - FidInmueble   │    │ - FidInquilino  │
│ - Fnombre       │    │ - Fdescripcion  │    │ - Fnombre       │
│ - Fapellidos    │    │ - Fdireccion    │    │ - Fapellidos    │
│ - Fcedula       │    │ - Fmonto        │    │ - Fcedula       │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         │                       └───────┬───────────────┘
         │                               │
         │              ┌─────────────────▼─────────────────┐
         │              │            TbCxc                │
         │              │    (Cuentas por Cobrar)         │
         │              │ - FidCuenta                     │
         │              │ - FkidInmueble                  │
         │              │ - FkidInquilino                 │
         │              │ - Fmonto                        │
         │              │ - Fvence                        │
         │              └─────────────────┬─────────────────┘
         │                               │
         │                    ┌─────────▼─────────┐
         │                    │   TbCxcCuotum     │
         │                    │    (Cuotas)       │
         │                    │ - FidCuota        │
         │                    │ - FkidCxc         │
         │                    │ - Fmonto          │
         │                    │ - Fvence          │
         │                    │ - Fstatus         │
         │                    └─────────┬─────────┘
         │                              │
         └─────────────────┬────────────┘
                           │
              ┌─────────────▼─────────────┐
              │         TbCobro           │
              │        (Cobros)           │
              │ - FidCobro               │
              │ - FkidCuenta             │
              │ - FkidUsuario            │
              │ - Fmonto                 │
              │ - Ffecha                 │
              └───────────────────────────┘
```

### **Entidades Principales**

#### **TbInmueble** - Gestión de Propiedades
```sql
CREATE TABLE TbInmuebles (
    FidInmueble INT IDENTITY PRIMARY KEY,
    FkidPropietario INT NOT NULL,
    Fdescripcion NVARCHAR(255),
    Fdireccion NVARCHAR(500),
    Fmonto DECIMAL(18,2),
    FkidMoneda INT,
    Factivo BIT DEFAULT 1,
    FfechaCreacion DATETIME2 DEFAULT GETDATE(),
    Fcoordenadas NVARCHAR(100) -- Lat,Lng para Google Maps
)
```

#### **TbCxc** - Cuentas por Cobrar
```sql
CREATE TABLE TbCxcs (
    FidCuenta INT IDENTITY PRIMARY KEY,
    FkidInmueble INT NOT NULL,
    FkidInquilino INT NOT NULL,
    Fmonto DECIMAL(18,2) NOT NULL,
    Fvence DATETIME2 NOT NULL,
    FdiasGracia INT DEFAULT 0,
    FtasaMora DECIMAL(5,2) DEFAULT 0,
    FkidPeriodoPago INT NOT NULL,
    Fstatus CHAR(1) DEFAULT 'A', -- A=Activa, S=Saldada, C=Cancelada
    Factivo BIT DEFAULT 1
)
```

#### **TbCobro** - Registro de Cobros
```sql
CREATE TABLE TbCobros (
    FidCobro INT IDENTITY PRIMARY KEY,
    FkidCuenta INT NOT NULL,
    FkidUsuario INT NOT NULL,
    Fmonto DECIMAL(18,2) NOT NULL,
    Ffecha DATETIME2 DEFAULT GETDATE(),
    Fconcepto NVARCHAR(500),
    Fstatus CHAR(1) DEFAULT 'A', -- A=Activo, N=Anulado
    FtipoComprobante NVARCHAR(100),
    FnoComprobante NVARCHAR(100)
)
```

### **Relaciones Importantes**
- **TbInmueble** ← `FkidPropietario` → **TbPropietario** (1:N)
- **TbCxc** ← `FkidInmueble` → **TbInmueble** (N:1)
- **TbCxc** ← `FkidInquilino` → **TbInquilino** (N:1)
- **TbCxcCuotum** ← `FkidCxc` → **TbCxc** (N:1)
- **TbCobro** ← `FkidCuenta` → **TbCxc** (N:1)

---

## 🔐 Sistema de Autenticación

### **ASP.NET Core Identity**
El sistema utiliza **Identity** para manejo completo de usuarios:

```csharp
// Configuración en Program.cs
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
```

### **JWT Authentication para API**
```csharp
// Configuración JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});
```

### **Sistema de Roles y Permisos**

#### **Roles Predefinidos**
```csharp
public static class AppRoles
{
    public const string Administrator = "Administrator";
    public const string Gerente = "Gerente"; 
    public const string Usuario = "Usuario";
}
```

#### **Permisos Granulares**
```csharp
public static class Permissions
{
    // Inquilinos
    public const string InquilinosVer = "Permissions.Inquilinos.Ver";
    public const string InquilinosCrear = "Permissions.Inquilinos.Crear";
    public const string InquilinosEditar = "Permissions.Inquilinos.Editar";
    public const string InquilinosAnular = "Permissions.Inquilinos.Anular";
    
    // Propietarios
    public const string PropietariosVer = "Permissions.Propietarios.Ver";
    // ... más permisos
    
    // Cobros
    public const string CobrosVer = "Permissions.Cobros.Ver";
    public const string CobrosCrear = "Permissions.Cobros.Crear";
    // ... más permisos
}
```

#### **Autorización por Policies**
```csharp
// Configuración de políticas
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Permissions.Inquilinos.Ver", policy =>
        policy.RequireClaim("permission", "Permissions.Inquilinos.Ver"));
    
    options.AddPolicy("Permissions.Cobros.Crear", policy =>
        policy.RequireClaim("permission", "Permissions.Cobros.Crear"));
});
```

---

## 📋 Módulos Funcionales

### **1. 🏠 Gestión de Inmuebles**

#### **Controlador Principal**
```csharp
[Authorize]
public class TbInmueblesController : BaseController
{
    [Authorize(Policy = "Permissions.Inmuebles.Ver")]
    public async Task<IActionResult> CargarInmuebles()
    
    [Authorize(Policy = "Permissions.Inmuebles.Crear")]
    public async Task<IActionResult> Create()
    
    [Authorize(Policy = "Permissions.Inmuebles.Editar")]
    public async Task<IActionResult> Edit(int? id)
}
```

#### **Características**
- ✅ CRUD completo con validaciones
- ✅ Integración con Google Maps
- ✅ Validación de duplicados
- ✅ Estados activo/inactivo
- ✅ Asociación con propietarios

### **2. 👥 Gestión de Personas**

#### **Inquilinos**
```csharp
[Authorize]
public class TbInquilinoesController : BaseController
{
    [Authorize(Policy = "Permissions.Inquilinos.Crear")]
    public async Task<IActionResult> Create([Bind] TbInquilino tbInquilino)
    {
        // Validación de cédula única
        var existeCedula = await _context.TbInquilinos
            .AnyAsync(i => i.Fcedula == tbInquilino.Fcedula);
            
        if (existeCedula)
        {
            ModelState.AddModelError("Fcedula", "Ya existe un inquilino con esta cédula");
            return BadRequest(new { success = false, errors = GetModelErrors() });
        }
        
        // Crear inquilino...
    }
}
```

#### **Validaciones Implementadas**
- ✅ Cédula única por inquilino/propietario
- ✅ Formato de teléfono dominicano
- ✅ Validación de emails
- ✅ Campos obligatorios

### **3. 💰 Sistema de Cuentas por Cobrar**

#### **Generación Automática de Cuotas**
```csharp
public class GeneradorDeCuotasService
{
    public async Task<List<TbCxcCuotum>> GenerarCuotasAsync(
        int cxcId, 
        DateTime fechaInicio, 
        int numeroCuotas, 
        decimal monto, 
        int periodoPagoId)
    {
        var cuotas = new List<TbCxcCuotum>();
        
        for (int i = 1; i <= numeroCuotas; i++)
        {
            var fechaVencimiento = CalcularFechaVencimiento(fechaInicio, i, periodoPagoId);
            
            cuotas.Add(new TbCxcCuotum
            {
                FkidCxc = cxcId,
                Fnumero = i,
                Fmonto = monto,
                Fvence = fechaVencimiento,
                Fstatus = 'P' // Pendiente
            });
        }
        
        return cuotas;
    }
}
```

### **4. 💳 Sistema de Cobros**

#### **Modalidades de Cobro**

##### **Cobro Rápido**
```csharp
public IActionResult CreateCobroRapido()
{
    // Interfaz simplificada para cobros express
    var metodosPago = new List<SelectListItem>
    {
        new SelectListItem { Value = "Efectivo", Text = "Efectivo" },
        new SelectListItem { Value = "Transferencia", Text = "Transferencia" }
    };
    
    return PartialView("_CobroRapidoPartial", metodosPago);
}
```

##### **Cobro Detallado**
```csharp
[Authorize(Policy = "Permissions.Cobros.Crear")]
public IActionResult Create()
{
    // Verificar comprobantes fiscales
    var tieneComprobantes = _context.TbComprobantesFiscales.Any();
    if (!tieneComprobantes)
    {
        return BadRequest(new
        {
            success = false,
            errorType = "no_fiscal_documents",
            message = "No hay comprobantes fiscales configurados"
        });
    }
    
    // Cargar datos para formulario completo...
}
```

#### **Cálculo Automático de Mora**
```csharp
public class CalculadorMoraService
{
    public decimal CalcularMora(DateTime fechaVencimiento, decimal monto, decimal tasaMora, int diasGracia)
    {
        var diasRetraso = (DateTime.Now.Date - fechaVencimiento.Date).Days;
        
        if (diasRetraso <= diasGracia)
            return 0;
            
        var diasMora = diasRetraso - diasGracia;
        return (monto * tasaMora / 100) * diasMora;
    }
}
```

### **5. 📊 Sistema de Reportes**

#### **Dashboard Interactivo**
```csharp
public async Task<IActionResult> Index()
{
    var hoy = DateTime.Now;
    var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
    
    var viewModel = new DashboardViewModel
    {
        // Métricas del mes actual
        CobrosDelMes = await _context.TbCobros
            .Where(c => c.Ffecha >= inicioMes && c.Fstatus == 'A')
            .SumAsync(c => c.Fmonto),
            
        // Cuentas vencidas
        CuentasVencidas = await _context.TbCxcCuota
            .Where(c => c.Fvence < hoy && c.Fstatus == 'P')
            .CountAsync(),
            
        // Actividades recientes
        ActividadesRecientes = await ObtenerActividadesRecientes()
    };
    
    return View(viewModel);
}
```

#### **Reportes Disponibles**
- **📈 Reportes de Cobros**: Por período, usuario, método de pago
- **📋 Estados de Cuenta**: Detallado por inquilino
- **⚠️ Reportes de Atrasos**: Seguimiento de morosidad
- **💰 Reportes de Gastos**: Control de egresos

### **6. 👨‍💼 Gestión de Usuarios**

#### **Creación con Permisos**
```csharp
[Authorize(Policy = "Permissions.Usuario.Crear")]
public async Task<IActionResult> Create(UsuarioViewModel viewModel)
{
    // Crear usuario en Identity
    var user = new IdentityUser
    {
        UserName = viewModel.Fusuario,
        Email = viewModel.Femail
    };
    
    var result = await _userManager.CreateAsync(user, viewModel.Fpassword);
    
    if (result.Succeeded)
    {
        // Asignar rol
        await _userManager.AddToRoleAsync(user, viewModel.SelectedRole);
        
        // Crear registro en TbUsuario
        var nuevoUsuario = new TbUsuario
        {
            Fnombre = viewModel.Fnombre,
            Fusuario = viewModel.Fusuario,
            Femail = viewModel.Femail,
            IdentityId = user.Id,
            Fnivel = GetNivelByRole(viewModel.SelectedRole)
        };
        
        _context.TbUsuarios.Add(nuevoUsuario);
        await _context.SaveChangesAsync();
        
        // Asignar permisos específicos
        if (viewModel.SelectedPermissions?.Any() == true)
        {
            await AssignPermissionsToUser(user.Id, viewModel.SelectedPermissions);
        }
    }
}
```

---

## 🌐 API REST

### **Endpoints de Autenticación**

#### **Login**
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "usuario@ejemplo.com",
  "password": "password123"
}
```

**Respuesta:**
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiration": "2025-09-24T10:30:00Z",
  "user": {
    "id": "123",
    "name": "Juan Pérez",
    "email": "juan@ejemplo.com",
    "roles": ["Usuario"]
  }
}
```

#### **Registro**
```http
POST /api/auth/register
Content-Type: application/json

{
  "name": "Nuevo Usuario",
  "email": "nuevo@ejemplo.com", 
  "password": "password123"
}
```

### **Endpoints de Cobros**

#### **Crear Cobro**
```http
POST /api/cobros
Authorization: Bearer {token}
Content-Type: application/json

{
  "cuentaId": 123,
  "monto": 15000.00,
  "metodoPago": "Efectivo",
  "concepto": "Pago de alquiler mes de septiembre",
  "desglose": {
    "efectivo": 15000.00,
    "transferencia": 0,
    "tarjeta": 0
  }
}
```

#### **Listar Cobros**
```http
GET /api/cobros?page=1&pageSize=10&fechaDesde=2025-09-01&fechaHasta=2025-09-30
Authorization: Bearer {token}
```

### **Swagger Documentation**
La API está completamente documentada con **Swagger/OpenAPI**:

- **URL**: `https://localhost:7000/swagger`
- **Autenticación JWT**: Configurada automáticamente
- **Filtros personalizados**: Para CSRF y autenticación

---

## 🎨 Frontend y UI

### **Tecnologías Frontend**
- **Bootstrap 5**: Framework CSS responsive
- **jQuery**: Manipulación DOM y AJAX
- **DataTables**: Tablas interactivas con paginación
- **SweetAlert2**: Notificaciones y confirmaciones elegantes
- **Select2**: Selectores con búsqueda avanzada
- **Google Maps API**: Integración de mapas

### **Arquitectura JavaScript**
```javascript
// Patrón Módulo para organización
const CobroModule = {
    // Configuración
    config: {
        apiUrl: '/api/cobros',
        selectors: {
            form: '#cobroForm',
            table: '#cobrosTable'
        }
    },
    
    // Inicialización
    init() {
        this.bindEvents();
        this.loadData();
    },
    
    // Eventos
    bindEvents() {
        $(this.config.selectors.form).on('submit', this.handleSubmit.bind(this));
    },
    
    // Métodos
    async handleSubmit(e) {
        e.preventDefault();
        // Lógica de envío...
    }
};
```

### **Componentes Reutilizables**

#### **Toast Notifications**
```javascript
// Configuración global de toasts
const Toast = Swal.mixin({
    toast: true,
    position: 'top-end',
    showConfirmButton: false,
    timer: 3000,
    timerProgressBar: true
});

// Función helper
function showToast(message, type = 'success') {
    Toast.fire({
        icon: type,
        title: message
    });
}
```

#### **DataTables Configuración**
```javascript
function initDataTable(selector, options = {}) {
    const defaultOptions = {
        responsive: true,
        language: {
            url: '/lib/datatables/Spanish.json'
        },
        pageLength: 10,
        order: [[0, 'desc']]
    };
    
    return $(selector).DataTable({
        ...defaultOptions,
        ...options
    });
}
```

### **Validaciones Cliente**
```javascript
// Validación de formularios con jQuery Validation
function initFormValidation(form) {
    form.validate({
        rules: {
            Fnombre: {
                required: true,
                minlength: 2
            },
            Fcedula: {
                required: true,
                digits: true,
                minlength: 11,
                maxlength: 11
            }
        },
        messages: {
            Fnombre: {
                required: "El nombre es obligatorio",
                minlength: "Mínimo 2 caracteres"
            }
        }
    });
}
```

---

## ⚙️ Servicios y Lógica de Negocio

### **TokenService - Gestión JWT**
```csharp
public class TokenService : ITokenService
{
    public async Task<string> GenerateTokenAsync(IdentityUser user, IList<string> roles, IList<string> permissions)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email)
        };
        
        // Agregar roles como claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        
        // Agregar permisos como claims
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(24),
            signingCredentials: creds
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

### **EmailSender - Notificaciones**
```csharp
public class EmailSender : IEmailSender
{
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        using var client = new SmtpClient(_smtpHost, _smtpPort)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(_smtpUser, _smtpPassword)
        };
        
        var message = new MailMessage
        {
            From = new MailAddress(_smtpUser, "SG-Alquileres"),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };
        
        message.To.Add(email);
        
        await client.SendMailAsync(message);
    }
}
```

### **ChequeaCxCVencidasServices - Alertas Automáticas**
```csharp
public class ChequeaCxCVencidasServices
{
    public async Task<List<CuentaVencida>> ObtenerCuentasVencidasAsync()
    {
        var fechaHoy = DateTime.Now.Date;
        
        return await _context.TbCxcCuota
            .Where(cuota => cuota.Fvence.Date < fechaHoy && cuota.Fstatus == 'P')
            .Include(c => c.FkidCxcNavigation)
                .ThenInclude(cxc => cxc.FkidInquilinoNavigation)
            .Include(c => c.FkidCxcNavigation)
                .ThenInclude(cxc => cxc.FkidInmuebleNavigation)
            .Select(cuota => new CuentaVencida
            {
                CuentaId = cuota.FkidCxc,
                InquilinoNombre = cuota.FkidCxcNavigation.FkidInquilinoNavigation.Fnombre,
                InmuebleDescripcion = cuota.FkidCxcNavigation.FkidInmuebleNavigation.Fdescripcion,
                FechaVencimiento = cuota.Fvence,
                Monto = cuota.Fmonto,
                DiasVencido = (fechaHoy - cuota.Fvence.Date).Days
            })
            .ToListAsync();
    }
}
```

---

## 🔧 Configuración Avanzada

### **Program.cs - Configuración Principal**
```csharp
var builder = WebApplication.CreateBuilder(args);

// 🔐 Configuración de Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 🗄️ Configuración de Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🌐 Configuración de Controllers
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
})
.AddRazorRuntimeCompilation()
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// 📊 Configuración de Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de Alquileres",
        Version = "v1",
        Description = "API REST para el sistema de gestión de alquileres"
    });
    
    // Configuración de autenticación JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando Bearer token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
});

// 🔧 Inyección de Dependencias
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<GeneradorDeCuotasService>();
builder.Services.AddScoped<CalculadorMoraService>();
builder.Services.AddScoped<ChequeaCxCVencidasServices>();
```

### **ApplicationDbContext - Configuración EF**
```csharp
public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options) { }
    
    // DbSets principales
    public DbSet<TbInmueble> TbInmuebles { get; set; }
    public DbSet<TbInquilino> TbInquilinos { get; set; }
    public DbSet<TbPropietario> TbPropietarios { get; set; }
    public DbSet<TbCxc> TbCxcs { get; set; }
    public DbSet<TbCobro> TbCobros { get; set; }
    public DbSet<TbUsuario> TbUsuarios { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Configuraciones específicas
        builder.Entity<TbCxc>()
            .HasIndex(c => c.FkidInquilino)
            .HasDatabaseName("IX_TbCxcs_FkidInquilino");
            
        builder.Entity<TbCobro>()
            .Property(c => c.Fmonto)
            .HasColumnType("decimal(18,2)");
            
        // Configurar relaciones
        builder.Entity<TbCxc>()
            .HasOne<TbInmueble>()
            .WithMany()
            .HasForeignKey(c => c.FkidInmueble);
    }
}
```

### **appsettings.json - Configuración**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AlquileresDB;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "Key": "mi-clave-secreta-jwt-muy-segura-y-larga",
    "Issuer": "SG-Alquileres",
    "Audience": "SG-Alquileres-Users",
    "ExpirationInHours": 24
  },
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUser": "tu-email@gmail.com",
    "SmtpPassword": "tu-app-password",
    "EnableSsl": true
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

---

## 🐛 Debugging y Logging

### **Configuración de Logging**
```csharp
// En Program.cs
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information);
}
```

### **Uso de ILogger en Controladores**
```csharp
public class TbCobrosController : BaseController
{
    private readonly ILogger<TbCobrosController> _logger;
    
    public TbCobrosController(ApplicationDbContext context, ILogger<TbCobrosController> logger)
        : base(context)
    {
        _logger = logger;
    }
    
    public async Task<IActionResult> Create([FromBody] CobroRequest request)
    {
        try
        {
            _logger.LogInformation("Iniciando creación de cobro para cuenta {CuentaId}", request.CuentaId);
            
            // Lógica de creación...
            
            _logger.LogInformation("Cobro creado exitosamente con ID {CobroId}", nuevoCobro.FidCobro);
            return Ok(new { success = true, cobroId = nuevoCobro.FidCobro });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear cobro para cuenta {CuentaId}", request.CuentaId);
            return StatusCode(500, new { success = false, message = "Error interno del servidor" });
        }
    }
}
```

### **Manejo Global de Errores**
```csharp
// Middleware personalizado
public class GlobalExceptionMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no manejado en {Path}", context.Request.Path);
            
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                success = false,
                message = "Error interno del servidor",
                traceId = Activity.Current?.Id ?? context.TraceIdentifier
            }));
        }
    }
}
```

---

## 📈 Performance y Optimización

### **Entity Framework Optimizations**

#### **Lazy Loading Selectivo**
```csharp
// Deshabilitar lazy loading globalmente
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.UseLazyLoadingProxies(false);
}

// Cargar datos específicamente cuando sea necesario
public async Task<List<CobroViewModel>> GetCobrosAsync()
{
    return await _context.TbCobros
        .Include(c => c.FkidCuentaNavigation)
            .ThenInclude(cxc => cxc.FkidInquilinoNavigation)
        .Include(c => c.FkidUsuarioNavigation)
        .Where(c => c.Fstatus == 'A')
        .Select(c => new CobroViewModel
        {
            CobroId = c.FidCobro,
            Monto = c.Fmonto,
            Fecha = c.Ffecha,
            InquilinoNombre = c.FkidCuentaNavigation.FkidInquilinoNavigation.Fnombre
        })
        .ToListAsync();
}
```

#### **Paginación Eficiente**
```csharp
public async Task<PagedResult<T>> GetPagedAsync<T>(
    IQueryable<T> query, 
    int pageNumber, 
    int pageSize)
{
    var count = await query.CountAsync();
    var items = await query
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
        
    return new PagedResult<T>
    {
        Items = items,
        TotalCount = count,
        PageNumber = pageNumber,
        PageSize = pageSize
    };
}
```

### **Caché de Datos Frecuentes**
```csharp
public class CachedDataService
{
    private readonly IMemoryCache _cache;
    
    public async Task<List<SelectListItem>> GetPropietariosForSelectAsync()
    {
        const string cacheKey = "propietarios-select-list";
        
        if (!_cache.TryGetValue(cacheKey, out List<SelectListItem> propietarios))
        {
            propietarios = await _context.TbPropietarios
                .Where(p => p.Factivo)
                .Select(p => new SelectListItem
                {
                    Value = p.FidPropietario.ToString(),
                    Text = $"{p.Fnombre} {p.Fapellidos}"
                })
                .ToListAsync();
                
            _cache.Set(cacheKey, propietarios, TimeSpan.FromMinutes(30));
        }
        
        return propietarios;
    }
}
```

### **Optimización JavaScript**
```javascript
// Debounce para búsquedas
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Búsqueda optimizada
const optimizedSearch = debounce(function(searchTerm) {
    // Realizar búsqueda...
}, 300);

// Carga lazy de DataTables
$(document).ready(function() {
    // Inicializar DataTables solo cuando sea visible
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                initDataTable(entry.target);
                observer.unobserve(entry.target);
            }
        });
    });
    
    $('.data-table-container').each(function() {
        observer.observe(this);
    });
});
```

---

## 📚 Recursos Adicionales

### **Documentación Relacionada**
- [ASP.NET Core MVC](https://docs.microsoft.com/aspnet/core/mvc)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [ASP.NET Core Identity](https://docs.microsoft.com/aspnet/core/security/authentication/identity)
- [JWT Authentication](https://jwt.io/introduction/)

### **Herramientas Recomendadas**
- **Visual Studio 2022** - IDE principal
- **SQL Server Management Studio** - Gestión de BD
- **Postman** - Testing de API
- **Git** - Control de versiones

### **Extensiones VS Code**
- C# for Visual Studio Code
- REST Client
- SQL Server (mssql)
- GitLens

---

<div align="center">

**📝 Esta documentación se actualiza constantemente**  
*Última actualización: Septiembre 2025*

[⬆️ Volver arriba](#-documentación-técnica---sg-alquileres)

</div>
