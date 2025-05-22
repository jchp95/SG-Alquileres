namespace Alquileres.Models
{
    public class DetallesCobroViewModel
    {
        public TbCobro Cobro { get; set; }
        public List<TbCobrosDetalle> Detalles { get; set; }
        public TbCobrosDesglose Desglose { get; set; }
        public TbCxc Cxc { get; set; }
        public TbInquilino Inquilino { get; set; }

        public TbInmueble Inmueble { get; set; }
        public List<TbCxcCuotum> Cuotas { get; set; }

        // Propiedades calculadas
        public decimal TotalPagos => Desglose?.Fefectivo + Desglose?.Ftransferencia + Desglose?.Ftarjeta +
                                   Desglose?.FnotaCredito + Desglose?.Fcheque + Desglose?.Fdeposito +
                                   Desglose?.FdebitoAutomatico ?? 0;
    }
}