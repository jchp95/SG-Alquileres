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
    [Authorize]
    public class TbCuentasPorCobrarController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GeneradorDeCuotasService> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public TbCuentasPorCobrarController(ApplicationDbContext context,
        ILogger<GeneradorDeCuotasService> logger,
        UserManager<IdentityUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager; // Asignación del UserManager
        }

        [HttpPost]
        [Authorize(Policy = "Permissions.CxC.Anular")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var cxC = await _context.TbCxcs.FindAsync(id);
            if (cxC == null)
            {
                return NotFound();
            }

            // Cambiar el estado del inmueble
            cxC.Factivo = !cxC.Factivo;

            _context.Update(cxC);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Carga vista principal
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.CxC.Ver")]
        public async Task<IActionResult> CargarCuentasPorCobrar()
        {
            try
            {
                var cuentas = await _context.TbCxcs.ToListAsync();
                var inquilinos = await _context.TbInquilinos.ToListAsync();
                var periodos = await _context.PeriodosPagos.ToListAsync();

                var cuentasPorCobrarViewModels = new List<CuentaPorCobrarViewModel>();

                foreach (var c in cuentas)
                {
                    // Obtener la última cuota de la cuenta
                    var ultimaCuota = await _context.TbCxcCuota
                        .Where(q => q.FidCxc == c.FidCuenta)
                        .OrderByDescending(q => q.Fvence)
                        .FirstOrDefaultAsync();

                    // Calcular la fecha de la próxima cuota
                    DateTime? fechaProxCuota = null;
                    if (ultimaCuota != null)
                    {
                        fechaProxCuota = CalcularFechaProxCuota(ultimaCuota.Fvence, c.FidPeriodoPago);
                    }

                    cuentasPorCobrarViewModels.Add(new CuentaPorCobrarViewModel
                    {
                        FidCuenta = c.FidCuenta,
                        FidInquilino = c.FidInquilino,
                        InquilinoNombre = c.FidInquilino.HasValue
                            ? inquilinos.FirstOrDefault(i => i.FidInquilino == c.FidInquilino)?.Fnombre + " " +
                              inquilinos.FirstOrDefault(i => i.FidInquilino == c.FidInquilino)?.Fapellidos
                            : "Desconocido",
                        FidInmueble = c.FkidInmueble,
                        Fmonto = c.Fmonto,
                        FfechaInicio = c.FfechaInicio,
                        FdiasGracia = c.FdiasGracia,
                        Factivo = c.Factivo,
                        FtasaMora = c.FtasaMora,
                        Fnota = c.Fnota,
                        FidPeriodoPago = c.FidPeriodoPago,
                        NombrePeriodoPago = periodos.FirstOrDefault(p => p.Id == c.FidPeriodoPago)?.Nombre ?? "Desconocido",
                        FfechaProxCuota = fechaProxCuota // Asignar la fecha de la próxima cuota calculada
                    });
                }

                return PartialView("_CuentasPorCobrarPartial", cuentasPorCobrarViewModels);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Método para calcular la fecha de la próxima cuota
        private DateTime? CalcularFechaProxCuota(DateTime fechaVencimiento, int? periodoPagoId)
        {
            if (!periodoPagoId.HasValue)
                return null;

            int dias = 0;

            // Determinar el número de días según el periodo de pago
            switch (periodoPagoId.Value)
            {
                case 1: // Semanal
                    dias = 7;
                    break;
                case 2: // Quincenal
                    dias = 15;
                    break;
                case 3: // Mensual
                    dias = 30;
                    break;
                default:
                    throw new ArgumentException("Periodo de pago no válido");
            }

            // Calcular y devolver la fecha de la próxima cuota
            return fechaVencimiento.AddDays(dias);
        }

        // GET: Cargar formulario de creación
        [HttpGet]
        [Authorize(Policy = "Permissions.CxC.Crear")]
        public async Task<IActionResult> Create()
        {
            // Cargar los periodos de pago desde la base de datos
            var periodosPagos = await _context.PeriodosPagos.ToListAsync();

            // Crear una lista de opciones que incluya la opción predeterminada
            var opciones = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Seleccionar método de pago" } // Opción predeterminada
            };

            // Agregar las opciones de periodos de pago
            opciones.AddRange(periodosPagos.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Nombre
            }));

            // Crear el SelectList para el ViewBag
            ViewBag.FidPeriodoPago = opciones;

            // Retornar la vista parcial
            return PartialView("_CreateCuentasPorCobrarPartial");
        }

        [HttpPost]
        [Authorize(Policy = "Permissions.CxC.Crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FidInquilino,FkidInmueble,FfechaInicio,Fmonto,FdiasGracia,FtasaMora,Fnota,FidPeriodoPago")] TbCxc tbCxc)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            try
            {
                var identityId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(identityId))
                    return BadRequest("El usuario no está autenticado.");

                var usuario = await _context.TbUsuarios.FirstOrDefaultAsync(u => u.IdentityId == identityId);
                if (usuario == null)
                    return BadRequest($"No se encontró un usuario con el IdentityId: {identityId}");

                tbCxc.FkidUsuario = usuario.FidUsuario;
                tbCxc.Factivo = true;

                _context.Add(tbCxc);
                await _context.SaveChangesAsync();

                var numeroCuotaMaxima = await _context.TbCxcCuota
                    .Where(c => c.FidCxc == tbCxc.FidCuenta)
                    .MaxAsync(c => (int?)c.FNumeroCuota) ?? 0;

                var fechaVencimiento = CalcularFechaVencimiento(tbCxc.FfechaInicio, tbCxc.FidPeriodoPago);

                var cuota = new TbCxcCuotum
                {
                    FidCxc = tbCxc.FidCuenta,
                    FNumeroCuota = numeroCuotaMaxima + 1,
                    Fvence = fechaVencimiento,
                    Fmonto = (int)tbCxc.Fmonto,
                    Fsaldo = tbCxc.Fmonto,
                    Fmora = tbCxc.FtasaMora,
                    FfechaUltCalculo = fechaVencimiento,
                    Factivo = true,
                    Fstatus = 'N'
                };

                _context.TbCxcCuota.Add(cuota);
                await _context.SaveChangesAsync();

                var cuentasPorCobrar = await _context.TbCxcs.ToListAsync();
                var cuentasPorCobrarViewModels = cuentasPorCobrar.Select(c => new CuentaPorCobrarViewModel
                {
                    FidCuenta = c.FidCuenta,
                    FidInquilino = c.FidInquilino,
                    FidInmueble = c.FkidInmueble,
                    Fmonto = c.Fmonto,
                    FfechaInicio = c.FfechaInicio,
                    FdiasGracia = c.FdiasGracia,
                    Factivo = c.Factivo,
                    FtasaMora = c.FtasaMora,
                    Fnota = c.Fnota,
                    FidPeriodoPago = c.FidPeriodoPago
                }).ToList();

                return PartialView("_CuentasPorCobrarPartial", cuentasPorCobrarViewModels);
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(new { success = false, message = argEx.Message });
            }
            catch (DbUpdateException dbEx)
            {
                // Logear si tienes sistema de logs
                return StatusCode(500, new { success = false, message = "Error al guardar en base de datos.", detail = dbEx.Message });
            }
            catch (Exception ex)
            {
                // Logear también aquí
                return StatusCode(500, new { success = false, message = "Ocurrió un error inesperado.", detail = ex.Message });
            }
        }



        // Método para calcular la fecha de vencimiento
        private DateTime CalcularFechaVencimiento(DateTime fechaInicio, int periodoPagoId)
        {
            int dias = 0;

            // Determinar el número de días según el periodo de pago
            switch (periodoPagoId)
            {
                case 1: // Suponiendo que 1 es semanal
                    dias = 7;
                    break;
                case 2: // Suponiendo que 2 es quincenal
                    dias = 15;
                    break;
                case 3: // Suponiendo que 3 es mensual
                    dias = 30;
                    break;
                default:
                    throw new ArgumentException("Periodo de pago no válido");
            }

            // Calcular y devolver la fecha de vencimiento
            return fechaInicio.AddDays(dias);
        }

        [HttpGet]
        public async Task<IActionResult> BuscarInquilinos(string searchTerm = null)
        {
            var inquilinosQuery = _context.TbInquilinos.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                inquilinosQuery = inquilinosQuery.Where(i =>
                    i.Fnombre.Contains(searchTerm) || i.Fapellidos.Contains(searchTerm));
            }

            var resultados = await inquilinosQuery
                .Select(i => new
                {
                    id = i.FidInquilino,
                    text = $"{i.Fnombre} {i.Fapellidos}",
                    tipo = "inquilino"
                })
                .ToListAsync();

            return Json(new { results = resultados });
        }

        [HttpGet]
        public async Task<IActionResult> BuscarInmuebles(string searchTerm = null)
        {
            var inmueblesQuery = _context.TbInmuebles.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                inmueblesQuery = inmueblesQuery.Where(m =>
                    m.Fdescripcion.Contains(searchTerm) ||
                    m.Fdireccion.Contains(searchTerm) ||
                    m.Fubicacion.Contains(searchTerm));
            }

            var resultados = await inmueblesQuery
                .Select(m => new
                {
                    id = m.FidInmueble,
                    text = $"{m.Fdireccion} - Ubicación: {m.Fubicacion}",
                    tipo = "inmueble"
                })
                .ToListAsync();

            return Json(new { results = resultados });
        }

        // GET: TbCuentasPorCobrar/Edit/{id}
        [HttpGet]
        [Authorize(Policy = "Permissions.CxC.Editar")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tbCxc = await _context.TbCxcs.FindAsync(id);
            if (tbCxc == null)
            {
                return NotFound();
            }

            // Asegurarse de que la fecha no tenga componente de hora
            if (tbCxc.FfechaInicio != null)
            {
                tbCxc.FfechaInicio = tbCxc.FfechaInicio.Date;
            }

            // Obtener el nombre del Inquilino
            var inquilino = await _context.TbInquilinos
                .Where(p => p.FidInquilino == tbCxc.FidInquilino)
                .Select(p => p.Fnombre)
                .FirstOrDefaultAsync();

            // Obtener información del inmueble si existe
            string inmuebleDesc = string.Empty;
            if (tbCxc.FkidInmueble.HasValue)
            {
                inmuebleDesc = await _context.TbInmuebles
                    .Where(i => i.FidInmueble == tbCxc.FkidInmueble.Value)
                    .Select(i => i.Fdireccion + "-" + i.Fubicacion)
                    .FirstOrDefaultAsync() ?? string.Empty;
            }

            ViewBag.Inquilino = inquilino;
            ViewBag.Inmueble = inmuebleDesc;

            return PartialView("_EditCxcPartial", tbCxc);
        }

        [HttpPost]
        [Authorize(Policy = "Permissions.CxC.Editar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FidCuenta,FidInquilino,FkidInmueble,FfechaInicio,Fmonto,FdiasGracia,FtasaMora,Fnota,FidPeriodoPago")] TbCxc tbCxc)
        {
            if (id != tbCxc.FidCuenta)
            {
                _logger.LogWarning("El ID de la cuenta por cobrar no coincide con el ID proporcionado.");
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("El modelo no es válido. Errores: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList());
                return Json(new
                {
                    success = false,
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            try
            {
                // 1: Eliminar TbCobrosDesgloses relacionados
                var cobrosDesgloses = await _context.TbCobrosDesgloses
                    .Where(cd => _context.TbCobros
                        .Where(c => c.FidCobro == cd.FkidCobro && c.FkidCxc == id)
                        .Any())
                    .ToListAsync();

                if (cobrosDesgloses.Any())
                {
                    _context.TbCobrosDesgloses.RemoveRange(cobrosDesgloses);
                    await _context.SaveChangesAsync();
                }

                // 2: Eliminar TbCobrosDetalles relacionados
                var cobrosDetalles = await _context.TbCobrosDetalles
                    .Where(cdt => _context.TbCobros
                        .Where(c => c.FidCobro == cdt.FkidCobro && c.FkidCxc == id)
                        .Any())
                    .ToListAsync();

                if (cobrosDetalles.Any())
                {
                    _context.TbCobrosDetalles.RemoveRange(cobrosDetalles);
                    await _context.SaveChangesAsync();
                }

                // 3: Eliminar TbCobros relacionados
                var cobros = await _context.TbCobros
                    .Where(c => c.FkidCxc == id)
                    .ToListAsync();

                if (cobros.Any())
                {
                    _context.TbCobros.RemoveRange(cobros);
                    await _context.SaveChangesAsync();
                }

                // 4: Eliminar la cuota relacionada
                var cuotaExistente = await _context.TbCxcCuota
                    .Where(c => c.FidCxc == id)
                    .OrderByDescending(c => c.FNumeroCuota)
                    .FirstOrDefaultAsync();

                if (cuotaExistente != null)
                {
                    _context.TbCxcCuota.Remove(cuotaExistente);
                    await _context.SaveChangesAsync();
                }

                // 5: Actualizar la cuenta por cobrar
                var existingCxc = await _context.TbCxcs.FindAsync(id);
                if (existingCxc == null)
                {
                    _logger.LogWarning("No se encontró la cuenta por cobrar con ID: {Id}", id);
                    return NotFound();
                }

                existingCxc.FidInquilino = tbCxc.FidInquilino;
                existingCxc.FkidInmueble = tbCxc.FkidInmueble;
                existingCxc.FfechaInicio = tbCxc.FfechaInicio;
                existingCxc.Fmonto = tbCxc.Fmonto;
                existingCxc.FdiasGracia = tbCxc.FdiasGracia;
                existingCxc.FtasaMora = tbCxc.FtasaMora;
                existingCxc.Fnota = tbCxc.Fnota;
                existingCxc.FidPeriodoPago = tbCxc.FidPeriodoPago;

                _context.Update(existingCxc);
                await _context.SaveChangesAsync();

                // 6: Crear nueva cuota
                var numeroCuotaMaxima = await _context.TbCxcCuota
                    .Where(c => c.FidCxc == existingCxc.FidCuenta)
                    .MaxAsync(c => (int?)c.FNumeroCuota) ?? 0;

                var fechaVencimiento = CalcularFechaVencimiento(existingCxc.FfechaInicio, existingCxc.FidPeriodoPago);

                var nuevaCuota = new TbCxcCuotum
                {
                    FidCxc = existingCxc.FidCuenta,
                    FNumeroCuota = numeroCuotaMaxima + 1,
                    Fvence = fechaVencimiento,
                    Fmonto = (int)existingCxc.Fmonto,
                    Fsaldo = existingCxc.Fmonto,
                    Fmora = existingCxc.FtasaMora,
                    FfechaUltCalculo = fechaVencimiento,
                    Factivo = true,
                    Fstatus = 'N'
                };

                _context.TbCxcCuota.Add(nuevaCuota);
                await _context.SaveChangesAsync();

                // 7: Cargar las cuentas por cobrar actualizadas
                var cuentasPorCobrar = await _context.TbCxcs.ToListAsync();
                var cuentasPorCobrarViewModels = cuentasPorCobrar.Select(c => new CuentaPorCobrarViewModel
                {
                    FidCuenta = c.FidCuenta,
                    FidInquilino = c.FidInquilino,
                    FidInmueble = c.FkidInmueble,
                    Fmonto = c.Fmonto,
                    FfechaInicio = c.FfechaInicio,
                    FdiasGracia = c.FdiasGracia,
                    Factivo = c.Factivo,
                    FtasaMora = c.FtasaMora,
                    Fnota = c.Fnota,
                    FidPeriodoPago = c.FidPeriodoPago
                }).ToList();

                return PartialView("_CuentasPorCobrarPartial", cuentasPorCobrarViewModels);
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "Error de argumento: {Message}", argEx.Message);
                return BadRequest(new { success = false, message = argEx.Message });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Error al guardar en la base de datos: {Message}", dbEx.Message);
                return StatusCode(500, new { success = false, message = "Error al guardar en base de datos.", detail = dbEx.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrió un error inesperado: {Message}", ex.Message);
                return StatusCode(500, new { success = false, message = "Ocurrió un error inesperado.", detail = ex.Message });
            }
        }

    }
}