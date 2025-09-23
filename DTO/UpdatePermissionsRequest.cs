using System.ComponentModel.DataAnnotations;

public class UpdatePermissionsRequest
{
    [Required(ErrorMessage = "El ID de usuario es requerido")]
    public string UserId { get; set; } = null!;

    public List<string> SelectedPermissions { get; set; } = new List<string>();
}