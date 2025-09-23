using System.Collections.Generic;

namespace Alquileres.Models
{
    public class ReporteViewModel
    {

        public int FidInquilino { get; set; }
        public int FidInmueble { get; set; }
        public int FidCuenta { get; set; }
        public string NombreInquilino { get; set; }
        public string DireccionInmueble { get; set; }
        public string UbicacionInmueble { get; set; }
        public string FechaActual { get; set; }
        public string HoraActual { get; set; }
        public decimal Fmonto { get; set; }
        public string FechaInicio { get; set; }
        public string NombreUsuario { get; set; }

        public List<TbCxcCuotum> Cuotas { get; set; }
        public List<TbCobro> Pagos { get; set; }
        public List<TbCxcCuotum> Pendientes { get; set; }

        // Para los cobros
        public int FidCobro { get; set; }
        public string Fconcepto { get; set; } = null!;
        public DateOnly Ffecha { get; set; }
        public TimeOnly Fhora { get; set; }
        public decimal MontoCobro { get; set; }
        public decimal Fdescuento { get; set; }
        public decimal Fcargos { get; set; }
        public bool Factivo { get; set; }

        // Para los atrasos
        public string DescripcionInmueble { get; set; }
        public int CantCuotasAtrasadas { get; set; }
        public decimal MontoTotalAtraso { get; set; }
        public decimal MoraTotal { get; set; }
        public decimal Total { get; set; }

        // Para las cuotas
        public int NumeroCuota { get; set; }
        public int DiasAtrasos { get; set; }
        public DateOnly Fvence { get; set; }
        public decimal Fmora { get; set; }

        //Para los gastos
        public decimal Ingresos { get; set; }
        public decimal Gastos { get; set; }
        public bool HayDatos { get; set; }
        public decimal Resultados => Ingresos - Gastos;


    }

}