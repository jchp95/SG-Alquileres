using System;
using System.ComponentModel.DataAnnotations;

namespace Alquileres.Models
{
    public class CuotaCreateViewModel
    {
        [Required(ErrorMessage = "La cuenta por cobrar es requerida")]
        [Display(Name = "ID de Cuenta por Cobrar")]
        public int FidCxc { get; set; }

        [Required(ErrorMessage = "El número de cuota es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El número de cuota debe ser mayor a 0")]
        [Display(Name = "Número de Cuota")]
        public int FNumeroCuota { get; set; }

        [Required(ErrorMessage = "La fecha de vencimiento es requerida")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Vencimiento")]
        public DateTime Fvence { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        [Display(Name = "Monto")]
        public decimal Fmonto { get; set; }

        [Required(ErrorMessage = "La tasa de mora es requerida")]
        [Range(0, 100, ErrorMessage = "La tasa de mora debe estar entre 0 y 100")]
        [Display(Name = "Tasa de Mora")]
        public decimal TasaMora { get; set; }

        [Required(ErrorMessage = "La cantidad de cuotas es requerida")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe crear al menos 1 cuota")]
        [Display(Name = "Cantidad de Cuotas")]
        public int CantidadCuotas { get; set; }
    }
}