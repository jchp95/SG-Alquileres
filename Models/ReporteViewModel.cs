using System.Collections.Generic;

namespace Alquileres.Models
{
    public class ReporteViewModel
    {
        public int FidCuenta { get; set; }
        public string NombreInquilino { get; set; }
        public string DireccionInmueble { get; set; }
        public string UbicacionInmueble { get; set; }
        public string FechaActual { get; set; }
        public string HoraActual { get; set; }
        public decimal Fmonto { get; set; }
        public string FechaInicio { get; set; }
        public string NombreUsuario { get; set; }

        // Para los cobros
        public int FidCobro { get; set; }
        public string Fconcepto { get; set; } = null!;

        // Para los atrasos
        public string DescripcionInmueble { get; set; }
        public int CantCuotasAtrasadas { get; set; }
        public decimal MontoTotalAtraso { get; set; }
        public decimal Mora { get; set; }
        public decimal Total { get; set; }


    }

}