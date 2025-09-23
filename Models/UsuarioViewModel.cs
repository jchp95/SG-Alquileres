using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alquileres.Models
{
    public class UsuarioViewModel
    {
        public int FidUsuario { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [Display(Name = "Nombre Completo")]
        public string Fnombre { get; set; } = null!;

        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [Display(Name = "Nombre de Usuario")]
        public string Fusuario { get; set; } = null!;

        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido")]
        [Display(Name = "Correo Electrónico")]
        public string Femail { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y como máximo {1} caracteres.", MinimumLength = 3)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Fpassword { get; set; } = null!;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        [Compare("Fpassword", ErrorMessage = "Las contraseñas no coinciden.")]
        public string FRepeatPassword { get; set; } = null!;

        [Display(Name = "Nivel de Acceso")]
        public int Fnivel { get; set; } = 3; // Valor por defecto

        public string? IdentityId { get; set; }
        public bool? Factivado { get; set; }
        public bool? Factivo { get; set; }
        public int? FkidSucursal { get; set; }
        public bool? FTutorialVisto { get; set; }

        [Display(Name = "Rol")]
        [Required(ErrorMessage = "El rol es requerido")]
        public string SelectedRole { get; set; } = null!;

        // Solo para poblar el dropdown, no debe ser requerido ni validado
        public List<SelectListItem> Roles { get; set; } = new List<SelectListItem>();

        // Permisos seleccionados al crear el usuario
        public List<string> SelectedPermissions { get; set; } = new List<string>();
    }
}