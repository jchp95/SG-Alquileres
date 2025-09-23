using System.ComponentModel.DataAnnotations;

public class RegisterApiRequest
{
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    public string Nombre { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }

    [Required(ErrorMessage = "La contraseña es requerida")]
    [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y como máximo {1} caracteres.", MinimumLength = 2)]
    public string Password { get; set; }

    [Required(ErrorMessage = "La confirmación de contraseña es requerida")]
    [Compare("Password", ErrorMessage = "La contraseña y su confirmación no coinciden.")]
    public string ConfirmPassword { get; set; }
}