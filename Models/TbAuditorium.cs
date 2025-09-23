using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alquileres.Models;

[Table("tb_auditoria")]
public partial class TbAuditorium
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("fid_auditoria")]
    public int FidAuditoria { get; set; }

    [ForeignKey("FidUsuario")]
    [Column("fkid_usuario")]
    public int FkidUsuario { get; set; }

    [Column("fkid_registro")]
    public int FkidRegistro { get; set; }

    [Column("ftabla", TypeName = "varchar(50)")]
    public string Ftabla { get; set; } = null!;

    [Column("ffecha", TypeName = "Date")]
    public DateTime Ffecha { get; set; }

    [Column("fhora")]
    public string Fhora { get; set; }

    [Column("faccion", TypeName = "varchar(50)")]
    public string Faccion { get; set; } = null!;

}
