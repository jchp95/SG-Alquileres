using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Alquileres.Services;
using Alquileres.Context;
using Alquileres.Models;
using Microsoft.AspNetCore.Authorization;
using Alquileres.Interfaces;

namespace Alquileres.Controllers
{
    [ApiController]
    [IgnoreAntiforgeryToken]
    [Route("api/[controller]")]
    public class AuthApiController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<AuthApiController> _logger;
        private readonly GeneradorDeCuotasService _cuotasService;
        private readonly ChequeaCxCVencidasServices _cxCService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IPermissionService _permissionService;

        public AuthApiController(
            ApplicationDbContext dbContext,
            SignInManager<IdentityUser> signInManager,
            ILogger<AuthApiController> logger,
            GeneradorDeCuotasService cuotasService,
            ChequeaCxCVencidasServices cxCService,
            UserManager<IdentityUser> userManager,
            ITokenService tokenService,
            IPermissionService permissionService)
        {
            _dbContext = dbContext;
            _signInManager = signInManager;
            _logger = logger;
            _cuotasService = cuotasService;
            _cxCService = cxCService;
            _userManager = userManager;
            _tokenService = tokenService;
            _permissionService = permissionService;
        }

        // Login.
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginApiRequest request)
        {
            if (request == null)
                return BadRequest(new { Message = "Request body cannot be empty" });

            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user == null)
                return Unauthorized(new { Message = "Usuario o contraseña incorrectos" });

            var result = await _signInManager.PasswordSignInAsync(
                request.UserName,
                request.Password,
                request.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);

                // ✅ Ahora pasamos permisos al token
                var token = _tokenService.GenerateToken(user, roles, permissions);

                try
                {
                    await _cuotasService.VerificarCuotasVencidasAsync();
                    await _cxCService.VerificarCxCVencidasAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al verificar cuotas vencidas después del login");
                }

                return Ok(new
                {
                    Token = token,
                    UserName = user.UserName,
                    Roles = roles,
                    Permissions = permissions,
                    Message = "Inicio de sesión exitoso"
                });
            }

            return Unauthorized(new { Message = "Usuario o contraseña incorrectos" });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Usuario cerró sesión.");
            return Ok(new { Message = "Sesión cerrada correctamente." });
        }

        // - Register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterApiRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new IdentityUser { UserName = request.UserName };
            if (string.IsNullOrWhiteSpace(request.UserName))
            {
                return BadRequest(new { Message = "El nombre de usuario no puede estar vacío." });
            }

            _logger.LogInformation("RegisterRequest recibido: " + JsonSerializer.Serialize(request));

            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("Usuario creado exitosamente con contraseña.");

                // Crear registro en TbUsuario
                var nuevoUsuario = new TbUsuario
                {
                    Fnombre = request.UserName,
                    Fusuario = request.UserName,
                    Femail = request.Email,
                    Fpassword = user.PasswordHash,
                    Fnivel = 2, // Nivel por defecto para usuarios normales
                    Factivado = true,
                    FkidSucursal = 1, // Ajustar según necesidad
                    Factivo = true,
                    IdentityId = user.Id
                };

                _dbContext.TbUsuarios.Add(nuevoUsuario);
                await _dbContext.SaveChangesAsync();

                // Opcional: Iniciar sesión automáticamente después del registro
                await _signInManager.SignInAsync(user, isPersistent: false);

                // Generar token JWT para el nuevo usuario
                var roles = await _userManager.GetRolesAsync(user);
                var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);
                var token = _tokenService.GenerateToken(user, roles, permissions);

                return Ok(new
                {
                    Token = token,
                    UserName = user.UserName,
                    Message = "Registro exitoso"
                });
            }

            return BadRequest(result.Errors);
        }

        // - ForgotPassword
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordApiRequest request)
        {
            _logger.LogInformation($"Intento de cambio de contraseña recibido");

            if (request == null)
            {
                _logger.LogWarning("Request body is null");
                return BadRequest(new { Message = "Request body cannot be empty" });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                _logger.LogWarning($"Model validation errors: {JsonSerializer.Serialize(errors)}");
                return BadRequest(new
                {
                    Message = "Validation failed",
                    Errors = errors
                });
            }

            // Obtener el usuario actual
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(new { Message = "Usuario no autenticado" });
            }

            // Verificar si el usuario tiene contraseña
            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (!hasPassword)
            {
                return BadRequest(new { Message = "El usuario no tiene contraseña establecida" });
            }

            // Cambiar la contraseña
            var changePasswordResult = await _userManager.ChangePasswordAsync(
                user,
                request.OldPassword,
                request.NewPassword);

            if (!changePasswordResult.Succeeded)
            {
                _logger.LogWarning($"Error al cambiar contraseña: {JsonSerializer.Serialize(changePasswordResult.Errors)}");

                var errorMessages = changePasswordResult.Errors.Select(e => e.Description).ToArray();
                return BadRequest(new
                {
                    Message = "Error al cambiar la contraseña",
                    Errors = errorMessages
                });
            }

            // Actualizar la sesión
            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation($"Usuario {user.UserName} cambió su contraseña exitosamente");

            return Ok(new { Message = "Contraseña cambiada exitosamente" });
        }

        // agregar más endpoints según necesite:
        // - RefreshToken
    }
}