// Licenciado por .NET Foundation bajo MIT
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Alquileres.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ForgotPasswordModel> _logger;

        public ForgotPasswordModel(
            UserManager<IdentityUser> userManager,
            IEmailSender emailSender,
            ILogger<ForgotPasswordModel> logger)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "El correo electrónico es obligatorio")]
            [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido")]
            [Display(Name = "Correo electrónico")]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);

                // Medida de seguridad: No revelar si el email existe o está confirmado
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    _logger.LogInformation($"Solicitud de restablecimiento para email no registrado: {Input.Email}");
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                try
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                    var resetUrl = Url.Page(
                        "/Account/ResetPassword",
                        pageHandler: null,
                        values: new { area = "Identity", code = token },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(
                        Input.Email,
                        "Restablecer contraseña - Alquileres",
                        $"""
                        <h3>Restablecimiento de contraseña</h3>
                        <p>Para crear una nueva contraseña, <a href='{HtmlEncoder.Default.Encode(resetUrl)}'>haga clic aquí</a>.</p>
                        <p>Si no solicitó este cambio, puede ignorar este mensaje.</p>
                        <p><small>Enlace válido por 24 horas</small></p>
                        """);

                    _logger.LogInformation($"Enlace de restablecimiento enviado a: {Input.Email}");
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al enviar email de restablecimiento");
                    ModelState.AddModelError(string.Empty, "Ocurrió un error al procesar su solicitud.");
                }
            }

            return Page();
        }
    }
}