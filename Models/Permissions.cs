using Microsoft.AspNetCore.Identity;

namespace Alquileres.Models
{
    public static class Permissions
    {
        // Permisos para Inquilinos
        public static class Inquilinos
        {
            public const string Ver = "Permissions.Inquilinos.Ver";
            public const string Crear = "Permissions.Inquilinos.Crear";
            public const string Editar = "Permissions.Inquilinos.Editar";
            public const string Anular = "Permissions.Inquilinos.Anular";
        }

        // Permisos para Propietarios
        public static class Propietarios
        {
            public const string Ver = "Permissions.Propietarios.Ver";
            public const string Crear = "Permissions.Propietarios.Crear";
            public const string Editar = "Permissions.Propietarios.Editar";
            public const string Anular = "Permissions.Propietarios.Anular";
        }

        // Permisos para Inmuebles
        public static class Inmuebles
        {
            public const string Ver = "Permissions.Inmuebles.Ver";
            public const string Crear = "Permissions.Inmuebles.Crear";
            public const string Editar = "Permissions.Inmuebles.Editar";
            public const string Anular = "Permissions.Inmuebles.Anular";
        }

        // Permisos para Cuenta por cobrar (CxC)
        public static class CxC
        {
            public const string Ver = "Permissions.CxC.Ver";
            public const string Crear = "Permissions.CxC.Crear";
            public const string Editar = "Permissions.CxC.Editar";
            public const string Anular = "Permissions.CxC.Anular";
            public const string Cancelar = "Permissions.CxC.Cancelar";
        }

        // Permisos para las cuotas
        public static class Cuotas
        {
            public const string Ver = "Permissions.Cuotas.Ver";
            public const string Crear = "Permissions.Cuotas.Crear";
            public const string Eliminar = "Permissions.Cuotas.Eliminar";
        }

        // Permisos para Cobros
        public static class Cobros
        {
            public const string Ver = "Permissions.Cobros.Ver";
            public const string Crear = "Permissions.Cobros.Crear";
            public const string VerDetalles = "Permissions.Cobros.VerDetalles";
            public const string Anular = "Permissions.Cobros.Anular";
        }

        // Permisos para Reportes
        public static class Reportes
        {
            public const string Ver = "Permissions.Reportes.Ver";
        }
        public static class Home
        {
            public const string Ver = "Permissions.Home.Ver";
        }

        public static List<string> GetAllPermissions()
        {
            return typeof(Permissions)
                .GetNestedTypes()
                .SelectMany(t => t.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
                .Where(f => f.FieldType == typeof(string))
                .Select(f => (string)f.GetValue(null))
                .ToList();
        }

        public static Dictionary<string, List<string>> GetPermissionsByCategory()
        {
            return new Dictionary<string, List<string>>
            {
                ["Home"] = new List<string> { Home.Ver },
                ["Inquilinos"] = new List<string> { Inquilinos.Ver, Inquilinos.Crear, Inquilinos.Editar, Inquilinos.Anular },
                ["Propietarios"] = new List<string> { Propietarios.Ver, Propietarios.Crear, Propietarios.Editar, Propietarios.Anular },
                ["Inmuebles"] = new List<string> { Inmuebles.Ver, Inmuebles.Crear, Inmuebles.Editar, Inmuebles.Anular },
                ["CuentaPorCobrar"] = new List<string> { CxC.Ver, CxC.Crear, CxC.Editar, CxC.Anular, CxC.Cancelar },
                ["Cuotas"] = new List<string> { Cuotas.Ver, Cuotas.Crear, Cuotas.Eliminar },
                ["Cobros"] = new List<string> { Cobros.Ver, Cobros.Crear, Cobros.VerDetalles, Cobros.Anular },
                ["Reportes"] = new List<string> { Reportes.Ver }
            };
        }

        // Método adicional para obtener el nombre legible
        public static string GetCategoryDisplayName(string categoryKey)
        {
            return categoryKey switch
            {
                "CuentaPorCobrar" => "Cuenta por cobrar",
                _ => categoryKey
            };
        }
    }
}