using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alquileres.Models
{
    public class TbCobrosDesglose
    {
        [Key]
        [Column("fid_desglose")]
        public int FidDesglose { get; set; }

        [Column("fkid_cobro")]
        public int FkidCobro { get; set; }

        [Column("fefectivo")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Fefectivo { get; set; }

        [Column("ftransferencia")]
        [DisplayFormat(DataFormatString = "{0:N3}")]
        public decimal Ftransferencia { get; set; }

        [Column("fmonto_recibido")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal FmontoRecibido { get; set; }

        [Column("ftarjeta")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Ftarjeta { get; set; }

        [Column("fnota_credito")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal FnotaCredito { get; set; }

        [Column("fcheque")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Fcheque { get; set; }

        [Column("fdeposito")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Fdeposito { get; set; }

        [Column("fdebito_automatico")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal FdebitoAutomatico { get; set; }

        [Column("fno_nota_credito")]
        public int FnoNotaCredito { get; set; }

        [Column("fkid_usuario")]
        public int FkidUsuario { get; set; }

        [Column("factivo")]
        public bool Factivo { get; set; }

    }
}