using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alquileres.Models
{

    [Table("tb_cobros_desglose")]
    public class TbCobrosDesglose
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("fid_desglose")]
        public int FidDesglose { get; set; }

        [ForeignKey("FidCobro")]
        [Column("fkid_cobro")]
        public int FkidCobro { get; set; }

        [Column("fefectivo", TypeName = "decimal(10,2)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Fefectivo { get; set; }

        [Column("ftransferencia", TypeName = "decimal(10,2)")]
        [DisplayFormat(DataFormatString = "{0:N3}")]
        public decimal Ftransferencia { get; set; }

        [Column("fmonto_recibido", TypeName = "decimal(10,2)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal FmontoRecibido { get; set; }

        [Column("ftarjeta", TypeName = "decimal(10,2)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Ftarjeta { get; set; }

        [Column("fnota_credito", TypeName = "decimal(10,2)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal FnotaCredito { get; set; }

        [Column("fcheque", TypeName = "decimal(10,2)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Fcheque { get; set; }

        [Column("fdeposito", TypeName = "decimal(10,2)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Fdeposito { get; set; }

        [Column("fdebito_automatico", TypeName = "decimal(10,2)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal FdebitoAutomatico { get; set; }

        [Column("fno_nota_credito")]
        public int FnoNotaCredito { get; set; }

        [ForeignKey("FidUsuario")]
        [Column("fkid_usuario")]
        public int FkidUsuario { get; set; }

        [Column("factivo")]
        public bool Factivo { get; set; }

    }
}