using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alquileres.Models
{
    [Table("tb_cxc_cuota")]
    public class TbCxcCuotum
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("fid_cuota")]
        public int FidCuota { get; set; }

        [Required]
        [ForeignKey("FidCuenta")]
        [Column("fkid_cxc")]
        [DisplayName("Cuenta por cobrar")]
        public int FkidCxc { get; set; }

        [Column("fnumero_cuota")]
        [DisplayName("Número de Cuota")]
        public int FNumeroCuota { get; set; }

        [Required]
        [Column("fvence", TypeName = "Date")]
        [DisplayName("Fecha vencimiento")]
        public DateTime Fvence { get; set; }

        [Required]
        [Column("fmonto", TypeName = "decimal(10,2)")]
        [DisplayName("Monto de la Cuota")]
        public decimal Fmonto { get; set; }

        [Required]
        [Column("fsaldo", TypeName = "decimal(10,2)")]
        [DisplayName("Saldo de la Cuota")]
        public decimal Fsaldo { get; set; }

        [Required]
        [DefaultValue(0)]
        [Column("fmora", TypeName = "decimal(10,2)")]
        [DisplayName("Mora de la Cuota")]
        public decimal Fmora { get; set; }

        [Required]
        [Column("ffecha_ult_calculo", TypeName = "Date")]
        public DateTime FfechaUltCalculo { get; set; }

        [Column("fdias_atraso")]
        [DisplayName("Dias de atrasos")]
        public int FdiasAtraso { get; set; }

        [Required]
        [Column("fstatus", TypeName = "varchar(1)")]
        [DisplayName("Estado")]
        public char Fstatus { get; set; }

        [Column("factivo")]
        public bool Factivo { get; set; }


    }
}
