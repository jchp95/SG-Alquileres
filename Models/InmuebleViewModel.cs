namespace Alquileres.Models
{
    public class InmuebleViewModel
    {
        public int FidInmueble { get; set; }
        public string Fdescripcion { get; set; }
        public string Fdireccion { get; set; }
        public string Fubicacion { get; set; }
        public decimal Fprecio { get; set; }
        public bool Factivo { get; set; }
        public bool EsVencida { get; set; }
        public string PropietarioNombre { get; set; }
        public string TipoMoneda { get; set; }


    }
}