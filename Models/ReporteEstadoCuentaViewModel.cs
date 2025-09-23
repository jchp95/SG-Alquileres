namespace Alquileres.Models
{
    public class ReporteEstadoCuentaViewModel
    {
        public int FidCuenta { get; set; }
        public string NombreUsuario { get; set; }
        public string NombreInquilino { get; set; }
        public string DescripcionInmueble { get; set; }
        public string DireccionInmueble { get; set; }
        public string UbicacionInmueble { get; set; }
        public int CuotasTotales { get; set; }
        public int CuotasPagadas { get; set; }
        public int CuotasPendientes { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal MontoPagado { get; set; }
        public decimal MontoPendiente { get; set; }
    }
}