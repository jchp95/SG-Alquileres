using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Alquileres.Models;

public partial class TbInmueble
{
    public int FidInmueble { get; set; }

    public int FkidPropietario { get; set; }

    [Display(Name = "Descripción")]
    public string Fdescripcion { get; set; } = null!;

    [Display(Name = "Dirección")]
    public string Fdireccion { get; set; } = null!;

    [Display(Name = "Ubicación")]
    public string Fubicacion { get; set; } = null!;

    [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
    [Display(Name = "Precio")]
    public decimal Fprecio { get; set; }

    public bool Factivo { get; set; }

}
