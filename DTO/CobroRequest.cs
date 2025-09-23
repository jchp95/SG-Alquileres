using Alquileres.Models;

namespace Alquileres.DTO
{
    public class CobroRequest
    {
        public int FkidCxc { get; set; }
        public decimal FmontoCobro { get; set; }
        public decimal Fdescuento { get; set; }
        public decimal Fcargos { get; set; }
        public string Fconcepto { get; set; }
        public List<int> CuotasSeleccionadas { get; set; }
        public int FnumeroCuota { get; set; }
        public decimal FmontoCuota { get; set; }
        public decimal Fmora { get; set; }
        public decimal Fefectivo { get; set; }
        public decimal FmontoRecibido { get; set; }
        public decimal Ftransferencia { get; set; }
        public decimal Ftarjeta { get; set; }
        public decimal FnotaCredito { get; set; }
        public decimal Fcheque { get; set; }
        public decimal Fdeposito { get; set; }
        public decimal FdebitoAutomatico { get; set; }
        public int FnoNotaCredito { get; set; }
        public string ComprobanteFiscalSeleccionado { get; set; }
    }
}