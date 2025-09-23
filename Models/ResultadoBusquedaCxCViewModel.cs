namespace Alquileres.Models
{
    public class ResultadoBusquedaCxCViewModel
    {
        public int? Id { get; set; }
        public string Text { get; set; }
        public string Tipo { get; set; }
        public int NumeroCuota { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public DateOnly FfechaInicio { get; set; }
        public decimal Monto { get; set; }
        public decimal Mora { get; set; }
    }
}
