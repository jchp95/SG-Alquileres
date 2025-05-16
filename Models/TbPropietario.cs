using System.ComponentModel.DataAnnotations;

namespace Alquileres.Models;

public partial class TbPropietario
{
    public int FidPropietario { get; set; }

    [Display(Name = "Nombre")]
    public string Fnombre { get; set; } = null!;

    [Display(Name = "Apellidos")]
    public string Fapellidos { get; set; } = null!;

    [Display(Name = "Cédula")]
    public string Fcedula { get; set; } = null!;

    [Display(Name = "Dirección")]
    public string Fdireccion { get; set; } = null!;

    [Display(Name = "Teléfono")]
    public string Ftelefono { get; set; } = null!;

    [Display(Name = "Celular")]
    public string Fcelular { get; set; } = null!;

    public bool Factivo { get; set; }


}
