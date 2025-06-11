// Licenciado por .NET Foundation bajo uno o más acuerdos
// .NET Foundation concede licencia de este archivo bajo la licencia MIT
#nullable disable

using System.ComponentModel.DataAnnotations;
using Alquileres.Context;
using Alquileres.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Alquileres.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _dbContext;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _dbContext = dbContext;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "El nombre de usuario es requerido")]
            [Display(Name = "Nombre de usuario")]
            public string UserName { get; set; }

            [Required(ErrorMessage = "La contraseña es requerida")]
            [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y como máximo {1} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Contraseña")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar contraseña")]
            [Compare("Password", ErrorMessage = "La contraseña y su confirmación no coinciden.")]
            public string ConfirmPassword { get; set; }
        }

        public class NoOpEmailSender : IEmailSender
        {
            public Task SendEmailAsync(string email, string subject, string htmlMessage)
            {
                // Implementación vacía
                return Task.CompletedTask;
            }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.UserName, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Usuario creado exitosamente con contraseña.");

                    // Crear registro en TbUsuario
                    var nuevoUsuario = new TbUsuario
                    {
                        Fnombre = Input.UserName,
                        Fusuario = Input.UserName,
                        Fpassword = user.PasswordHash,
                        Fnivel = 2, // Nivel por defecto para usuarios normales
                        Factivado = true,
                        FkidSucursal = 1, // Ajustar según necesidad
                        FestadoSync = "A",
                        Factivo = true,
                        IdentityId = user.Id
                    };

                    _dbContext.TbUsuarios.Add(nuevoUsuario);
                    await _dbContext.SaveChangesAsync();

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }

        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"No se pudo crear una instancia de '{nameof(IdentityUser)}'. " +
                    $"Asegúrese que '{nameof(IdentityUser)}' no es una clase abstracta y tiene un constructor sin parámetros, " +
                    $"o alternativamente sobrescriba la página de registro en /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("La interfaz de usuario requiere un almacén de usuarios con soporte para email.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}