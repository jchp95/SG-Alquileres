// Licenciado por .NET Foundation bajo uno o más acuerdos
// .NET Foundation concede licencia de este archivo bajo la licencia MIT
#nullable disable

using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Alquileres.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _sender;

        public RegisterConfirmationModel(UserManager<IdentityUser> userManager, IEmailSender sender)
        {
            _userManager = userManager;
            _sender = sender;
        }

        /// <summary>
        ///     Esta API soporta la infraestructura de UI por defecto de ASP.NET Core Identity
        ///     y no está diseñada para ser usada directamente desde tu código.
        ///     Esta API puede cambiar o eliminarse en futuras versiones.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        ///     Esta API soporta la infraestructura de UI por defecto de ASP.NET Core Identity
        ///     y no está diseñada para ser usada directamente desde tu código.
        ///     Esta API puede cambiar o eliminarse en futuras versiones.
        /// </summary>
        public bool DisplayConfirmAccountLink { get; set; }

        /// <summary>
        ///     Esta API soporta la infraestructura de UI por defecto de ASP.NET Core Identity
        ///     y no está diseñada para ser usada directamente desde tu código.
        ///     Esta API puede cambiar o eliminarse en futuras versiones.
        /// </summary>
        public string EmailConfirmationUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
        {
            if (email == null)
            {
                return RedirectToPage("/Index");
            }
            returnUrl = returnUrl ?? Url.Content("~/");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"No se pudo encontrar un usuario con el email '{email}'.");
            }

            Email = email;
            // Una vez que agregues un servicio de email real, deberías eliminar este código que permite confirmar la cuenta
            DisplayConfirmAccountLink = true;
            if (DisplayConfirmAccountLink)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                EmailConfirmationUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                    protocol: Request.Scheme);
            }

            return Page();
        }
    }
}