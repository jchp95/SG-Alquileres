using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Alquileres.Areas.Identity.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<ResetPasswordModel> _logger;

        public ResetPasswordModel(
            UserManager<IdentityUser> userManager,
            ILogger<ResetPasswordModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "El correo electrónico es requerido")]
            [EmailAddress(ErrorMessage = "Por favor ingrese un correo electrónico válido")]
            public string Email { get; set; }

            [Required(ErrorMessage = "La contraseña es requerida")]
            [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y como máximo {1} caracteres.", MinimumLength = 3)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar contraseña")]
            [Compare("Password", ErrorMessage = "La contraseña y la confirmación no coinciden.")]
            public string ConfirmPassword { get; set; }

            [Required(ErrorMessage = "El código de verificación es requerido")]
            public string Code { get; set; }
        }

        public IActionResult OnGet(string token = null)
        {
            var code = Request.Query["code"];

            _logger.LogInformation($"Token recibido (token): {token}");
            _logger.LogInformation($"Token recibido (query code): {code}");

            if (string.IsNullOrEmpty(token) && string.IsNullOrEmpty(code))
            {
                return BadRequest("Se requiere un token para restablecer la contraseña.");
            }

            try
            {
                Input = new InputModel
                {
                    Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token ?? code))
                };
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al decodificar el token");
                return BadRequest("Token inválido o corrupto.");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                // No revelar que el usuario no existe
                TempData["ResetPasswordSuccess"] = "Si el correo electrónico existe en nuestro sistema, recibirás un enlace para restablecer tu contraseña.";
                return Page();
            }

            var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);
            if (result.Succeeded)
            {
                // Invalidar cualquier token existente
                await _userManager.UpdateSecurityStampAsync(user);

                TempData["ResetPasswordSuccess"] = "Tu contraseña ha sido restablecida exitosamente.";
                return Page();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }
    }
}