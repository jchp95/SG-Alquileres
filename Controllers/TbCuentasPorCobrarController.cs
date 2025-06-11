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
            try
            {
                var cxC = await _context.TbCxcs.FindAsync(id);
                if (cxC == null)
                {
                    _logger.LogWarning($"Intento de cambiar estado de la CxC no encontrado: {id}");
                    return NotFound();
                }

                // Cambiar el estado del inmueble
                cxC.Factivo = !cxC.Factivo;
                _context.Update(cxC);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Estado de la CxC {id} cambiado a: {cxC.Factivo}");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cambiar estado de la CxC {id}");
                return StatusCode(500, "Error interno al cambiar el estado");
            }
        }

        [HttpPost]
        [Authorize(Policy = "Permissions.CxC.Cancelar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarCxC(int id, [FromBody] CancelarCxCRequest request)
        {
            try
            {
                // Validar contraseña primero
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized(new { success = false, message = "Usuario no autenticado." });

                var isValid = await _userManager.CheckPasswordAsync(user, request.Password);
                if (!isValid)
                    return BadRequest(new { success = false, message = "Contraseña incorrecta." });

                // Validar motivo de anulación
                if (string.IsNullOrWhiteSpace(request.MotivoCancelacion))
                    return BadRequest(new { success = false, message = "Debe especificar un motivo para la cancelación." });

                // Obtener el cobro principal
                var cxC = await _context.TbCxcs.FindAsync(id);

                if (cxC == null)
                    return NotFound(new { success = false, message = "CxC no encontrada." });

                // Verificar si ya está anulado
                if (cxC.Fstatus == 'S')
                    return BadRequest(new { success = false, message = "La CxC ya está cancelada." });

                // Obtener usuario actual
                var identityId = _userManager.GetUserId(User);
                var usuario = await _context.TbUsuarios.FirstOrDefaultAsync(u => u.IdentityId == identityId);

                // Obtener todas las cuotas asociadas a esta CxC que no estén ya canceladas
                var cuotas = await _context.TbCxcCuota
                    .Where(c => c.FidCxc == id && c.Fstatus != 'S')
                    .ToListAsync();

                // Actualizar todas las cuotas relacionadas
                foreach (var cuota in cuotas)
                {
                    cuota.Fstatus = 'S'; // Cancelada
                    cuota.Fsaldo = 0;   // Saldo a cero
                    _context.TbCxcCuota.Update(cuota);
                }

                // Actualizar datos de anulación del cobro principal
                cxC.FmotivoCancelacion = request.MotivoCancelacion;
                cxC.FfechaCancelacion = DateOnly.FromDateTime(DateTime.Now);
                cxC.FkidUsuario = usuario?.FidUsuario ?? cxC.FkidUsuario;
                cxC.Fstatus = 'S'; // Cancelada

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "CxC y sus cuotas canceladas exitosamente.",
                    cuotasCanceladas = cuotas.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar la CxC.");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno al cancelar la CxC.",
                    error = ex.Message
                });
            }
        }

        public class CancelarCxCRequest
        {
            public string MotivoCancelacion { get; set; }
            public string Password { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> GetUsuarioActual()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized(new { success = false, message = "Usuario no autenticado." });

                return Ok(new
                {
                    success = true,
                    usuario = new
                    {
                        UserName = user.UserName
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario actual");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno al obtener usuario",
                    error = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ValidarContrasena([FromBody] ValidarContrasenaRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Password))
                    return BadRequest(new { success = false, message = "La contraseña es requerida." });

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized(new { success = false, message = "Usuario no autenticado." });

                var isValid = await _userManager.CheckPasswordAsync(user, request.Password);

                return Ok(new
                {
                    success = isValid,
                    message = isValid ? "Contraseña válida" : "Contraseña incorrecta"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar contraseña");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno al validar contraseña",
                    error = ex.Message
                });
            }
        }

        public class ValidarContrasenaRequest
        {
            public string Password { get; set; }
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
                // Ordenar las cuentas por cobrar por FidCuenta
                var cuentas = await _context.TbCxcs
                    .OrderBy(c => c.FidCuenta)  // Orden ascendente por defecto
                    .ToListAsync();

                var inquilinos = await _context.TbInquilinos.ToListAsync();
                var periodos = await _context.PeriodosPagos.ToListAsync();

                var cuentasPorCobrarViewModels = new List<CuentaPorCobrarViewModel>();

                foreach (var c in cuentas)
                {
                    // Obtener la última cuota de la cuenta (mantenemos esta consulta separada)
                    var ultimaCuota = await _context.TbCxcCuota
                        .Where(q => q.FidCxc == c.FidCuenta)
                        .OrderByDescending(q => q.Fvence)
                        .FirstOrDefaultAsync();

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
                        Fstatus = c.Fstatus,
                        FidPeriodoPago = c.FidPeriodoPago,
                        NombrePeriodoPago = periodos.FirstOrDefault(p => p.Id == c.FidPeriodoPago)?.Nombre ?? "Desconocido",
                        FfechaProxCuota = c.FfechaProxCuota,
                    });
                }

                return PartialView("_CuentasPorCobrarPartial", cuentasPorCobrarViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar cuentas por cobrar");
                return PartialView("_CuentasPorCobrarPartial", new List<CuentaPorCobrarViewModel>());
            }
        }

        // Método para calcular la fecha de la próxima cuota
        private DateTime CalcularFechaProxCuota(DateTime fechaVencimiento, int periodoPagoId)
        {
            int dias = 0;

            switch (periodoPagoId)
            {
                case 1: dias = 7; break;
                case 2: dias = 15; break;
                case 3: dias = 30; break;
                default: throw new ArgumentException("Periodo de pago no válido");
            }

            return fechaVencimiento.AddDays(dias);
        }

        // GET: Cargar formulario de creación
        [HttpGet]
        [Authorize(Policy = "Permissions.CxC.Crear")]
        public async Task<IActionResult> Create()
        {
            // Cargar los periodos de pago desde la base de datos
            var periodosPagos = await _context.PeriodosPagos.ToListAsync();

            // Verificar si hay periodos de pago disponibles
            if (periodosPagos == null || !periodosPagos.Any())
            {
                // Manejar el caso donde no hay periodos de pago disponibles
                ModelState.AddModelError("", "No hay periodos de pago disponibles.");
                return PartialView("_CreateCuentasPorCobrarPartial");
            }

            // Crear una lista de opciones con los periodos de pago
            var opciones = periodosPagos.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Nombre,
                Selected = p.Nombre.Equals("Mensual", StringComparison.OrdinalIgnoreCase) // Seleccionar el periodo "Mensual" por defecto
            }).ToList();

            // Crear el SelectList para el ViewBag
            ViewBag.FidPeriodoPago = opciones;

            // Retornar la vista parcial
            return PartialView("_CreateCuentasPorCobrarPartial");
        }

        [HttpPost]
        [Authorize(Policy = "Permissions.CxC.Crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TbCxc tbCxc)
        {
            // Loggear el estado del modelo
            Console.WriteLine($"ModelState isValid: {ModelState.IsValid}");
            foreach (var state in ModelState)
            {
                Console.WriteLine($"{state.Key}: {state.Value.RawValue}");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                Console.WriteLine("Errores de validación:");
                foreach (var error in errors)
                {
                    Console.WriteLine(error);
                }
                return Json(new { success = false, errors = errors });
            }

            try
            {
                // Verificar datos recibidos
                Console.WriteLine($"Datos recibidos - Inquilino: {tbCxc.FidInquilino}, Inmueble: {tbCxc.FkidInmueble}, Monto: {tbCxc.Fmonto}");

                var identityId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(identityId))
                    return Json(new { success = false, message = "El usuario no está autenticado." });

                var usuario = await _context.TbUsuarios.FirstOrDefaultAsync(u => u.IdentityId == identityId);
                if (usuario == null)
                    return Json(new { success = false, message = $"No se encontró un usuario con el IdentityId: {identityId}" });

                // Configuración básica de la cuenta
                if (string.IsNullOrEmpty(tbCxc.Fnota))
                {
                    tbCxc.Fnota = "No se proporcionaron notas adicionales";
                }

                tbCxc.FfechaInicio = tbCxc.FfechaInicio.Date.Add(DateTime.Now.TimeOfDay);
                tbCxc.FkidUsuario = usuario.FidUsuario;
                tbCxc.Factivo = true;
                tbCxc.Fstatus = 'N';

                // Loggear antes de guardar
                Console.WriteLine("Intentando guardar la cuenta por cobrar...");

                _context.Add(tbCxc);
                var result = await _context.SaveChangesAsync();
                Console.WriteLine($"SaveChangesAsync result: {result}");

                // Calcular todas las cuotas necesarias
                var fechaActual = DateTime.Now;
                // MODIFICACIÓN: Fecha límite es el último día del mes actual
                var fechaLimite = new DateTime(fechaActual.Year, fechaActual.Month, DateTime.DaysInMonth(fechaActual.Year, fechaActual.Month));

                var fechaVencimiento = tbCxc.FfechaInicio;
                int numeroCuota = 1;
                decimal saldoPendiente = tbCxc.Fmonto;

                // Lista para almacenar todas las fechas de vencimiento
                var fechasVencimiento = new List<DateTime>();

                // Generar cuotas desde la fecha de inicio hasta la fecha límite
                while (fechaVencimiento <= fechaLimite)
                {
                    if (fechaVencimiento >= tbCxc.FfechaInicio)
                    {
                        fechasVencimiento.Add(fechaVencimiento);
                        Console.WriteLine($"Generando cuota para: {fechaVencimiento.ToShortDateString()}");
                    }
                    // ✅ Usar el método que calcula correctamente la siguiente fecha
                    fechaVencimiento = CalcularFechaVencimiento(fechaVencimiento, tbCxc.FidPeriodoPago);
                }

                // Generar las cuotas para cada fecha de vencimiento
                foreach (var fecha in fechasVencimiento)
                {
                    var cuota = new TbCxcCuotum
                    {
                        FidCxc = tbCxc.FidCuenta,
                        FNumeroCuota = numeroCuota,
                        Fvence = fecha,
                        Fmonto = (int)tbCxc.Fmonto,
                        Fsaldo = saldoPendiente,
                        Fmora = tbCxc.FtasaMora,
                        FfechaUltCalculo = fecha,
                        Factivo = true,
                        Fstatus = 'N'
                    };

                    _context.TbCxcCuota.Add(cuota);
                    numeroCuota++;
                }

                // Actualizar la fecha de próxima cuota en la cuenta
                if (fechasVencimiento.Any())
                {
                    // La próxima cuota será un mes después de la última generada
                    tbCxc.FfechaProxCuota = fechasVencimiento.Last().AddMonths(1);
                    Console.WriteLine($"Próxima cuota programada para: {tbCxc.FfechaProxCuota.ToShortDateString()}");
                }
                else
                {
                    // Si no se generaron cuotas, la próxima será un mes después de la fecha de inicio
                    tbCxc.FfechaProxCuota = tbCxc.FfechaInicio.AddMonths(1);
                }

                _context.Update(tbCxc);
                await _context.SaveChangesAsync();

                // Obtener las cuentas actualizadas para devolver en la respuesta
                var cuentas = await (from c in _context.TbCxcs
                                     join i in _context.TbInquilinos on c.FidInquilino equals i.FidInquilino
                                     join p in _context.PeriodosPagos on c.FidPeriodoPago equals p.Id
                                     select new
                                     {
                                         Cxc = c,
                                         InquilinoNombre = $"{i.Fnombre} {i.Fapellidos}",
                                         PeriodoNombre = p.Nombre
                                     })
                                 .ToListAsync();

                var cuentasPorCobrarViewModels = cuentas.Select(x => new CuentaPorCobrarViewModel
                {
                    FidCuenta = x.Cxc.FidCuenta,
                    FidInquilino = x.Cxc.FidInquilino,
                    InquilinoNombre = x.InquilinoNombre,
                    FidInmueble = x.Cxc.FkidInmueble,
                    Fmonto = x.Cxc.Fmonto,
                    FfechaInicio = x.Cxc.FfechaInicio,
                    FdiasGracia = x.Cxc.FdiasGracia,
                    Factivo = x.Cxc.Factivo,
                    FtasaMora = x.Cxc.FtasaMora,
                    Fnota = x.Cxc.Fnota,
                    FidPeriodoPago = x.Cxc.FidPeriodoPago,
                    NombrePeriodoPago = x.PeriodoNombre,
                    FfechaProxCuota = x.Cxc.FfechaProxCuota
                }).ToList();

                // Devolver JSON con éxito y los datos necesarios
                return Json(new
                {
                    success = true,
                    message = "Cuenta por cobrar creada correctamente.",
                    data = cuentasPorCobrarViewModels
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Método para calcular la fecha de vencimiento
        private DateTime CalcularFechaVencimiento(DateTime fechaInicio, int periodoPagoId)
        {
            switch (periodoPagoId)
            {
                case 1: // Semanal
                    return fechaInicio.AddDays(7);
                case 2: // Quincenal
                    return fechaInicio.AddDays(15);
                case 3: // Mensual (siempre último día del mes)
                    return fechaInicio.AddMonths(1);
                default:
                    throw new ArgumentException("Periodo de pago no válido");
            }
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
                    tipo = "inmueble",
                    monto = m.Fprecio // Asegúrate de que este campo exista en tu modelo
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

            // Cargar los periodos de pago desde la base de datos
            var periodosPagos = await _context.PeriodosPagos.ToListAsync();

            // Crear una lista de opciones con los periodos de pago
            var opciones = periodosPagos.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Nombre,
                Selected = p.Id == tbCxc.FidPeriodoPago // Seleccionar el periodo correspondiente
            }).ToList();

            // Crear el SelectList para el ViewBag
            ViewBag.FidPeriodoPago = opciones;

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

                // Obtener las cuentas actualizadas para devolver en la respuesta
                var cuentas = await (from c in _context.TbCxcs
                                     join i in _context.TbInquilinos on c.FidInquilino equals i.FidInquilino
                                     join p in _context.PeriodosPagos on c.FidPeriodoPago equals p.Id
                                     select new
                                     {
                                         Cxc = c,
                                         InquilinoNombre = $"{i.Fnombre} {i.Fapellidos}",
                                         PeriodoNombre = p.Nombre
                                     })
                         .ToListAsync();

                var cuentasPorCobrarViewModels = cuentas.Select(x => new CuentaPorCobrarViewModel
                {
                    FidCuenta = x.Cxc.FidCuenta,
                    FidInquilino = x.Cxc.FidInquilino,
                    InquilinoNombre = x.InquilinoNombre,
                    FidInmueble = x.Cxc.FkidInmueble,
                    Fmonto = x.Cxc.Fmonto,
                    FfechaInicio = x.Cxc.FfechaInicio,
                    FdiasGracia = x.Cxc.FdiasGracia,
                    Factivo = x.Cxc.Factivo,
                    FtasaMora = x.Cxc.FtasaMora,
                    Fnota = x.Cxc.Fnota,
                    FidPeriodoPago = x.Cxc.FidPeriodoPago,
                    NombrePeriodoPago = x.PeriodoNombre,
                    FfechaProxCuota = x.Cxc.FfechaProxCuota
                }).ToList();

                // Devolver JSON con éxito y los datos necesarios
                return Json(new
                {
                    success = true,
                    message = "Cuenta por cobrar creada correctamente.",
                    data = cuentasPorCobrarViewModels
                });
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