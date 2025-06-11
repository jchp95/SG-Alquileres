using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alquileres.Models
{
    public class TbCxcCuotum
    {
        public int FidCuota { get; set; }

        [Required]
        [ForeignKey("CxC")]
        [DisplayName("Cuenta por cobrar")]
        public int FidCxc { get; set; }


        [DisplayName("Número de Cuota")]
        public int FNumeroCuota { get; set; }

        [Required]
        [DisplayName("Fecha vencimiento")]
        public DateTime Fvence { get; set; }

        [Required]
        [DisplayName("Monto de la Cuota")]
        public int Fmonto { get; set; }

        [Required]
        [DisplayName("Saldo de la Cuota")]
        public decimal Fsaldo { get; set; }

        [Required]
        [DisplayName("Mora de la Cuota")]
        public decimal Fmora { get; set; }

        [Required]
        public DateTime FfechaUltCalculo { get; set; }

        [Required]
        [DisplayName("Estado")]
        public char Fstatus { get; set; }

        public bool Factivo { get; set; }


    }
}
