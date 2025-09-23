using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alquileres.Models

{
    [Table("tb_gasto")]
    public partial class TbGasto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("fid_gasto")]
        public int FidGasto { get; set; }

        [ForeignKey("FidGastoTipo")]
        [Column("fkid_gasto_tipo")]
        public int FkidGastoTipo { get; set; }

        [ForeignKey("FidUsuario")]
        [Column("fkid_usuario")]
        public int FkidUsuario { get; set; }

        [Column("fmonto", TypeName = "decimal(10,2)")]
        public decimal Fmonto { get; set; }

        [Column("fdescripcion", TypeName = "varchar(250)")]
        public string Fdescripcion { get; set; } = null!;

        [Column("ffecha", TypeName = "date")]
        public DateOnly Ffecha { get; set; }

        [Column("factivo")]
        public bool Factivo { get; set; }
    }
}
