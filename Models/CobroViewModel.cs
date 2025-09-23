using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;

namespace Alquileres.Models
{
    public class CobroViewModel
    {
        internal List<SelectListItem> MetodosPago;
        internal string MetodoPagoSeleccionado;

        public int FidCobro { get; set; }
        public int FkidCxc { get; set; }
        public DateOnly Ffecha { get; set; }
        public TimeOnly Fhora { get; set; }

        [DisplayName("Monto")]
        public decimal Fmonto { get; set; }

        [DisplayName("Mora")]
        public decimal Fmora { get; set; }

        [DisplayName("Descuento")]
        public decimal Fdescuento { get; set; }

        [DisplayName("Cargos")]
        public decimal Fcargos { get; set; }

        [DisplayName("Concepto")]
        public string Fconcepto { get; set; } = null!;
        public string NombreOrigen { get; set; } = string.Empty; // Propiedad para el nombre del origen
        public bool Factivo { get; set; }

        public string NombreInquilino { get; set; } = string.Empty; // Propiedad para el nombre del inquilino
        public string DescripcionInmueble { get; set; } = string.Empty; // Propiedad para la descripción del inmueble

        public List<SelectListItem> Comprobantes { get; set; }
        public string ComprobanteSeleccionado { get; set; } // Nueva propiedad
    }
}