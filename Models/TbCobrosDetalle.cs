using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alquileres.Models;

[Table("tb_cobros_detalle")]
public partial class TbCobrosDetalle
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("fid_cobro_detalle")]
    public int FidCobroDetalle { get; set; }

    [ForeignKey("FidCobro")]
    [Column("fkid_cobro")]
    public int FkidCobro { get; set; }

    [Column("fnumero_cuota")]
    public int FnumeroCuota { get; set; }

    [Column("fmonto", TypeName = "decimal(10,2)")]
    public decimal Fmonto { get; set; }

    [Column("fmora", TypeName = "decimal(10,2)")]
    public decimal Fmora { get; set; }

    [Column("factivo")]
    public bool Factivo { get; set; }
}
