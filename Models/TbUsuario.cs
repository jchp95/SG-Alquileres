using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Alquileres.Models;

public partial class TbUsuario
{
    public int FidUsuario { get; set; }

    public string Fnombre { get; set; } = null!;

    public string Fusuario { get; set; } = null!;

    public int Fnivel { get; set; }

    public string Fpassword { get; set; } = null!;

    public bool Factivado { get; set; }

    public int FkidUsuario { get; set; }

    public int FkidSucursal { get; set; }

    public string FestadoSync { get; set; } = null!;

    public bool Factivo { get; set; }

    [MaxLength(450)]
    public string? IdentityId { get; set; }
    public bool FTutorialVisto { get; set; }


}
