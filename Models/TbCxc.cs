using System.ComponentModel.DataAnnotations;

namespace Alquileres.Models;

public partial class TbCxc
{
    public int FidCuenta { get; set; }

    [Display(Name = "Inquilino")]
    public int? FidInquilino { get; set; }  // Hacer nullable

    [Display(Name = "Inmueble")]
    public int? FkidInmueble { get; set; }  // Hacer nullable

    [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
    [Display(Name = "Fecha de Inicio")]
    public DateTime FfechaInicio { get; set; }

    [Required(ErrorMessage = "El monto es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    [Display(Name = "Monto")]
    public decimal Fmonto { get; set; }

    public int FkidUsuario { get; set; }

    [Display(Name = "Días de Gracia")]
    public int FdiasGracia { get; set; }

    [Display(Name = "Tasa Mora %")]
    public decimal FtasaMora { get; set; }

    [Display(Name = "Fecha próxima a la Cuota")]
    public DateTime FfechaProxCuota { get; set; }

    [Display(Name = "Período de Pago")]
    public int FidPeriodoPago { get; set; }

    [Display(Name = "Notas")]
    public string Fnota { get; set; } = null!;

    public bool Factivo { get; set; }

}
