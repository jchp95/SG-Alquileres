# 🚀 Guía de Instalación y Configuración - SG-Alquileres

<div align="center">

**Guía paso a paso para instalar y configurar el Sistema de Gestión de Alquileres**  
*Incluye configuración de desarrollo, producción y troubleshooting*

</div>

---

## 📑 Tabla de Contenido

1. [🛠️ Instalación desde Cero](#️-instalación-desde-cero)
2. [⚙️ Configuración de Desarrollo](#️-configuración-de-desarrollo)
3. [🌐 Configuración de Producción](#-configuración-de-producción)
4. [🗄️ Configuración de Base de Datos](#️-configuración-de-base-de-datos)
5. [🔐 Configuración de Seguridad](#-configuración-de-seguridad)
6. [📧 Configuración de Email](#-configuración-de-email)
7. [🐛 Solución de Problemas](#-solución-de-problemas)
8. [🔧 Mantenimiento](#-mantenimiento)

---

## 🛠️ Instalación desde Cero

### **Paso 1: Prerrequisitos del Sistema**

#### **Para Windows:**
1. **Descargar e instalar .NET 9.0 SDK**
   ```powershell
   # Verificar instalación
   dotnet --version
   # Debería mostrar: 9.0.x
   ```

2. **Instalar SQL Server**
   - **Opción 1 (Recomendada)**: [SQL Server 2022 Express](https://www.microsoft.com/sql-server/sql-server-downloads)
   - **Opción 2**: SQL Server LocalDB (incluido con Visual Studio)
   ```powershell
   # Verificar LocalDB
   sqllocaldb info
   ```

3. **Instalar Visual Studio 2022** (Opcional pero recomendado)
   - Incluir workload "ASP.NET and web development"
   - Incluir workload "Data storage and processing"

#### **Para macOS:**
```bash
# Instalar .NET 9.0
brew install --cask dotnet

# Instalar SQL Server usando Docker
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" \
   -p 1433:1433 --name sqlserver \
   -d mcr.microsoft.com/mssql/server:2022-latest
```

#### **Para Linux (Ubuntu/Debian):**
```bash
# Instalar .NET 9.0
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-9.0

# SQL Server con Docker
sudo docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" \
   -p 1433:1433 --name sqlserver \
   -d mcr.microsoft.com/mssql/server:2022-latest
```

### **Paso 2: Clonar y Configurar el Proyecto**

```bash
# 1. Clonar el repositorio
git clone https://github.com/tu-usuario/sg-alquileres.git
cd sg-alquileres

# 2. Restaurar paquetes NuGet
dotnet restore

# 3. Verificar herramientas Entity Framework
dotnet tool list -g
# Si no está instalado:
dotnet tool install --global dotnet-ef

# 4. Verificar instalación
dotnet ef --version
```

---

## ⚙️ Configuración de Desarrollo

### **Configurar appsettings.Development.json**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AlquileresDB_Dev;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
  },
  "JwtSettings": {
    "Key": "desarrollo-clave-jwt-super-secreta-y-larga-para-desarrollo",
    "Issuer": "SG-Alquileres-Dev",
    "Audience": "SG-Alquileres-Users-Dev",
    "ExpirationInHours": 24
  },
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUser": "tu-email-desarrollo@gmail.com",
    "SmtpPassword": "tu-app-password",
    "EnableSsl": true,
    "FromName": "SG-Alquileres Desarrollo"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

### **Configurar Variables de Usuario**

#### **Para Windows (Visual Studio):**
1. Clic derecho en el proyecto → "Manage User Secrets"
2. Agregar configuración sensible:

```json
{
  "EmailSettings": {
    "SmtpPassword": "tu-password-real-aqui"
  },
  "JwtSettings": {
    "Key": "tu-clave-jwt-super-secreta"
  }
}
```

#### **Para desarrollo con CLI:**
```bash
# Inicializar user secrets
dotnet user-secrets init

# Agregar secrets
dotnet user-secrets set "EmailSettings:SmtpPassword" "tu-password"
dotnet user-secrets set "JwtSettings:Key" "tu-clave-jwt-secreta"

# Listar secrets
dotnet user-secrets list
```

---

## 🌐 Configuración de Producción

### **appsettings.Production.json**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tu-servidor-prod;Database=AlquileresDB;User Id=tu-usuario;Password=tu-password;TrustServerCertificate=true;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "Key": "#{JWT_SECRET_KEY}#",
    "Issuer": "SG-Alquileres",
    "Audience": "SG-Alquileres-Users",
    "ExpirationInHours": 8
  },
  "EmailSettings": {
    "SmtpHost": "#{SMTP_HOST}#",
    "SmtpPort": 587,
    "SmtpUser": "#{SMTP_USER}#",
    "SmtpPassword": "#{SMTP_PASSWORD}#",
    "EnableSsl": true,
    "FromName": "SG-Alquileres"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Error"
    }
  },
  "AllowedHosts": "tu-dominio.com,*.tu-dominio.com"
}
```

### **Variables de Entorno para Producción**

#### **Linux/Docker:**
```bash
# Crear archivo .env
cat > .env << EOF
ASPNETCORE_ENVIRONMENT=Production
JWT_SECRET_KEY=tu-clave-jwt-super-secreta-produccion
SMTP_HOST=smtp.tu-proveedor.com
SMTP_USER=noreply@tu-dominio.com
SMTP_PASSWORD=tu-password-smtp
DB_CONNECTION_STRING="Server=tu-servidor;Database=AlquileresDB;User Id=sa;Password=tu-password;"
EOF

# Cargar variables
source .env
```

#### **Windows (PowerShell):**
```powershell
# Configurar variables de entorno
$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:JWT_SECRET_KEY = "tu-clave-jwt-super-secreta"
$env:SMTP_HOST = "smtp.tu-proveedor.com"

# O usar setx para persistencia
setx ASPNETCORE_ENVIRONMENT "Production"
```

### **Configuración IIS (Windows Server)**

#### **web.config**
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" 
                  arguments=".\Alquileres.dll" 
                  stdoutLogEnabled="true" 
                  stdoutLogFile=".\logs\stdout" 
                  hostingModel="inprocess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
          <environmentVariable name="JWT_SECRET_KEY" value="tu-clave-secreta" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
```

---

## 🗄️ Configuración de Base de Datos

### **Crear Base de Datos desde Cero**

#### **Opción 1: Usando EF Migrations (Recomendado)**
```bash
# 1. Crear la primera migración
dotnet ef migrations add InitialCreate

# 2. Actualizar la base de datos
dotnet ef database update

# 3. Verificar que se creó correctamente
dotnet ef database update --verbose
```

#### **Opción 2: Script SQL Manual**
```sql
-- Crear base de datos
CREATE DATABASE AlquileresDB;
GO

USE AlquileresDB;
GO

-- Ejecutar script de tablas
-- (El script se genera automáticamente con las migraciones)
```

### **Configuración de Conexiones Alternativas**

#### **SQL Server con Autenticación Windows:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=SERVIDOR\\INSTANCIA;Database=AlquileresDB;Integrated Security=true;TrustServerCertificate=true"
  }
}
```

#### **SQL Server en Docker:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=AlquileresDB;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true"
  }
}
```

#### **Azure SQL Database:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:tu-servidor.database.windows.net,1433;Initial Catalog=AlquileresDB;Persist Security Info=False;User ID=tu-usuario;Password=tu-password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

### **Comandos Útiles Entity Framework**

```bash
# Crear nueva migración
dotnet ef migrations add NombreDeLaMigracion

# Ver migraciones pendientes
dotnet ef migrations list

# Revertir a migración específica
dotnet ef database update NombreMigracionAnterior

# Generar script SQL
dotnet ef migrations script

# Eliminar última migración (sin aplicar)
dotnet ef migrations remove

# Verificar modelo actual
dotnet ef dbcontext info

# Generar modelo desde BD existente (Scaffold)
dotnet ef dbcontext scaffold "tu-connection-string" Microsoft.EntityFrameworkCore.SqlServer -o Models
```

---

## 🔐 Configuración de Seguridad

### **Generar Claves JWT Seguras**

#### **PowerShell:**
```powershell
# Generar clave aleatoria de 256 bits
$bytes = New-Object byte[] 32
[System.Security.Cryptography.RNGCryptoServiceProvider]::Create().GetBytes($bytes)
[Convert]::ToBase64String($bytes)
```

#### **Linux/macOS:**
```bash
# Generar clave aleatoria
openssl rand -base64 32
```

#### **C# (en aplicación):**
```csharp
// Para generar durante desarrollo
public static string GenerateJwtKey()
{
    using var rng = RandomNumberGenerator.Create();
    var bytes = new byte[32];
    rng.GetBytes(bytes);
    return Convert.ToBase64String(bytes);
}
```

### **Configurar HTTPS en Desarrollo**

```bash
# Generar certificado de desarrollo
dotnet dev-certs https --trust

# Verificar certificados
dotnet dev-certs https --check

# Limpiar certificados (si hay problemas)
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

### **Configuración de Roles Iniciales**

Crear un archivo `SeedData.cs` mejorado:

```csharp
public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Crear roles si no existen
        var roles = new[] { "Administrator", "Gerente", "Usuario" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Crear usuario administrador por defecto
        const string adminEmail = "admin@sgalquileres.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        
        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };
            
            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Administrator");
                
                // Crear registro en TbUsuarios
                context.TbUsuarios.Add(new TbUsuario
                {
                    Fnombre = "Administrador",
                    Fusuario = adminEmail,
                    Femail = adminEmail,
                    IdentityId = adminUser.Id,
                    Fnivel = 1,
                    Factivo = true
                });
                
                await context.SaveChangesAsync();
            }
        }
    }
}
```

Llamar en `Program.cs`:
```csharp
// Después de app.Build()
using (var scope = app.Services.CreateScope())
{
    await SeedData.InitializeAsync(scope.ServiceProvider);
}
```

---

## 📧 Configuración de Email

### **Gmail con App Password**

1. **Habilitar 2FA** en tu cuenta de Gmail
2. **Generar App Password:**
   - Ve a Google Account → Security
   - 2-Step Verification → App passwords
   - Generar password para "Mail"

3. **Configurar en appsettings:**
```json
{
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUser": "tu-email@gmail.com",
    "SmtpPassword": "tu-16-character-app-password",
    "EnableSsl": true,
    "FromName": "SG-Alquileres"
  }
}
```

### **Outlook/Hotmail**
```json
{
  "EmailSettings": {
    "SmtpHost": "smtp-mail.outlook.com",
    "SmtpPort": 587,
    "SmtpUser": "tu-email@outlook.com",
    "SmtpPassword": "tu-password",
    "EnableSsl": true
  }
}
```

### **SendGrid (Recomendado para producción)**
```json
{
  "EmailSettings": {
    "SmtpHost": "smtp.sendgrid.net",
    "SmtpPort": 587,
    "SmtpUser": "apikey",
    "SmtpPassword": "tu-sendgrid-api-key",
    "EnableSsl": true
  }
}
```

### **Servidor SMTP Personalizado**
```json
{
  "EmailSettings": {
    "SmtpHost": "mail.tu-dominio.com",
    "SmtpPort": 587,
    "SmtpUser": "noreply@tu-dominio.com",
    "SmtpPassword": "tu-password",
    "EnableSsl": true
  }
}
```

---

## 🐛 Solución de Problemas

### **Problemas Comunes de Instalación**

#### **Error: "dotnet command not found"**
```bash
# Windows
# Reinstalar .NET SDK desde: https://dotnet.microsoft.com/download

# macOS
brew install --cask dotnet-sdk

# Linux
sudo apt-get install dotnet-sdk-9.0
```

#### **Error de conexión a SQL Server**
```bash
# Verificar que SQL Server está ejecutándose
# Windows:
services.msc # Buscar "SQL Server"

# Docker:
docker ps
docker start sqlserver
```

#### **Error: "Login failed for user"**
```sql
-- Crear usuario y otorgar permisos
USE master;
CREATE LOGIN tu_usuario WITH PASSWORD = 'tu_password';

USE AlquileresDB;
CREATE USER tu_usuario FOR LOGIN tu_usuario;
ALTER ROLE db_owner ADD MEMBER tu_usuario;
```

### **Errores de Migración**

#### **Error: "Database already exists"**
```bash
# Opción 1: Eliminar y recrear
dotnet ef database drop
dotnet ef database update

# Opción 2: Forzar recreación
dotnet ef database drop --force
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

#### **Error: "Column already exists"**
```bash
# Ver estado de migraciones
dotnet ef migrations list

# Marcar migraciones como aplicadas sin ejecutar
dotnet ef database update NombreMigracion --no-transactions
```

### **Problemas de SSL/HTTPS**

#### **Certificado no confiable en desarrollo:**
```bash
# Limpiar y regenerar certificados
dotnet dev-certs https --clean
dotnet dev-certs https --trust

# Si persiste el problema (Windows):
certlm.msc # Eliminar certificados de "Personal" y "Trusted Root"
```

#### **Error de SSL en producción:**
```csharp
// Temporal para desarrollo (NO usar en producción)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.CommandTimeout(30);
    }));
```

### **Problemas de Rendimiento**

#### **Consultas lentas:**
```csharp
// Habilitar logging de consultas
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information);

// Usar AsNoTracking para consultas de solo lectura
var datos = await _context.TbInmuebles
    .AsNoTracking()
    .Where(i => i.Factivo)
    .ToListAsync();
```

#### **Timeout de conexión:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "...;Connection Timeout=30;Command Timeout=300;"
  }
}
```

---

## 🔧 Mantenimiento

### **Backups Automáticos**

#### **Script PowerShell para backup diario:**
```powershell
# backup-database.ps1
$fecha = Get-Date -Format "yyyyMMdd_HHmmss"
$backupPath = "C:\Backups\AlquileresDB_$fecha.bak"

sqlcmd -S "(localdb)\mssqllocaldb" -Q "BACKUP DATABASE [AlquileresDB] TO DISK = '$backupPath'"

# Eliminar backups antiguos (más de 30 días)
Get-ChildItem "C:\Backups\AlquileresDB_*.bak" | 
    Where-Object {$_.LastWriteTime -lt (Get-Date).AddDays(-30)} | 
    Remove-Item -Force
```

#### **Script Bash para Linux:**
```bash
#!/bin/bash
# backup-database.sh
DATE=$(date +"%Y%m%d_%H%M%S")
BACKUP_PATH="/backups/AlquileresDB_$DATE.bak"

docker exec sqlserver /opt/mssql-tools/bin/sqlcmd \
    -S localhost -U sa -P "YourStrong!Passw0rd" \
    -Q "BACKUP DATABASE [AlquileresDB] TO DISK = N'$BACKUP_PATH'"

# Eliminar backups antiguos
find /backups -name "AlquileresDB_*.bak" -mtime +30 -delete
```

### **Actualización del Sistema**

#### **Proceso de actualización:**
```bash
# 1. Backup de la base de datos
# 2. Backup del código actual
cp -r /var/www/sgalquileres /var/www/sgalquileres_backup_$(date +%Y%m%d)

# 3. Detener la aplicación
sudo systemctl stop sgalquileres

# 4. Actualizar código
git pull origin main
dotnet restore
dotnet build --configuration Release

# 5. Ejecutar migraciones
dotnet ef database update --configuration Release

# 6. Reiniciar aplicación
sudo systemctl start sgalquileres
sudo systemctl status sgalquileres
```

### **Monitoreo de Logs**

#### **Configuración de logging avanzado:**
```csharp
// En Program.cs
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddFile("Logs/app-{Date}.log", LogLevel.Information);
    loggingBuilder.AddConsole();
});
```

#### **Análisis de logs:**
```bash
# Ver logs en tiempo real
tail -f Logs/app-$(date +%Y%m%d).log

# Buscar errores
grep -i "error\|exception" Logs/app-*.log

# Estadísticas de uso
grep "HTTP GET\|HTTP POST" Logs/app-*.log | wc -l
```

### **Optimización de Base de Datos**

#### **Scripts de mantenimiento:**
```sql
-- Actualizar estadísticas
UPDATE STATISTICS TbCobros;
UPDATE STATISTICS TbCxcs;

-- Reindexar tablas principales
ALTER INDEX ALL ON TbCobros REORGANIZE;
ALTER INDEX ALL ON TbCxcs REORGANIZE;

-- Verificar integridad
DBCC CHECKDB('AlquileresDB');

-- Shrink si es necesario (usar con precaución)
DBCC SHRINKDATABASE('AlquileresDB', 10);
```

---

## 📋 Checklist de Instalación

### **✅ Pre-instalación**
- [ ] .NET 9.0 SDK instalado
- [ ] SQL Server configurado
- [ ] Git instalado
- [ ] Editor de código (VS/VS Code)

### **✅ Instalación**
- [ ] Repositorio clonado
- [ ] Paquetes NuGet restaurados
- [ ] appsettings configurado
- [ ] Variables de entorno establecidas
- [ ] Base de datos creada
- [ ] Migraciones aplicadas

### **✅ Configuración**
- [ ] Usuario administrador creado
- [ ] Email configurado y probado
- [ ] JWT configurado
- [ ] HTTPS funcionando
- [ ] Permisos configurados

### **✅ Pruebas**
- [ ] Aplicación inicia correctamente
- [ ] Login funciona
- [ ] CRUD básico funciona
- [ ] API endpoints responden
- [ ] Swagger accesible

### **✅ Producción**
- [ ] Variables de entorno de producción
- [ ] SSL/TLS configurado
- [ ] Backups programados
- [ ] Logging configurado
- [ ] Monitoreo implementado

---

## 🆘 Soporte y Ayuda

### **Recursos de Ayuda**
- 📧 **Email**: soporte@sgalquileres.com
- 📚 **Documentación**: [DOCUMENTACION.md](DOCUMENTACION.md)
- 🐛 **Issues**: GitHub Issues
- 💬 **Discord**: [Servidor de la comunidad]

### **Información del Sistema**
```bash
# Obtener información completa del entorno
dotnet --info
dotnet --list-sdks
dotnet --list-runtimes

# Información de la aplicación
dotnet run --verbosity diagnostic
```

---

<div align="center">

**🎯 ¡Instalación Completa!**  
*Si seguiste todos los pasos, tu sistema debería estar funcionando correctamente*

[⬆️ Volver arriba](#-guía-de-instalación-y-configuración---sg-alquileres)

</div>
