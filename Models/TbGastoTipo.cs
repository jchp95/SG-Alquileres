using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alquileres.Models
{
    [Table("tb_gasto_tipo")]
    public partial class TbGastoTipo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("fid_gasto_tipo")]
        public int FidGastoTipo { get; set; }

        [ForeignKey("FidUsuario")]
        [Column("fkid_usuario")]
        public int FkidUsuario { get; set; }

        [Column("fmonto", TypeName = "decimal(10,2)")]
        public decimal Fmonto { get; set; }

        [Column("fdescripcion", TypeName = "varchar(250)")]
        [Required(ErrorMessage = "La descripción es obligatoria")]
        public string Fdescripcion { get; set; } = null!;

        [Column("factivo")]
        public bool Factivo { get; set; }
    }
}