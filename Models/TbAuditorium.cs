using System;
using System.Collections.Generic;

namespace Alquileres.Models;

public partial class TbAuditorium
{
    public int Fid { get; set; }

    public string Ftabla { get; set; } = null!;

    public int FkidRegistro { get; set; }

    public DateTime Ffecha { get; set; }

    public string Fhora { get; set; } = null!;

    public string Faccion { get; set; } = null!;

    public int FkidUsuario { get; set; }
}
