using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alquileres.Models;

[Table("tb_usuario")]
public partial class TbUsuario
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("fid_usuario")]
    public int FidUsuario { get; set; }

    [Column("identity_id", TypeName = "varchar(450)")]
    [ForeignKey("Id")]
    public string? IdentityId { get; set; }

    [Column("fnombre", TypeName = "varchar(50)")]
    public string Fnombre { get; set; } = null!;

    [Column("fusuario", TypeName = "varchar(20)")]
    public string Fusuario { get; set; } = null!;

    [Column("fnivel")]
    public int Fnivel { get; set; }

    [Column("fpassword", TypeName = "varchar(MAX)")]
    public string Fpassword { get; set; } = null!;

    [Column("factivado")]
    public bool Factivado { get; set; }

    [Column("fkid_sucursal")]
    public int FkidSucursal { get; set; }

    [Column("factivo")]
    public bool Factivo { get; set; }

    [Column("tutorial_visto")]
    public bool FTutorialVisto { get; set; }

    [EmailAddress]
    [Column("femail", TypeName = "varchar(100)")]
    public string Femail { get; set; } = null!;
}