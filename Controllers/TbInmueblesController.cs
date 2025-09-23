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
    public class TbInmueblesController : BaseController
    {

        private readonly ILogger<TbInmueblesController> _logger;

        private readonly UserManager<IdentityUser> _userManager;

        // Corrección: Usar el tipo correcto para el logger
        public TbInmueblesController(ApplicationDbContext context,
        ILogger<TbInmueblesController> logger,
        UserManager<IdentityUser> userManager) : base(context)
        {
            _logger = logger;
            _userManager = userManager;
        }

        [HttpPost]
        [Authorize(Policy = "Permissions.Inmuebles.Anular")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            try
            {
                var inmueble = await _context.TbInmuebles.FindAsync(id);
                if (inmueble == null)
                {
                    _logger.LogWarning($"Intento de cambiar estado de inmueble no encontrado: {id}");
                    return NotFound();
                }

                inmueble.Factivo = !inmueble.Factivo;
                _context.Update(inmueble);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Estado de inmueble {id} cambiado a: {inmueble.Factivo}");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cambiar estado del inmueble {id}");
                return StatusCode(500, "Error interno al cambiar el estado");
            }
        }

        // GET: TbInmuebles
        public IActionResult Index(string vista = "crear")
        {
            ViewData["Vista"] = vista;
            return View();
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Inmuebles.Ver")]
        public async Task<IActionResult> CargarInmuebles()
        {
            try
            {
                // Ordenar antes de proyectar a ViewModel
                var inmuebles = await _context.TbInmuebles
                    .OrderBy(i => i.FidInmueble)
                    .ToListAsync();

                var propietarios = await _context.TbPropietarios
                    .Where(p => p.Factivo)
                    .ToDictionaryAsync(p => p.FidPropietario, p => $"{p.Fnombre} {p.Fapellidos}");

                var monedas = await _context.TbMonedas
                    .Where(p => p.Factivo)
                    .ToDictionaryAsync(p => p.FidMoneda, p => $"{p.Fmoneda}");

                var inmuebleViewModels = inmuebles.Select(i => new InmuebleViewModel
                {
                    FidInmueble = i.FidInmueble,
                    Fdescripcion = i.Fdescripcion,
                    Fdireccion = i.Fdireccion,
                    Fubicacion = i.Fubicacion,
                    Fprecio = i.Fprecio,
                    Factivo = i.Factivo,
                    PropietarioNombre = propietarios.GetValueOrDefault(i.FkidPropietario, "Desconocido"),
                    TipoMoneda = monedas.GetValueOrDefault(i.FkidMoneda, "Desconocido")
                }).ToList();

                return PartialView("_InmueblesPartial", inmuebleViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar inmuebles");
                return PartialView("_InmueblesPartial", new List<InmuebleViewModel>());
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Inmuebles.Crear")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var propietarios = await _context.TbPropietarios
                    .Where(p => p.Factivo)
                    .Select(p => new
                    {
                        p.FidPropietario,
                        NombreCompleto = $"{p.Fnombre} {p.Fapellidos}"
                    })
                    .ToListAsync();

                var propietarioList = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "Seleccionar propietario" }
        };

                propietarioList.AddRange(propietarios.Select(p => new SelectListItem
                {
                    Value = p.FidPropietario.ToString(),
                    Text = p.NombreCompleto
                }));

                ViewBag.FkidPropietario = propietarioList;

                // 🔽 Obtener las monedas activas
                var monedas = await _context.TbMonedas
                    .Where(m => m.Factivo)
                    .Select(m => new SelectListItem
                    {
                        Value = m.FidMoneda.ToString(),
                        Text = m.Fmoneda // puedes incluir símbolo también si quieres
                    })
                    .ToListAsync();

                ViewBag.FkidMoneda = monedas;

                return PartialView("_CreateInmueblePartial", new TbInmueble());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar formulario de creación");
                return StatusCode(500);
            }
        }


        [HttpPost]
        [Authorize(Policy = "Permissions.Inmuebles.Crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FidInmueble,FkidPropietario,Fdescripcion,Fdireccion,Fubicacion,Fprecio,FkidMoneda,FkidUsuario")] TbInmueble tbInmueble)
        {
            try
            {
                // Guardar cultura actual
                var currentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;

                try
                {
                    // Forzar cultura invariante para parseo numérico
                    System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

                    var identityId = _userManager.GetUserId(User);
                    if (string.IsNullOrEmpty(identityId))
                        return BadRequest(new { success = false, message = "El usuario no está autenticado." });

                    var usuario = await _context.TbUsuarios.FirstOrDefaultAsync(u => u.IdentityId == identityId);
                    if (usuario == null)
                        return BadRequest(new { success = false, message = $"No se encontró un usuario con el IdentityId: {identityId}" });

                    if (!ModelState.IsValid)
                    {
                        var errors = ModelState.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        );

                        return BadRequest(new { success = false, errors });
                    }

                    tbInmueble.FkidUsuario = usuario.FidUsuario;
                    tbInmueble.FfechaRegistro = DateTime.Now;
                    tbInmueble.Factivo = true;

                    _context.Add(tbInmueble);
                    await _context.SaveChangesAsync();

                    // 🚀 Devolver JSON en vez de PartialView
                    return Ok(new { success = true });
                }
                finally
                {
                    // Restaurar cultura original
                    System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error de base de datos al crear inmueble");
                return StatusCode(500, new { success = false, message = "Error crítico en la base de datos. Contacte al administrador." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error general al crear inmueble");
                return StatusCode(500, new { success = false, message = "Error interno del servidor." });
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Inmuebles.Editar")] // Corregido el nombre de la política
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null) return NotFound();

                var tbInmueble = await _context.TbInmuebles.FindAsync(id);
                if (tbInmueble == null) return NotFound();

                var propietario = await _context.TbPropietarios
                    .Where(p => p.FidPropietario == tbInmueble.FkidPropietario)
                    .Select(p => $"{p.Fnombre} {p.Fapellidos}")
                    .FirstOrDefaultAsync();

                // Cargar los tipos de monedas desde la base de datos
                var tipoMoneda = await _context.TbMonedas.ToListAsync();

                // Crear una lista de opciones con los tipos de monedas
                var opciones = tipoMoneda.Select(p => new SelectListItem
                {
                    Value = p.FidMoneda.ToString(),
                    Text = p.Fmoneda,
                    Selected = p.FidMoneda == tbInmueble.FkidMoneda // Seleccionar el periodo correspondiente
                }).ToList();

                // Crear el SelectList para el ViewBag
                ViewBag.FkidMoneda = opciones;

                ViewBag.PropietarioNombre = propietario ?? "Desconocido";
                return PartialView("_EditInmueblePartial", tbInmueble);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cargar edición de inmueble {id}");
                return StatusCode(500);
            }
        }

        [HttpPost]
        [Authorize(Policy = "Permissions.Inmuebles.Editar")] // Corregido el nombre de la política
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FidInmueble,FkidPropietario,Fdescripcion,Fdireccion,Fubicacion,Fprecio,FkidMoneda")] TbInmueble tbInmueble)
        {
            try
            {
                if (id != tbInmueble.FidInmueble) return NotFound();

                if (ModelState.IsValid)
                {
                    var inmueble = await _context.TbInmuebles.FindAsync(id);
                    if (inmueble == null) return NotFound();

                    tbInmueble.Factivo = true;
                    tbInmueble.FkidUsuario = inmueble.FkidUsuario;

                    _context.Entry(inmueble).CurrentValues.SetValues(tbInmueble);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Inmueble actualizado: {id}");

                    return await CargarInmuebles();
                }

                return PartialView("_EditInmueblePartial", tbInmueble);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!TbInmuebleExists(tbInmueble.FidInmueble))
                {
                    return NotFound();
                }
                _logger.LogError(ex, $"Error de concurrencia al editar inmueble {id}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al editar inmueble {id}");
                return StatusCode(500);
            }
        }

        private bool TbInmuebleExists(int id)
        {
            return _context.TbInmuebles.Any(e => e.FidInmueble == id);
        }

        [HttpGet]
        public async Task<IActionResult> BuscarPropietario(string searchTerm = null)
        {
            try
            {
                var query = _context.TbPropietarios
                    .Where(p => p.Factivo)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(p =>
                        p.Fnombre.ToLower().Contains(searchTerm) ||
                        p.Fapellidos.ToLower().Contains(searchTerm) ||
                        (p.Fcedula != null && p.Fcedula.Contains(searchTerm)));
                }

                var resultados = await query
                    .OrderBy(p => p.Fapellidos)
                    .ThenBy(p => p.Fnombre)
                    .Take(20)
                    .Select(p => new
                    {
                        id = p.FidPropietario,
                        text = $"{p.Fnombre} {p.Fapellidos}",
                        tipo = "propietario",
                        cedula = p.Fcedula
                    })
                    .ToListAsync();

                return Json(new { results = resultados });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al buscar propietarios. Término: {searchTerm}");
                return Json(new { results = new List<object>() });
            }
        }
    }
}