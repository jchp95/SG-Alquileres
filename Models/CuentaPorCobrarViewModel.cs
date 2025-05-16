namespace Alquileres.Models
{
    public class CuentaPorCobrarViewModel
    {
        public int FidCuenta { get; set; }
        public int? FidInquilino { get; set; }
        public string InquilinoNombre { get; set; }
        public int? FidInmueble { get; set; }
        public decimal Fmonto { get; set; }
        public DateTime FfechaInicio { get; set; }
        public int FdiasGracia { get; set; }
        public bool Factivo { get; set; }
        public decimal FtasaMora { get; set; } // Nuevo campo
        public string Fnota { get; set; } = null!;
        public int FidPeriodoPago { get; set; } // Nuevo campo
        public string NombrePeriodoPago { get; set; }
        public DateTime? FfechaProxCuota { get; set; }

    }
}