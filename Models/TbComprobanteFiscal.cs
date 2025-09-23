using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alquileres.Models
{
    [Table("tb_comprobante_fiscal")]
    public class TbComprobanteFiscal
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("fid_comprobante")]
        public int FidComprobante { get; set; }

        [ForeignKey("FidEmpresa")]
        [Column("fkid_empresa")]
        public int? FkidEmpresa { get; set; }

        [Column("fid_tipo_comprobante")]
        public int? FidTipoComprobante { get; set; }

        [Column("fprefijo", TypeName = "varchar(3)")]
        public string Fprefijo { get; set; } = null!;

        [Column("finicia")]
        public int? Finicia { get; set; }

        [Column("ffinaliza")]
        public int? Ffinaliza { get; set; }

        [Column("fcontador")]
        public int? Fcontador { get; set; }

        [Column("fcomprobante", TypeName = "varchar(200)")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string Fcomprobante { get; set; }

        [Column("fvence", TypeName = "Date")]
        public DateTime? Fvence { get; set; } = DateTime.Now;

        [ForeignKey("FidUsuario")]
        [Column("fkid_usuario")]
        public int FkidUsuario { get; set; }

        [Column("ftipo_comprobante", TypeName = "varchar(50)")]
        public string FtipoComprobante { get; set; } = null!;

        [Column("festado_sync", TypeName = "varchar(1)")]
        [Required]
        public string FestadoSync { get; set; } = "S";
    }
}