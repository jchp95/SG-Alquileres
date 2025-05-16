namespace Alquileres.Models
{
    public class CobroViewModel
    {
        public int FidCobro { get; set; }
        public int FkidCxc { get; set; }
        public DateOnly Ffecha { get; set; }
        public TimeOnly Fhora { get; set; }
        public decimal Fmonto { get; set; }
        public decimal Fdescuento { get; set; }
        public decimal Fcargos { get; set; }
        public string Fconcepto { get; set; } = null!;
        public string NombreOrigen { get; set; } = string.Empty; // Propiedad para el nombre del origen
        public bool Factivo { get; set; }
    }
}