using System.Text.Json;
using Alquileres.Context;
using Alquileres.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Alquileres.Controllers
{
    [Authorize] // Esto aplicará a todas las acciones del controlador, requiriendo que el usuario esté autenticado.
    public class TbCuotasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TbInmueblesController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public TbCuotasController(ApplicationDbContext context, ILogger<TbInmueblesController> logger, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        // GET: TbCuotas/CargarCuotas
        [HttpGet]
        [Authorize(Policy = "Permissions.Cuotas.Ver")]
        public async Task<IActionResult> CargarCuota()
        {
            var cuotas = await _context.TbCxcCuota.ToListAsync();
            return PartialView("_CuotasPartial", cuotas); // Devuelve la vista parcial
        }

        // GET: TbCuotas/Create
        [HttpGet]
        [Authorize(Policy = "Permissions.Cuotas.Crear")]
        public IActionResult Create()
        {
            return PartialView("_CreateCuotaPartial");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permissions.Cuotas.Crear")]
        public async Task<IActionResult> Create([FromBody] CuotaCreateViewModel model)
        {
            try
            {
                // Validar el modelo
                if (!ModelState.IsValid)
                {
                    // Log detallado de los errores
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    _logger.LogWarning("Modelo no válido. Errores: {Errors}", string.Join(", ", errors));

                    return BadRequest(new
                    {
                        success = false,
                        message = "Datos inválidos",
                        errors = errors
                    });
                }

                // Obtener el usuario autenticado
                var identityId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(identityId))
                {
                    return Json(new { success = false, message = "El usuario no está autenticado." });
                }

                var usuario = await _context.TbUsuarios.FirstOrDefaultAsync(u => u.IdentityId == identityId);
                if (usuario == null)
                {
                    return Json(new { success = false, message = $"No se encontró un usuario con el IdentityId: {identityId}" });
                }

                // Verificar cuenta por cobrar
                var cuentaPorCobrar = await _context.TbCxcs.FindAsync(model.FidCxc);
                if (cuentaPorCobrar == null)
                {
                    return Json(new { success = false, message = "La cuenta por cobrar especificada no existe." });
                }

                // Obtener el último número de cuota para esta cuenta
                int ultimoNumeroCuota = await _context.TbCxcCuota
                    .Where(c => c.FidCxc == model.FidCxc)
                    .MaxAsync(c => (int?)c.FNumeroCuota) ?? 0;

                // Si se envió un número específico, usarlo como base (validando que sea mayor que el último existente)
                int numeroCuotaBase = Math.Max(model.FNumeroCuota, ultimoNumeroCuota + 1);

                // Crear las cuotas
                var cuotasCreadas = new List<TbCxcCuotum>();

                for (int i = 0; i < model.CantidadCuotas; i++)
                {
                    var fechaVencimiento = model.Fvence.AddMonths(i + 1);
                    var fechaProximoCalculo = fechaVencimiento.AddMonths(1);

                    var nuevaCuota = new TbCxcCuotum
                    {
                        FidCxc = model.FidCxc,
                        FNumeroCuota = numeroCuotaBase + i, // Usar el número base calculado
                        Fvence = fechaVencimiento,
                        Fmonto = (int)model.Fmonto,
                        Fmora = model.TasaMora,
                        Fstatus = 'N',
                        Factivo = true,
                        FfechaUltCalculo = fechaProximoCalculo,
                        Fsaldo = (int)model.Fmonto
                    };

                    _context.Add(nuevaCuota);
                    cuotasCreadas.Add(nuevaCuota);

                    _logger.LogInformation($"Creando cuota #{nuevaCuota.FNumeroCuota} para la cuenta {model.FidCxc}");
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"{model.CantidadCuotas} cuotas creadas correctamente",
                    cuotas = cuotasCreadas.Select(c => new
                    {
                        NumeroCuota = c.FNumeroCuota,
                        FechaVencimiento = c.Fvence.ToString("dd/MM/yyyy")
                    }).ToList(),
                    nextCuotaNumber = numeroCuotaBase + model.CantidadCuotas // Devolver el próximo número disponible
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cuotas");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno al crear las cuotas",
                    error = ex.Message
                });
            }
        }

        // GET: TbCuotas/Delete
        [HttpGet]
        [Authorize(Policy = "Permissions.Cuotas.Eliminar")]
        public IActionResult Delete()
        {
            return PartialView("_DeleteCuotaPartial");
        }

        // DELETE: TbCuotas/Delete/{id}
        [HttpDelete]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Permissions.Cuotas.Eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation($"Solicitud de eliminación recibida para la cuota con ID: {id}");

            try
            {
                var cuota = await _context.TbCxcCuota.FindAsync(id);
                if (cuota == null)
                {
                    _logger.LogWarning($"No se encontró la cuota con ID: {id}");
                    return Json(new { success = false, message = "Cuota no encontrada." });
                }

                _logger.LogInformation($"Cuota encontrada: {cuota}");

                _context.TbCxcCuota.Remove(cuota);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Cuota con ID: {id} eliminada correctamente.");
                return Json(new { success = true, message = "Cuota eliminada correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la cuota con ID: {id}", id);
                return Json(new { success = false, message = "Ocurrió un error al eliminar la cuota." });
            }
        }

        // GET: TbCuotas/BuscarCxc
        [HttpGet]
        public async Task<IActionResult> BuscarCxc(string? searchTerm = null)
        {
            // Filtro inicial: cuentas activas y que no estén canceladas (estado != 'S')
            var query = from cxc in _context.TbCxcs.Where(c => c.Factivo == true && c.Fstatus != 'S')
                        join inquilino in _context.TbInquilinos on cxc.FidInquilino equals inquilino.FidInquilino
                        select new { cxc, inquilino };

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(x => x.cxc.FidCuenta.ToString().Contains(searchTerm) ||
                                         x.cxc.Fmonto.ToString().Contains(searchTerm) ||
                                         x.cxc.FdiasGracia.ToString().Contains(searchTerm) ||
                                         x.cxc.FtasaMora.ToString().Contains(searchTerm) ||
                                         x.inquilino.Fnombre.Contains(searchTerm) ||
                                         x.inquilino.Fapellidos.Contains(searchTerm));
            }

            // Obtener el número máximo de cuotas por cuenta
            var resultados = await query
                .GroupBy(x => new
                {
                    x.cxc.FidCuenta,
                    x.inquilino.Fnombre,
                    x.inquilino.Fapellidos,
                    x.cxc.Fmonto,
                    x.cxc.FdiasGracia,
                    x.cxc.FtasaMora,
                    x.cxc.Fstatus
                })
                .Select(g => new ResultadoBusquedaCxCViewModel
                {
                    Id = g.Key.FidCuenta,
                    Text = $"Cuenta #{g.Key.FidCuenta} - {g.Key.Fnombre} {g.Key.Fapellidos} - " +
                           $"Monto: {g.Key.Fmonto:C} - Día de Gracia: {g.Key.FdiasGracia} días - Mora: {g.Key.FtasaMora}%",
                    Tipo = "cuenta por cobrar",
                    NumeroCuota = _context.TbCxcCuota.Where(c => c.FidCxc == g.Key.FidCuenta).Max(c => c.FNumeroCuota),
                    FechaVencimiento = _context.TbCxcCuota
                        .Where(c => c.FidCxc == g.Key.FidCuenta)
                        .OrderByDescending(c => c.Fvence)
                        .Select(c => (DateTime)c.Fvence)
                        .FirstOrDefault(),
                    Monto = _context.TbCxcCuota
                        .Where(c => c.FidCxc == g.Key.FidCuenta)
                        .OrderByDescending(c => c.Fvence)
                        .Select(c => c.Fmonto)
                        .FirstOrDefault(),
                    Mora = _context.TbCxcCuota
                        .Where(c => c.FidCxc == g.Key.FidCuenta)
                        .OrderByDescending(c => c.Fvence)
                        .Select(c => c.Fmora)
                        .FirstOrDefault(),
                })
                .ToListAsync();

            return Json(new { results = resultados });
        }

        public async Task<IActionResult> ObtenerCuotasPorCxC(int cuentaId)
        {
            try
            {
                var cuenta = await _context.TbCxcs
                    .FirstOrDefaultAsync(c => c.FidCuenta == cuentaId);

                if (cuenta == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Cuenta no encontrada",
                        cuotas = new List<object>()
                    });
                }

                var cuotas = await _context.TbCxcCuota
                    .Where(c => c.FidCxc == cuenta.FidCuenta && (c.Fstatus == 'N')) // Filtrar por estado
                    .Select(c => new
                    {
                        fidCuota = c.FidCuota,
                        fnumeroCuota = c.FNumeroCuota,
                        fvence = c.Fvence,
                        fmonto = c.Fmonto,
                        fsaldo = c.Fsaldo,
                        fmora = c.Fmora,
                        fstatus = c.Fstatus,
                    })
                    .ToListAsync();

                Console.WriteLine($"Cuotas encontradas: {JsonSerializer.Serialize(cuotas)}"); // Debug

                return Json(new
                {
                    success = true,
                    cuotas = cuotas
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cuotas");
                return Json(new
                {
                    success = false,
                    message = ex.Message,
                    cuotas = new List<object>()
                });
            }
        }

    }
}
