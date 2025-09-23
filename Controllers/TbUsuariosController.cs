using Microsoft.AspNetCore.Mvc;
using Alquileres.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Alquileres.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alquileres.Controllers
{
    public class TbUsuariosController : BaseController
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<TbUsuariosController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly RoleManager<IdentityRole> _roleManager;

        public TbUsuariosController(
            ApplicationDbContext context,
            IWebHostEnvironment env,
            ILogger<TbUsuariosController> logger,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IUserStore<IdentityUser> userStore) : base(context)
        {
            _env = env;
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
        }

        [Authorize(Policy = "Permissions.Usuario.Ver")]
        public async Task<IActionResult> Index()
        {
            var usuarios = await _context.TbUsuarios.ToListAsync();
            return View(usuarios);
        }

        [HttpPost]
        [Authorize(Policy = "Permissions.Usuario.Anular")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var usuario = await _context.TbUsuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            // Cambiar el estado del usuario
            usuario.Factivo = !usuario.Factivo;
            _context.Update(usuario);
            await _context.SaveChangesAsync();

            // Devolver la vista parcial actualizada con roles
            var usuariosSistema = await _userManager.Users.ToListAsync();
            var usuarios = await _context.TbUsuarios.OrderBy(i => i.FidUsuario).ToListAsync();

            var usuariosVm = new List<UsuarioViewModel>();
            foreach (var u in usuarios)
            {
                var identityUser = usuariosSistema.FirstOrDefault(us => us.UserName == u.Fusuario);
                string rol = string.Empty;
                if (identityUser != null)
                {
                    var roles = await _userManager.GetRolesAsync(identityUser);
                    rol = roles.FirstOrDefault() ?? string.Empty;
                }
                usuariosVm.Add(new UsuarioViewModel
                {
                    FidUsuario = u.FidUsuario,
                    Fnombre = u.Fnombre,
                    Fusuario = u.Fusuario,
                    Femail = u.Femail,
                    Factivo = u.Factivo,
                    IdentityId = identityUser?.Id,
                    SelectedRole = rol
                });
            }

            return PartialView("_UsuarioPartial", usuariosVm);
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Usuario.Ver")]
        public async Task<IActionResult> CargarUsuarios()
        {
            var usuariosSistema = await _userManager.Users.ToListAsync();
            var usuarios = await _context.TbUsuarios.OrderBy(i => i.FidUsuario).ToListAsync();

            var usuariosVm = new List<UsuarioViewModel>();
            foreach (var u in usuarios)
            {
                var identityUser = usuariosSistema.FirstOrDefault(us => us.UserName == u.Fusuario);
                string rol = string.Empty;
                if (identityUser != null)
                {
                    var roles = await _userManager.GetRolesAsync(identityUser);
                    rol = roles.FirstOrDefault() ?? string.Empty;
                }
                usuariosVm.Add(new UsuarioViewModel
                {
                    FidUsuario = u.FidUsuario,
                    Fnombre = u.Fnombre,
                    Fusuario = u.Fusuario,
                    Femail = u.Femail,
                    Factivo = u.Factivo,
                    IdentityId = identityUser?.Id,
                    SelectedRole = rol
                });
            }

            return PartialView("_UsuarioPartial", usuariosVm);
        }

        [Authorize(Policy = "Permissions.Usuario.Crear")]
        public async Task<IActionResult> Create()
        {
            var viewModel = new UsuarioViewModel();
            ViewBag.Roles = await GetRolesSelectList();

            // Devuelve la vista parcial en lugar de la vista completa
            return PartialView("_CreateUsuarioPartial", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permissions.Usuario.Crear")]
        public async Task<IActionResult> Create([Bind("FidUsuario,Fnombre,Fusuario,Femail,Fpassword,FRepeatPassword,SelectedRole,SelectedPermissions")] UsuarioViewModel viewModel)
        {
            try
            {
                ModelState.Remove("IdentityId");
                ModelState.Remove("Factivado");
                ModelState.Remove("Factivo");
                ModelState.Remove("FkidSucursal");
                ModelState.Remove("FTutorialVisto");

                // Validar modelo
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    );
                    return BadRequest(new { success = false, errors });
                }

                // Validar que se seleccionó un rol
                if (string.IsNullOrEmpty(viewModel.SelectedRole))
                {
                    ModelState.AddModelError("SelectedRole", "Debe seleccionar un rol");
                    var errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    );
                    return BadRequest(new { success = false, errors });
                }

                // Verificar si el nombre de usuario ya existe en Identity
                var existingIdentityUser = await _userManager.FindByNameAsync(viewModel.Fusuario);
                if (existingIdentityUser != null)
                {
                    ModelState.AddModelError("Fusuario", "Este nombre de usuario ya está registrado");
                    var errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    );
                    return BadRequest(new { success = false, errors });
                }

                // Verificar si el email ya existe en Identity
                var existingEmailUser = await _userManager.FindByEmailAsync(viewModel.Femail);
                if (existingEmailUser != null)
                {
                    ModelState.AddModelError("Femail", "Este correo electrónico ya está registrado");
                    var errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    );
                    return BadRequest(new { success = false, errors });
                }

                // Crear usuario en Identity
                var identityUser = new IdentityUser
                {
                    UserName = viewModel.Fusuario,
                    Email = viewModel.Femail,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(identityUser, viewModel.Fpassword);

                if (!createResult.Succeeded)
                {
                    foreach (var error in createResult.Errors)
                    {
                        ModelState.AddModelError("General", error.Description);
                    }
                    var errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    );
                    return BadRequest(new { success = false, errors });
                }

                // Asignar el rol seleccionado al usuario
                var roleResult = await _userManager.AddToRoleAsync(identityUser, viewModel.SelectedRole);
                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                    {
                        ModelState.AddModelError("General", error.Description);
                    }
                    var errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    );
                    return BadRequest(new { success = false, errors });
                }

                // Recuperar el usuario con su PasswordHash actualizado
                var identityCreated = await _userManager.FindByNameAsync(viewModel.Fusuario);

                // Determinar el nivel basado en el rol
                int nivel = viewModel.SelectedRole switch
                {
                    "Administrator" => 1,
                    "Gerente" => 2,
                    "Usuario" => 3,
                    _ => 3 // Por defecto usuario
                };

                // Crear registro en TbUsuario
                var tbUsuario = new TbUsuario
                {
                    Fnombre = viewModel.Fnombre,
                    Fusuario = viewModel.Fusuario,
                    Femail = viewModel.Femail,
                    Fpassword = identityCreated?.PasswordHash ?? string.Empty,
                    Fnivel = nivel,
                    FkidSucursal = 1,
                    Factivado = true,
                    Factivo = true,
                    IdentityId = identityCreated?.Id ?? string.Empty,
                    FTutorialVisto = false
                };

                _context.TbUsuarios.Add(tbUsuario);
                await _context.SaveChangesAsync();

                // Asignar permisos seleccionados si hay alguno
                if (viewModel.SelectedPermissions != null && viewModel.SelectedPermissions.Any())
                {
                    foreach (var perm in viewModel.SelectedPermissions)
                    {
                        await _userManager.AddClaimAsync(identityCreated, new System.Security.Claims.Claim("Permission", perm));
                    }
                }

                // 🚀 Éxito → devolver success y el IdentityId para asignar permisos
                return Ok(new { success = true, userId = identityCreated?.Id });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error de base de datos al crear usuario");
                return StatusCode(500, new { success = false, errors = new { General = new[] { "Error crítico en la base de datos. Contacte al administrador." } } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error general al crear usuario: {Message}", ex.Message);
                return StatusCode(500, new { success = false, errors = new { General = new[] { "Error interno del servidor." } } });
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Usuario.Editar")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tbUsuario = await _context.TbUsuarios.FindAsync(id);
            if (tbUsuario == null)
            {
                return NotFound();
            }

            UsuarioViewModel viewModel;
            // Obtener el usuario de Identity para saber su rol actual
            IdentityUser identityUser = null;
            if (!string.IsNullOrEmpty(tbUsuario.IdentityId))
            {
                identityUser = await _userManager.FindByIdAsync(tbUsuario.IdentityId);
            }
            if (identityUser != null)
            {
                var roles = await _userManager.GetRolesAsync(identityUser);
                viewModel = new UsuarioViewModel
                {
                    FidUsuario = tbUsuario.FidUsuario,
                    Fnombre = tbUsuario.Fnombre,
                    Fusuario = tbUsuario.Fusuario,
                    Femail = tbUsuario.Femail,
                    SelectedRole = roles.FirstOrDefault()
                };
            }
            else
            {
                viewModel = new UsuarioViewModel
                {
                    FidUsuario = tbUsuario.FidUsuario,
                    Fnombre = tbUsuario.Fnombre,
                    Fusuario = tbUsuario.Fusuario,
                    Femail = tbUsuario.Femail
                };
            }

            ViewBag.Roles = await GetRolesSelectList();
            return PartialView("_EditUsuarioPartial", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permissions.Usuario.Editar")]
        public async Task<IActionResult> Edit([Bind("FidUsuario,Fnombre,Fusuario,Femail,SelectedRole")] UsuarioViewModel viewModel)
        {
            try
            {
                // Remover validaciones innecesarias
                ModelState.Remove("Fpassword");
                ModelState.Remove("FRepeatPassword");
                ModelState.Remove("IdentityId");
                ModelState.Remove("Factivado");
                ModelState.Remove("Factivo");
                ModelState.Remove("FkidSucursal");
                ModelState.Remove("FTutorialVisto");

                if (!ModelState.IsValid)
                {
                    ViewBag.Roles = await GetRolesSelectList();
                    var errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                    return BadRequest(new { success = false, errors });
                }

                var usuarioExistente = await _context.TbUsuarios.FirstOrDefaultAsync(u => u.FidUsuario == viewModel.FidUsuario);
                if (usuarioExistente == null)
                {
                    return NotFound();
                }

                // Validar duplicados SOLO si el valor cambió
                if (viewModel.Fusuario != usuarioExistente.Fusuario)
                {
                    var usuarioDuplicado = await _context.TbUsuarios
                        .AnyAsync(u => u.Fusuario == viewModel.Fusuario && u.FidUsuario != viewModel.FidUsuario);
                    if (usuarioDuplicado)
                    {
                        ViewBag.Roles = await GetRolesSelectList();
                        return BadRequest(new
                        {
                            success = false,
                            errors = new { Fusuario = new[] { "Este nombre de usuario ya está registrado por otro usuario." } }
                        });
                    }
                }

                if (viewModel.Femail != usuarioExistente.Femail)
                {
                    var emailDuplicado = await _context.TbUsuarios
                        .AnyAsync(u => u.Femail == viewModel.Femail && u.FidUsuario != viewModel.FidUsuario);
                    if (emailDuplicado)
                    {
                        ViewBag.Roles = await GetRolesSelectList();
                        return BadRequest(new
                        {
                            success = false,
                            errors = new { Femail = new[] { "Este correo electrónico ya está registrado por otro usuario." } }
                        });
                    }
                }

                // Actualizar datos en TbUsuario
                usuarioExistente.Fnombre = viewModel.Fnombre;
                usuarioExistente.Fusuario = viewModel.Fusuario;
                usuarioExistente.Femail = viewModel.Femail;

                _context.Update(usuarioExistente);
                await _context.SaveChangesAsync();

                // Actualizar rol en Identity si se seleccionó uno
                // Actualizar rol en Identity si se seleccionó uno y existe IdentityId
                if (!string.IsNullOrEmpty(viewModel.SelectedRole) && !string.IsNullOrEmpty(usuarioExistente.IdentityId))
                {
                    var identityUser = await _userManager.FindByIdAsync(usuarioExistente.IdentityId);
                    if (identityUser != null)
                    {
                        // Remover roles existentes
                        var currentRoles = await _userManager.GetRolesAsync(identityUser);
                        await _userManager.RemoveFromRolesAsync(identityUser, currentRoles);

                        // Agregar nuevo rol
                        await _userManager.AddToRoleAsync(identityUser, viewModel.SelectedRole);

                        // Actualizar nivel en TbUsuario según el rol
                        int nivel = viewModel.SelectedRole switch
                        {
                            "Administrator" => 1,
                            "Gerente" => 2,
                            "Usuario" => 3,
                            _ => usuarioExistente.Fnivel
                        };

                        usuarioExistente.Fnivel = nivel;
                        _context.Update(usuarioExistente);
                        await _context.SaveChangesAsync();
                    }
                }

                // Reutilizar la lógica de CargarUsuarios para garantizar consistencia
                return await CargarUsuarios();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error de base de datos al editar usuario");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error crítico en la base de datos. Contacte al administrador."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error general al editar usuario: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"No se pudo crear una instancia de '{nameof(IdentityUser)}'.");
            }
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("Este sistema requiere un almacén de usuarios con soporte para email.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }

        [HttpGet]
        public async Task<IActionResult> VerificarUsuario(string usuario, int? id)
        {
            var usuarioExistente = await _context.TbUsuarios
                .FirstOrDefaultAsync(u => u.Fusuario == usuario);

            if (usuarioExistente == null || usuarioExistente.FidUsuario == id)
            {
                return Json(true);
            }

            return Json("Este nombre de usuario ya está registrado");
        }

        [HttpGet]
        public async Task<IActionResult> VerificarEmail(string email, int? id)
        {
            var emailExistente = await _context.TbUsuarios
                .FirstOrDefaultAsync(u => u.Femail == email);

            if (emailExistente == null || emailExistente.FidUsuario == id)
            {
                return Json(true);
            }

            return Json("Este correo electrónico ya está registrado");
        }

        private async Task<SelectList> GetRolesSelectList()
        {
            // Obtener roles desde Identity
            var roles = await _roleManager.Roles
                .Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                })
                .ToListAsync();

            return new SelectList(roles, "Value", "Text");
        }
    }
}