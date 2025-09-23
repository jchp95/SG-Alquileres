using System.Globalization;
using System.Security.Claims;
using Alquileres.Context;
using Alquileres.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Alquileres.Controllers
{
    [Authorize]
    public class TbGastosController : BaseController
    {

        private readonly ILogger<TbGastosController> _logger;

        private readonly UserManager<IdentityUser> _userManager;

        // Corrección: Usar el tipo correcto para el logger
        public TbGastosController(ApplicationDbContext context,
        ILogger<TbGastosController> logger,
        UserManager<IdentityUser> userManager) : base(context)
        {
            _logger = logger;
            _userManager = userManager;
        }


        // GET: TbGastos
        public IActionResult Index(string vista = "crear")
        {
            ViewData["Vista"] = vista;
            return View();
        }

        // GET: TbGastos/Cargar
        [HttpGet]
        public async Task<IActionResult> CargarGastoTipo()
        {
            var gastosTipos = await _context.TbGastoTipos.ToListAsync();
            return PartialView("_CreateGastoTipoPartial", gastosTipos); // Devuelve la vista parcial
        }

        // GET: TbGastos/Create
        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_CreateGastoTipoPartial", new List<TbGastoTipo>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TbGastoTipo tbGastoTipo)
        {
            try
            {
                // Debugging - log all permissions
                var claims = User.Claims.Where(c => c.Type == "Permission").ToList();
                _logger.LogInformation("User  permissions: {@Permissions}", claims.Select(c => c.Value));

                var identityId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(identityId))
                    return BadRequest(new { success = false, message = "El usuario no está autenticado." });

                var usuario = await _context.TbUsuarios.FirstOrDefaultAsync(u => u.IdentityId == identityId);
                if (usuario == null)
                    return BadRequest(new { success = false, message = $"No se encontró un usuario con el IdentityId: {identityId}" });

                if (!ModelState.IsValid)
                {
                    // Devuelve errores de validación
                    var errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                    return Json(new { success = false, errors });
                }

                tbGastoTipo.FkidUsuario = usuario.FidUsuario;
                tbGastoTipo.Factivo = true;
                _context.Add(tbGastoTipo);
                await _context.SaveChangesAsync();

                var gastosTipos = await _context.TbGastoTipos.ToListAsync();
                return PartialView("_CreateGastoTipoPartial", gastosTipos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear tipo de gasto");
                return Json(new { success = false, message = "Error al guardar el tipo de gasto" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var tipoGasto = await _context.TbGastoTipos.FindAsync(id);
                if (tipoGasto == null)
                {
                    return NotFound(new { success = false, message = "Tipo de gasto no encontrado" });
                }
                return Json(new { success = true, data = tipoGasto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tipo de gasto para editar");
                return StatusCode(500, new { success = false, message = "Error al obtener el tipo de gasto" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, [FromForm] string Fdescripcion)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Fdescripcion))
                {
                    return BadRequest(new { success = false, message = "La descripción es requerida" });
                }

                var tipoGasto = await _context.TbGastoTipos.FindAsync(id);
                if (tipoGasto == null)
                {
                    return NotFound(new { success = false, message = "Tipo de gasto no encontrado" });
                }

                tipoGasto.Fdescripcion = Fdescripcion;
                _context.Update(tipoGasto);
                await _context.SaveChangesAsync();

                var gastosTipos = await _context.TbGastoTipos.ToListAsync();
                return PartialView("_CreateGastoTipoPartial", gastosTipos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar tipo de gasto");
                return StatusCode(500, new { success = false, message = "Error al actualizar el tipo de gasto" });
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Gastos.Ver")]
        public async Task<IActionResult> CargarGasto()
        {
            try
            {
                // Get types for the dropdown
                ViewBag.TiposGasto = await _context.TbGastoTipos.ToListAsync();

                // Get expenses with types and user information
                var gastosConTipos = await _context.TbGastos
                    .Join(_context.TbGastoTipos,
                        gasto => gasto.FkidGastoTipo,
                        tipo => tipo.FidGastoTipo,
                        (gasto, tipo) => new
                        {
                            Gasto = gasto,
                            Tipo = tipo,
                            Usuario = _context.TbUsuarios.FirstOrDefault(u => u.FidUsuario == gasto.FkidUsuario)
                        })
                    .Select(x => new GastoViewModel
                    {
                        FidGasto = x.Gasto.FidGasto,
                        FkidGastoTipo = x.Tipo.FidGastoTipo,
                        TipoGasto = x.Tipo.Fdescripcion,
                        Fmonto = x.Gasto.Fmonto,
                        Ffecha = x.Gasto.Ffecha,
                        Fdescripcion = x.Gasto.Fdescripcion,
                        NombreUsuario = x.Usuario != null ? x.Usuario.Fusuario : "Desconocido",
                        Factivo = x.Gasto.Factivo
                    })
                    .ToListAsync();

                return PartialView("_ConsultaPartial", gastosConTipos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar gastos");
                return StatusCode(500, "Error interno al cargar los gastos");
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Gastos.Crear")]
        public async Task<IActionResult> CreateGasto()
        {
            // Cargar los tipos de gasto activos
            var tiposGasto = await _context.TbGastoTipos
                .Where(t => t.Factivo)
                .Select(t => new
                {
                    FidGastoTipo = t.FidGastoTipo,
                    Fdescripcion = t.Fdescripcion
                })
                .ToListAsync();

            ViewBag.TiposGasto = tiposGasto;

            // Return a single GastoViewModel instead of a list
            return PartialView("_CreateGastoPartial", new GastoViewModel());
        }

        [HttpPost]
        [Authorize(Policy = "Permissions.Gastos.Crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGasto([Bind("FidGasto,FkidGastoTipo,Fmonto,Fdescripcion,Ffecha")] TbGasto tbGasto)
        {
            try
            {
                // Debugging - log all permissions
                var claims = User.Claims.Where(c => c.Type == "Permission").ToList();
                _logger.LogInformation("User  permissions: {@Permissions}", claims.Select(c => c.Value));

                var identityId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(identityId))
                    return BadRequest(new { success = false, message = "El usuario no está autenticado." });

                var usuario = await _context.TbUsuarios.FirstOrDefaultAsync(u => u.IdentityId == identityId);
                if (usuario == null)
                    return BadRequest(new { success = false, message = $"No se encontró un usuario con el IdentityId: {identityId}" });


                if (ModelState.IsValid)
                {
                    tbGasto.FkidUsuario = usuario.FidUsuario;

                    tbGasto.Factivo = true;

                    _context.Add(tbGasto);
                    await _context.SaveChangesAsync();

                    return await CargarGasto();
                }

                return BadRequest(new { success = false, message = "El modelo no es válido" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el gasto");
                return StatusCode(500);
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Gastos.Editar")]
        public async Task<IActionResult> EditGasto(int? id)
        {
            try
            {
                if (id == null) return NotFound();

                var tbGasto = await _context.TbGastos.FindAsync(id);
                if (tbGasto == null) return NotFound();

                var tipoGasto = await _context.TbGastoTipos
                    .Where(p => p.FidGastoTipo == tbGasto.FkidGastoTipo)
                    .Select(p => $"{p.Fdescripcion}")
                    .FirstOrDefaultAsync();

                ViewBag.TipoGasto = tipoGasto ?? "Desconocido";
                return PartialView("_EditGastoPartial", tbGasto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cargar edición de gastos {id}");
                return StatusCode(500);
            }
        }

        [HttpPost]
        [Authorize(Policy = "Permissions.Gastos.Editar")] // Corregido el nombre de la política
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGasto(int id, [Bind("FidGasto,FkidGastoTipo,Fmonto,Ffecha,Fdescripcion")] TbGasto tbGasto)
        {
            try
            {
                if (id != tbGasto.FidGasto) return NotFound();

                if (ModelState.IsValid)
                {
                    var gasto = await _context.TbGastos.FindAsync(id);
                    if (gasto == null) return NotFound();

                    tbGasto.Factivo = true;
                    tbGasto.FkidUsuario = gasto.FkidUsuario;

                    _context.Entry(gasto).CurrentValues.SetValues(tbGasto);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Datos actualizados: {id}");

                    // Retornar JSON con éxito y URL para cargar la vista parcial
                    return Json(new { success = true, partialViewUrl = Url.Action("CargarGasto", "TbGastos") });
                }

                return PartialView("_EditGastoPartial", tbGasto);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!TbGastoExists(tbGasto.FidGasto))
                {
                    return NotFound();
                }
                _logger.LogError(ex, $"Error de concurrencia al editar el gasto {id}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al editar el gasto {id}");
                return StatusCode(500);
            }
        }

        private bool TbGastoExists(int id)
        {
            return _context.TbGastos.Any(e => e.FidGasto == id);
        }


        [HttpGet]
        public async Task<IActionResult> BuscarUsuario(string term = null)
        {
            try
            {
                var usersQuery = _context.TbUsuarios.AsQueryable();

                if (!string.IsNullOrWhiteSpace(term))
                {
                    term = term.ToLower();
                    usersQuery = usersQuery.Where(u =>
                        u.Fusuario.ToLower().Contains(term));
                }

                var results = await usersQuery
                    .OrderBy(u => u.Fusuario)
                    .Select(u => new
                    {
                        id = u.Fusuario, // El value del select2 será el nombre
                        text = u.Fusuario,
                        tipo = "usuario"
                    })
                    .ToListAsync();

                return Json(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios desde Identity.");
                return Json(new List<object>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> BuscarTipoGasto(string term = null)
        {
            try
            {
                var usersQuery = _context.TbGastoTipos.AsQueryable();

                if (!string.IsNullOrWhiteSpace(term))
                {
                    term = term.ToLower();
                    usersQuery = usersQuery.Where(u =>
                        u.Fdescripcion.ToLower().Contains(term));
                }

                var results = await usersQuery
                    .OrderBy(u => u.Fdescripcion)
                    .Select(u => new
                    {
                        id = u.Fdescripcion, // El value del select2 será el nombre
                        text = u.Fdescripcion,
                        tipo = "descripcion"
                    })
                    .ToListAsync();

                return Json(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la descripcion de los tipos de gastos.");
                return Json(new List<object>());
            }
        }

        [HttpPost]
        [Authorize(Policy = "Permissions.Gastos.Anular")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            try
            {
                var gasto = await _context.TbGastos.FindAsync(id);
                if (gasto == null)
                {
                    _logger.LogWarning($"Intento de cambiar estado del gasto no encontrado: {id}");
                    return NotFound();
                }

                gasto.Factivo = !gasto.Factivo;
                _context.Update(gasto);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Estado de gasto {id} cambiado a: {gasto.Factivo}");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cambiar estado del gasto {id}");
                return StatusCode(500, "Error interno al cambiar el estado");
            }
        }
    }
}