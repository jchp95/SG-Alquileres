"# 🏠 SG-Alquileres - Sistema de Gestión de Alquileres

<div align="center">

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=.net&logoColor=white)
![C#](https://img.shields.io/badge/C%23-12.0-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?style=for-the-badge&logo=.net&logoColor=white)
![JavaScript](https://img.shields.io/badge/JavaScript-F7DF1E?style=for-the-badge&logo=javascript&logoColor=black)

**Sistema integral de gestión de alquileres con funcionalidades avanzadas de cobros, reportes y administración de usuarios**

[🚀 Características](#-características) • [📦 Instalación](#-instalación) • [🔧 Configuración](#-configuración) • [📖 Documentación](#-documentación) • [🤝 Contribuir](#-contribuir)

</div>

---

## 📝 Descripción

**SG-Alquileres** es un sistema completo de gestión de alquileres desarrollado con **.NET 9.0** y **ASP.NET Core MVC**. Diseñado para administrar eficientemente propiedades en alquiler, inquilinos, propietarios, cuentas por cobrar, cobros y generar reportes detallados.

### 🎯 Objetivo

Simplificar y automatizar la gestión de alquileres, proporcionando herramientas robustas para:
- Control de propiedades e inquilinos
- Gestión financiera y cobros
- Reportería avanzada
- Administración de usuarios y permisos
- Auditoría completa del sistema

---

## ✨ Características

### 🏢 **Gestión de Propiedades**
- ✅ Registro completo de inmuebles con ubicación GPS
- ✅ Administración de propietarios e inquilinos
- ✅ Control de estados y validaciones

### 💰 **Sistema de Cobros**
- ✅ **Cobros rápidos y detallados** - Dos modalidades de cobro
- ✅ **Múltiples métodos de pago** - Efectivo, transferencia, tarjeta, cheque
- ✅ **Comprobantes fiscales** - Generación automática
- ✅ **Cálculo automático de mora** - Basado en días de retraso
- ✅ **Tickets de cobro** - Impresión y descarga

### 📊 **Reportes y Analytics**
- ✅ **Dashboard interactivo** - Métricas en tiempo real
- ✅ **Reportes de cobros** - Por período y usuario
- ✅ **Estado de cuentas** - Detallado por inquilino
- ✅ **Reportes de atrasos** - Seguimiento de morosidad
- ✅ **Exportación PDF/Excel** - Múltiples formatos

### 🔐 **Seguridad y Permisos**
- ✅ **Autenticación Identity** - ASP.NET Core Identity
- ✅ **Sistema de roles** - Administrador, Gerente, Usuario
- ✅ **Permisos granulares** - Control fino por funcionalidad
- ✅ **API REST con JWT** - Endpoints seguros
- ✅ **Auditoría completa** - Trazabilidad de todas las acciones

### 🛠️ **Funcionalidades Técnicas**
- ✅ **Arquitectura MVC** - Patrón Modelo-Vista-Controlador
- ✅ **Entity Framework Core** - ORM con migraciones
- ✅ **Swagger UI** - Documentación API automática
- ✅ **Responsive Design** - Optimizado para móviles
- ✅ **Validaciones robustas** - Cliente y servidor
- ✅ **Notificaciones toast** - SweetAlert2 integrado

---

## 🛠️ Tecnologías

### **Backend**
- **.NET 9.0** - Framework principal
- **ASP.NET Core MVC** - Patrón arquitectural
- **Entity Framework Core** - ORM
- **SQL Server** - Base de datos
- **AutoMapper** - Mapeo de objetos
- **JWT Bearer** - Autenticación API

### **Frontend**
- **Razor Pages** - Motor de vistas
- **Bootstrap 5** - Framework CSS
- **jQuery** - Manipulación DOM
- **DataTables** - Tablas interactivas
- **SweetAlert2** - Notificaciones
- **Select2** - Selectores avanzados

### **Herramientas**
- **Swagger/OpenAPI** - Documentación API
- **Visual Studio** - IDE principal
- **SQL Server Management Studio** - Gestión BD

---

## 📦 Instalación

### **Prerrequisitos**
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) (LocalDB o Server)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) o [VS Code](https://code.visualstudio.com/)

### **Pasos de instalación**

1. **Clonar el repositorio**
   ```bash
   git clone https://github.com/tu-usuario/sg-alquileres.git
   cd sg-alquileres
   ```

2. **Restaurar dependencias**
   ```bash
   dotnet restore
   ```

3. **Configurar la base de datos**
   - Edita `appsettings.json` con tu cadena de conexión:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AlquileresDB;Trusted_Connection=true;MultipleActiveResultSets=true"
     }
   }
   ```

4. **Aplicar migraciones**
   ```bash
   dotnet ef database update
   ```

5. **Ejecutar el proyecto**
   ```bash
   dotnet run
   ```

6. **Acceder a la aplicación**
   - Web: `https://localhost:7000`
   - API: `https://localhost:7000/swagger`

---

## 🔧 Configuración

### **Variables de entorno**
Configura las siguientes variables en `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "tu-cadena-de-conexion"
  },
  "JwtSettings": {
    "Key": "tu-clave-secreta-jwt",
    "Issuer": "SG-Alquileres",
    "Audience": "SG-Alquileres-Users"
  },
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUser": "tu-email@gmail.com",
    "SmtpPassword": "tu-password"
  }
}
```

### **Configuración inicial**
1. **Crear usuario administrador** en el primer inicio
2. **Configurar empresa** en el módulo de empresa
3. **Establecer permisos** para otros usuarios
4. **Configurar comprobantes fiscales** para cobros

---

## 📖 Estructura del Proyecto

```
📁 SG-Alquileres/
├── 📁 Controllers/          # Controladores MVC y API
├── 📁 Models/              # Modelos de datos y ViewModels
├── 📁 Views/               # Vistas Razor
├── 📁 wwwroot/            # Archivos estáticos (CSS, JS, imágenes)
├── 📁 Context/            # DbContext de Entity Framework
├── 📁 Migrations/         # Migraciones de base de datos
├── 📁 Services/           # Servicios de negocio
├── 📁 DTO/               # Data Transfer Objects
├── 📁 Interfaces/        # Interfaces de servicios
├── 📁 Filter/            # Filtros personalizados
├── 📁 Areas/Identity/    # Área de autenticación
├── 📄 Program.cs         # Punto de entrada
└── 📄 appsettings.json   # Configuración
```

---

## 🚀 Funcionalidades Principales

### 1. **Dashboard Principal**
- Métricas en tiempo real de cobros
- Gráficos de tendencias
- Actividades recientes
- Alertas de cuentas vencidas

### 2. **Gestión de Inmuebles**
- CRUD completo de propiedades
- Asignación de propietarios
- Estados activo/inactivo
- Ubicación con Google Maps

### 3. **Administración de Personas**
- **Inquilinos**: Registro con validación de cédula
- **Propietarios**: Gestión de dueños de propiedades
- Validaciones de duplicados
- Estados activo/inactivo

### 4. **Cuentas por Cobrar**
- Creación automática de cuotas
- Cálculo de fechas de vencimiento
- Períodos de pago configurables
- Seguimiento de estados

### 5. **Sistema de Cobros**
- **Modalidad Rápida**: Cobro directo
- **Modalidad Detallada**: Con desglose completo
- Múltiples métodos de pago
- Generación de comprobantes

### 6. **Reportería Avanzada**
- Reportes de cobros por período
- Estados de cuenta detallados
- Reportes de atrasos
- Exportación en múltiples formatos

### 7. **Gestión de Usuarios**
- Creación y edición de usuarios
- Asignación de roles
- Permisos granulares por módulo
- Auditoría de acciones

---

## 📡 API REST

La aplicación incluye una **API REST completa** documentada con Swagger:

### **Endpoints principales**
- `POST /api/auth/login` - Autenticación
- `POST /api/auth/register` - Registro de usuarios  
- `GET /api/cobros` - Listar cobros
- `POST /api/cobros` - Crear cobro
- `GET /api/cuentas-por-cobrar` - Listar cuentas

### **Autenticación**
La API utiliza **JWT Bearer tokens**:
```bash
Authorization: Bearer tu-jwt-token
```

### **Documentación**
Accede a la documentación completa en `/swagger`

---

## 🔐 Sistema de Permisos

### **Roles disponibles**
- **👑 Administrador**: Acceso completo al sistema
- **👨‍💼 Gerente**: Acceso a reportes y gestión
- **👤 Usuario**: Acceso básico según permisos

### **Permisos granulares**
Cada funcionalidad tiene permisos específicos:
- **Ver, Crear, Editar, Anular** para cada módulo
- Control fino por usuario
- Herencia por roles

---

## 🧪 Testing

### **Ejecutar pruebas**
```bash
dotnet test
```

### **Tipos de pruebas**
- ✅ Pruebas unitarias de servicios
- ✅ Pruebas de integración de controladores
- ✅ Pruebas de validación de modelos

---

## 📋 Roadmap

### **Próximas características**
- [ ] 📱 **Aplicación móvil** (React Native)
- [ ] 🔔 **Notificaciones push** para recordatorios
- [ ] 📊 **Analytics avanzado** con Machine Learning
- [ ] 🏦 **Integración bancaria** para pagos automáticos
- [ ] 📄 **Generación de contratos** automática
- [ ] 🌐 **Modo multi-tenancy** para múltiples empresas

---

## 🤝 Contribuir

¡Las contribuciones son bienvenidas! Por favor:

1. **Fork** el proyecto
2. Crea una **rama** para tu feature (`git checkout -b feature/nueva-funcionalidad`)
3. **Commit** tus cambios (`git commit -m 'Agregar nueva funcionalidad'`)
4. **Push** a la rama (`git push origin feature/nueva-funcionalidad`)
5. Abre un **Pull Request**

### **Guías de contribución**
- Seguir las convenciones de código existentes
- Incluir pruebas para nuevas funcionalidades
- Actualizar la documentación según sea necesario
- Usar mensajes de commit descriptivos

---

## 📄 Licencia

Este proyecto está licenciado bajo la **MIT License** - ver el archivo [LICENSE.md](LICENSE.md) para más detalles.

---

## 👨‍💻 Desarrollado por

**Anthony & Julio** - Desarrollo Full Stack  
📧 Email: [tu-email@gmail.com](mailto:tu-email@gmail.com)  
🐙 GitHub: [@tu-usuario](https://github.com/tu-usuario)

---

## 🙏 Agradecimientos

- **Microsoft** por .NET y Entity Framework
- **Bootstrap Team** por el framework CSS  
- **SweetAlert2** por las notificaciones
- **DataTables** por las tablas interactivas
- **Comunidad Open Source** por las librerías utilizadas

---

<div align="center">

**⭐ Si este proyecto te resulta útil, considera darle una estrella en GitHub ⭐**

[⬆️ Volver arriba](#-sg-alquileres---sistema-de-gestión-de-alquileres)

</div>" 
