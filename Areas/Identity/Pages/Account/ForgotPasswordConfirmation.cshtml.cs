// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Alquileres.Areas.Identity.Pages.Account
{
    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [AllowAnonymous]
    public class ForgotPasswordConfirmation : PageModel
    {
        private readonly ILogger<ForgotPasswordConfirmation> _logger;

        public ForgotPasswordConfirmation(ILogger<ForgotPasswordConfirmation> logger)
        {
            _logger = logger;
        }

        [TempData]
        public string DebugResetLink { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Email { get; set; }

        public void OnGet()
        {
            // Opcional: Puedes extraer el enlace de los logs si necesitas
        }
    }
}
