using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Alquileres.Models;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace Alquileres.Controllers
{
    public class AdministrarPermisosController : Controller
    {
        private readonly IPermissionService _permissionService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<HomeController> _logger;

        public AdministrarPermisosController(
            IPermissionService permissionService,
            UserManager<IdentityUser> userManager,
            ILogger<HomeController> logger)
        {
            _permissionService = permissionService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _permissionService.GetAllUsersAsync();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserPermissions(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var allPermissions = Permissions.GetPermissionsByCategory();

            // Depuración específica para CxC
            if (allPermissions.TryGetValue("CuentaPorCobrar", out var cxCPermissions))
            {
                Console.WriteLine($"Permisos CxC a enviar: {string.Join(", ", cxCPermissions)}");
            }

            var model = new ManagePermissionsViewModel
            {
                UserId = userId,
                UserName = user.UserName,
                PermissionCategories = await _permissionService.GetCategorizedPermissionsAsync(userId)
            };

            return Ok(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePermissions([FromBody] UpdatePermissionsRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        errors = ModelState.Values
                                  .SelectMany(v => v.Errors)
                                  .Select(e => e.ErrorMessage)
                    });
                }

                var result = await _permissionService.UpdateUserPermissionsAsync(
                    request.UserId,
                    request.SelectedPermissions ?? new List<string>());

                if (!result.Succeeded)
                {
                    return BadRequest(new
                    {
                        success = false,
                        errors = result.Errors.Select(e => e.Description)
                    });
                }

                // Registrar la actualización exitosa
                _logger.LogInformation($"Permisos actualizados para usuario {request.UserId}");

                return Ok(new
                {
                    success = true,
                    updatedCount = request.SelectedPermissions?.Count ?? 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar permisos");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Error interno del servidor"
                });
            }
        }

    }



    public class UpdatePermissionsRequest
    {
        [Required(ErrorMessage = "El ID de usuario es requerido")]
        public string UserId { get; set; }

        public List<string> SelectedPermissions { get; set; } = new List<string>();
    }
}