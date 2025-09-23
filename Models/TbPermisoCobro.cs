using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alquileres.Models;

[Table("tb_permiso_cobro")]
public partial class TbPermisoCobro
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("fid_permiso_cobro")]
    public int FidPermisoCobro { get; set; }

    [ForeignKey("FidUsuario")]
    [Column("fkid_usuario")]
    public int FkidUsuario { get; set; }
}
