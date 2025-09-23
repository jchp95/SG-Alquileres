using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alquileres.Models
{
    public class EmpresaViewModel
    {

        public int IdEmpresa { get; set; } // Este campo no puede ser NULL, ya que es la clave primaria.

        public string? Rnc { get; set; } // Nullable, ya que puede ser NULL en la base de datos.

        public string? Nombre { get; set; } // Nullable.

        public string? Direccion { get; set; } // Nullable.

        public string? Telefonos { get; set; } // Nullable.

        public string? Slogan { get; set; } // Nullable.

        public string? Mensaje { get; set; } // Nullable.

        public byte[]? Logo { get; set; } // Nullable.

        public byte[]? Fondo { get; set; } // Nullable.

        public byte[]? CodigoQrWeb { get; set; } // Nullable.

        public byte[]? CodigoQrRedes { get; set; } // Nullable.

        public string? Email { get; set; } // Nullable.

        public string? Contrasena { get; set; } // Nullable.

        public int? FidComprobante { get; set; }

        public string? NombreComprobante { get; set; } // Add nullable annotation

        public List<SelectListItem> Comprobantes { get; set; }

    }
}
