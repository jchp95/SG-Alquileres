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
    public class TbCuotasController : BaseController
    {
        private readonly ILogger<TbInmueblesController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public TbCuotasController(ApplicationDbContext context,
        ILogger<TbInmueblesController> logger,
        UserManager<IdentityUser> userManager) : base(context)
        {
            _logger = logger;
            _userManager = userManager;
        }
        public IActionResult Index(string vista = "crear")
        {
            ViewData["Vista"] = vista;
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
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    _logger.LogWarning("Modelo no válido. Errores: {Errors}", string.Join(", ", errors));
                    return BadRequest(new { success = false, message = "Datos inválidos", errors = errors });
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

                // Obtener TODAS las cuotas existentes para esta cuenta (ordenadas por fecha)
                var cuotasExistentes = await _context.TbCxcCuota
                    .Where(c => c.FkidCxc == model.FidCxc)
                    .OrderBy(c => c.Fvence)
                    .ToListAsync();

                // Determinar el día de vencimiento deseado (del modelo)
                int diaVencimientoDeseado = model.Fvence.Day;
                bool esUltimoDiaMes = diaVencimientoDeseado == DateTime.DaysInMonth(model.Fvence.Year, model.Fvence.Month);

                // Obtener fecha actual para comparación
                DateTime fechaActual = DateTime.Now.Date;

                // 1. Actualizar fechas de vencimiento y estados de cuotas existentes si es necesario
                foreach (var cuotaExistente in cuotasExistentes)
                {
                    // No modificar cuotas con estado 'S'
                    if (cuotaExistente.Fstatus == 'S')
                    {
                        _logger.LogInformation($"Saltando cuota #{cuotaExistente.FNumeroCuota} porque tiene estado 'S'");
                        continue;
                    }

                    DateTime nuevaFechaVencimiento;

                    if (esUltimoDiaMes)
                    {
                        // Usar último día del mes
                        nuevaFechaVencimiento = new DateTime(
                            cuotaExistente.Fvence.Year,
                            cuotaExistente.Fvence.Month,
                            DateTime.DaysInMonth(cuotaExistente.Fvence.Year, cuotaExistente.Fvence.Month)
                        );
                    }
                    else
                    {
                        // Usar día deseado, ajustando si no existe en ese mes
                        int ultimoDiaMes = DateTime.DaysInMonth(cuotaExistente.Fvence.Year, cuotaExistente.Fvence.Month);
                        int dia = Math.Min(diaVencimientoDeseado, ultimoDiaMes);

                        nuevaFechaVencimiento = new DateTime(
                            cuotaExistente.Fvence.Year,
                            cuotaExistente.Fvence.Month,
                            dia
                        );
                    }

                    // Determinar el estado basado en la nueva fecha de vencimiento
                    char nuevoEstado = nuevaFechaVencimiento < fechaActual ? 'V' : 'N';

                    if (cuotaExistente.Fvence != nuevaFechaVencimiento || cuotaExistente.Fstatus != nuevoEstado)
                    {
                        cuotaExistente.Fvence = nuevaFechaVencimiento;
                        cuotaExistente.Fstatus = nuevoEstado;
                        _context.Update(cuotaExistente);
                        _logger.LogInformation($"Actualizando cuota #{cuotaExistente.FNumeroCuota} - fecha: {nuevaFechaVencimiento:dd/MM/yyyy}, estado: {nuevoEstado}");
                    }
                }

                // 2. Generar nuevas cuotas que faltan
                var cuotasCreadas = new List<TbCxcCuotum>();
                DateTime fechaGeneracion = model.Fvence;
                int cuotasGeneradas = 0;

                // Primero determinamos todas las fechas de cuotas que deberían existir (existentes + nuevas)
                var todasFechasCuotas = new List<DateTime>();

                // Agregar fechas de cuotas existentes (ya actualizadas)
                todasFechasCuotas.AddRange(cuotasExistentes.Select(c => c.Fvence));

                // Agregar fechas de nuevas cuotas que necesitamos crear
                DateTime fechaTemporal = model.Fvence;
                while (cuotasGeneradas < model.CantidadCuotas)
                {
                    if (!todasFechasCuotas.Any(f => f.Year == fechaTemporal.Year && f.Month == fechaTemporal.Month))
                    {
                        DateTime fechaVencimiento;

                        if (esUltimoDiaMes)
                        {
                            fechaVencimiento = new DateTime(
                                fechaTemporal.Year,
                                fechaTemporal.Month,
                                DateTime.DaysInMonth(fechaTemporal.Year, fechaTemporal.Month)
                            );
                        }
                        else
                        {
                            int ultimoDiaMes = DateTime.DaysInMonth(fechaTemporal.Year, fechaTemporal.Month);
                            int dia = Math.Min(diaVencimientoDeseado, ultimoDiaMes);

                            fechaVencimiento = new DateTime(
                                fechaTemporal.Year,
                                fechaTemporal.Month,
                                dia
                            );
                        }

                        todasFechasCuotas.Add(fechaVencimiento);
                        cuotasGeneradas++;
                    }
                    fechaTemporal = fechaTemporal.AddMonths(1);
                }

                // Ordenar todas las fechas (existentes + nuevas)
                todasFechasCuotas = todasFechasCuotas.OrderBy(f => f).ToList();

                // 3. Renumerar TODAS las cuotas (existentes + nuevas) en orden cronológico
                int numeroCuota = 1;
                foreach (var fecha in todasFechasCuotas)
                {
                    // Buscar si ya existe una cuota para esta fecha
                    var cuotaExistente = cuotasExistentes.FirstOrDefault(c => c.Fvence == fecha);

                    if (cuotaExistente != null)
                    {
                        if (cuotaExistente.Fstatus == 'S')
                        {
                            // Si es pagada, mantener su número pero ajustar el contador para la siguiente
                            _logger.LogInformation($"Manteniendo número de cuota {cuotaExistente.FNumeroCuota} para cuota con estado 'S'");
                            numeroCuota = cuotaExistente.FNumeroCuota + 1; // Ajustar el contador
                            continue;
                        }

                        // Actualizar número de cuota existente
                        if (cuotaExistente.FNumeroCuota != numeroCuota)
                        {
                            cuotaExistente.FNumeroCuota = numeroCuota;
                            _context.Update(cuotaExistente);
                            _logger.LogInformation($"Reasignando número de cuota de {cuotaExistente.FNumeroCuota} a {numeroCuota} para vencimiento {fecha:dd/MM/yyyy}");
                        }
                    }
                    else
                    {
                        // Determinar el estado basado en la fecha de vencimiento
                        char estado = fecha < fechaActual ? 'V' : 'N';

                        // Crear nueva cuota
                        var nuevaCuota = new TbCxcCuotum
                        {
                            FkidCxc = model.FidCxc,
                            FNumeroCuota = numeroCuota,
                            Fvence = fecha,
                            Fmonto = (int)model.Fmonto,
                            Fmora = model.TasaMora,
                            Fstatus = estado,
                            Factivo = true,
                            FfechaUltCalculo = fecha.AddMonths(1),
                            Fsaldo = (int)model.Fmonto
                        };

                        _context.Add(nuevaCuota);
                        cuotasCreadas.Add(nuevaCuota);
                        _logger.LogInformation($"Creando cuota #{numeroCuota} con vencimiento {fecha:dd/MM/yyyy}, estado: {estado}");
                    }

                    numeroCuota++;
                }

                await _context.SaveChangesAsync();

                // Actualizar fecha de próxima cuota en la cuenta
                if (todasFechasCuotas.Any())
                {
                    cuentaPorCobrar.FfechaProxCuota = todasFechasCuotas.Last().AddMonths(1);
                    _context.Update(cuentaPorCobrar);
                    await _context.SaveChangesAsync();
                }

                return Json(new
                {
                    success = true,
                    message = $"{cuotasCreadas.Count} cuotas creadas correctamente (de {model.CantidadCuotas} solicitadas)",
                    cuotasCreadas = cuotasCreadas.Select(c => new
                    {
                        NumeroCuota = c.FNumeroCuota,
                        FechaVencimiento = c.Fvence.ToString("dd/MM/yyyy"),
                        Estado = c.Fstatus.ToString()
                    }).ToList(),
                    cuotasActualizadas = cuotasExistentes.Count(c => c.Fstatus != 'S'),
                    nextCuotaNumber = numeroCuota
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
                        join inquilino in _context.TbInquilinos on cxc.FkidInquilino equals inquilino.FidInquilino
                        join inmueble in _context.TbInmuebles on cxc.FkidInmueble equals inmueble.FidInmueble
                        select new { cxc, inmueble, inquilino };

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(x => x.cxc.FidCuenta.ToString().Contains(searchTerm) ||
                                        x.cxc.Fmonto.ToString().Contains(searchTerm) ||
                                        x.cxc.FdiasGracia.ToString().Contains(searchTerm) ||
                                        x.cxc.FtasaMora.ToString().Contains(searchTerm) ||
                                        x.inquilino.Fnombre.Contains(searchTerm) ||
                                        x.inquilino.Fapellidos.Contains(searchTerm) ||
                                        x.inmueble.Fdescripcion.Contains(searchTerm));
            }

            // Obtener el número máximo de cuotas por cuenta
            var resultados = await query
                .GroupBy(x => new
                {
                    x.cxc.FidCuenta,
                    x.inmueble.Fdescripcion,
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
                           $"Inmueble: {g.Key.Fdescripcion} - " +
                           $"Monto: {g.Key.Fmonto:C} - Día de Gracia: {g.Key.FdiasGracia} días - Mora: {g.Key.FtasaMora}%",
                    Tipo = "cuenta por cobrar",
                    NumeroCuota = _context.TbCxcCuota.Where(c => c.FkidCxc == g.Key.FidCuenta).Max(c => c.FNumeroCuota),
                    FechaVencimiento = _context.TbCxcCuota
                        .Where(c => c.FkidCxc == g.Key.FidCuenta)
                        .OrderByDescending(c => c.Fvence)
                        .Select(c => (DateTime)c.Fvence)
                        .FirstOrDefault(),
                    Monto = _context.TbCxcCuota
                        .Where(c => c.FkidCxc == g.Key.FidCuenta)
                        .OrderByDescending(c => c.Fvence)
                        .Select(c => c.Fmonto)
                        .FirstOrDefault(),
                    Mora = _context.TbCxcCuota
                        .Where(c => c.FkidCxc == g.Key.FidCuenta)
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
                    .Where(c => c.FkidCxc == cuenta.FidCuenta && (c.Fstatus == 'N')) // Filtrar por estado
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
