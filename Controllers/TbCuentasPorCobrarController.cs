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
    public class TbCuentasPorCobrarController : BaseController
    {
        private readonly ILogger<GeneradorDeCuotasService> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public TbCuentasPorCobrarController(ApplicationDbContext context,
        ILogger<GeneradorDeCuotasService> logger,
        UserManager<IdentityUser> userManager) : base(context)
        {
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
                    .Where(c => c.FkidCxc == id && c.Fstatus != 'S')
                    .ToListAsync();

                // Actualizar todas las cuotas relacionadas
                foreach (var cuota in cuotas)
                {
                    cuota.Fstatus = 'S'; // Cancelada
                    cuota.Fsaldo = 0;   // Saldo a cero
                    _context.TbCxcCuota.Update(cuota);
                }

                // Actualizar estado del cobro principal (solo el estado)
                cxC.Fstatus = 'S'; // Cancelada
                _context.TbCxcs.Update(cxC);

                // Crear nuevo registro en TbCxcNulo con los datos de anulación
                var cxcNulo = new TbCxcNulo
                {
                    FkidCuenta = id,
                    FkidUsuario = usuario?.FidUsuario ?? cxC.FkidUsuario,
                    Fhora = TimeOnly.FromDateTime(DateTime.Now),
                    FmotivoAnulacion = request.MotivoCancelacion,
                    FfechaAnulacion = DateOnly.FromDateTime(DateTime.Now)
                };

                _context.TbCxcNulos.Add(cxcNulo);

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


        // GET: Carga vista principal
        public IActionResult Index(string vista = "crear")
        {
            ViewData["Vista"] = vista;
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
                var inmuebles = await _context.TbInmuebles.ToListAsync();
                var periodos = await _context.PeriodosPagos.ToListAsync();

                var cuentasPorCobrarViewModels = new List<CuentaPorCobrarViewModel>();

                foreach (var c in cuentas)
                {
                    // Obtener la última cuota de la cuenta (mantenemos esta consulta separada)
                    var ultimaCuota = await _context.TbCxcCuota
                        .Where(q => q.FkidCxc == c.FidCuenta)
                        .OrderByDescending(q => q.Fvence)
                        .FirstOrDefaultAsync();

                    cuentasPorCobrarViewModels.Add(new CuentaPorCobrarViewModel
                    {
                        FidCuenta = c.FidCuenta,
                        FidInquilino = c.FkidInquilino,
                        InquilinoNombre = c.FkidInquilino.HasValue
                            ? inquilinos.FirstOrDefault(i => i.FidInquilino == c.FkidInquilino)?.Fnombre + " " +
                              inquilinos.FirstOrDefault(i => i.FidInquilino == c.FkidInquilino)?.Fapellidos
                            : "Desconocido",
                        FidInmueble = c.FkidInmueble,
                        Fmonto = c.Fmonto,
                        FfechaInicio = c.FfechaInicio,
                        FdiasGracia = c.FdiasGracia,
                        Factivo = c.Factivo,
                        FtasaMora = c.FtasaMora,
                        Fnota = c.Fnota,
                        Fstatus = c.Fstatus,
                        FidPeriodoPago = c.FkidPeriodoPago,
                        NombrePeriodoPago = periodos.FirstOrDefault(p => p.FidPeriodoPago == c.FkidPeriodoPago)?.Fnombre ?? "Desconocido",
                        FfechaProxCuota = c.FfechaProxCuota,
                        Fdescripcion = c.FkidInmueble.HasValue
                            ? inmuebles.FirstOrDefault(i => i.FidInmueble == c.FkidInmueble)?.Fdescripcion
                            : "Sin inmueble",
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
                Value = p.FidPeriodoPago.ToString(),
                Text = p.Fnombre,
                Selected = p.Fnombre.Equals("Mensual", StringComparison.OrdinalIgnoreCase) // Seleccionar el periodo "Mensual" por defecto
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
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, errors = errors });
            }

            try
            {
                var identityId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(identityId))
                    return Json(new { success = false, message = "El usuario no está autenticado." });

                var usuario = await _context.TbUsuarios.FirstOrDefaultAsync(u => u.IdentityId == identityId);
                if (usuario == null)
                    return Json(new { success = false, message = $"No se encontró un usuario con el IdentityId: {identityId}" });

                // Configuración básica de la cuenta
                tbCxc.Fnota = string.IsNullOrEmpty(tbCxc.Fnota) ? "No se proporcionaron notas adicionales" : tbCxc.Fnota;
                tbCxc.FfechaInicio = tbCxc.FfechaInicio.Date;
                tbCxc.FkidUsuario = usuario.FidUsuario;
                tbCxc.Factivo = true;
                tbCxc.Fstatus = 'N';

                // Validación adicional: Verificar si ya existe una CxC activa para el mismo inquilino e inmueble
                var existeCxcActiva = await _context.TbCxcs
                    .AnyAsync(c => c.FkidInmueble == tbCxc.FkidInmueble && c.Factivo);

                if (existeCxcActiva)
                {
                    return Json(new { success = false, message = "Ya existe una cuenta por cobrar activa para este inmueble seleccionado." });
                }

                _context.Add(tbCxc);
                await _context.SaveChangesAsync();

                // Generar cuotas
                var fechaActual = DateTime.Now.Date;
                var fechaInicio = tbCxc.FfechaInicio;
                bool esUltimoDiaMes = fechaInicio.Day == DateTime.DaysInMonth(fechaInicio.Year, fechaInicio.Month);

                int numeroCuota = 1;
                decimal saldoPendiente = tbCxc.Fmonto;
                var fechasVencimiento = new List<DateTime>();
                DateTime fechaProxCuota = DateTime.MinValue;

                // Primera cuota siempre vence un mes después de la fecha de inicio
                DateTime fechaVencimiento = CalcularSiguienteVencimiento(fechaInicio, esUltimoDiaMes);
                fechasVencimiento.Add(fechaVencimiento);

                // Si la fecha de inicio es en el pasado, generar cuotas hasta el mes actual + 1 mes adicional
                if (fechaInicio < fechaActual)
                {
                    DateTime siguienteVencimiento = CalcularSiguienteVencimiento(fechaVencimiento, esUltimoDiaMes);

                    // Calculamos la fecha límite (mes actual + 1 mes adicional)
                    DateTime fechaLimite = new DateTime(fechaActual.Year, fechaActual.Month, 1)
                        .AddMonths(2)
                        .AddDays(-1); // Último día del mes siguiente

                    while (siguienteVencimiento <= fechaLimite)
                    {
                        fechasVencimiento.Add(siguienteVencimiento);
                        siguienteVencimiento = CalcularSiguienteVencimiento(siguienteVencimiento, esUltimoDiaMes);
                    }
                }

                // La próxima cuota es siempre el siguiente vencimiento después del último generado
                fechaProxCuota = CalcularSiguienteVencimiento(fechasVencimiento.Last(), esUltimoDiaMes);

                // Generar las cuotas para cada fecha de vencimiento
                foreach (var fecha in fechasVencimiento)
                {
                    // Determinar el estado basado en la fecha de vencimiento
                    char estado = fecha < fechaActual ? 'V' : 'N';

                    var cuota = new TbCxcCuotum
                    {
                        FkidCxc = tbCxc.FidCuenta,
                        FNumeroCuota = numeroCuota,
                        Fvence = fecha,
                        Fmonto = (int)tbCxc.Fmonto,
                        Fsaldo = saldoPendiente,
                        Fmora = 0,
                        FfechaUltCalculo = fecha,
                        Factivo = true,
                        Fstatus = estado
                    };

                    _context.TbCxcCuota.Add(cuota);
                    numeroCuota++;
                }

                // Asignar la fecha de próxima cuota
                tbCxc.FfechaProxCuota = fechaProxCuota;

                _context.Update(tbCxc);
                await _context.SaveChangesAsync();

                var cuentas = await (from c in _context.TbCxcs
                                     join i in _context.TbInquilinos on c.FkidInquilino equals i.FidInquilino
                                     join p in _context.PeriodosPagos on c.FkidPeriodoPago equals p.FidPeriodoPago
                                     select new
                                     {
                                         Cxc = c,
                                         InquilinoNombre = $"{i.Fnombre} {i.Fapellidos}",
                                         PeriodoNombre = p.Fnombre
                                     })
                                .ToListAsync();

                var cuentasPorCobrarViewModels = cuentas.Select(x => new CuentaPorCobrarViewModel
                {
                    FidCuenta = x.Cxc.FidCuenta,
                    FidInquilino = x.Cxc.FkidInquilino,
                    InquilinoNombre = x.InquilinoNombre,
                    FidInmueble = x.Cxc.FkidInmueble,
                    Fmonto = x.Cxc.Fmonto,
                    FfechaInicio = x.Cxc.FfechaInicio,
                    FdiasGracia = x.Cxc.FdiasGracia,
                    Factivo = x.Cxc.Factivo,
                    FtasaMora = x.Cxc.FtasaMora,
                    Fnota = x.Cxc.Fnota,
                    FidPeriodoPago = x.Cxc.FkidPeriodoPago,
                    NombrePeriodoPago = x.PeriodoNombre,
                    FfechaProxCuota = x.Cxc.FfechaProxCuota
                }).ToList();

                return Json(new
                {
                    success = true,
                    message = "Cuenta por cobrar creada correctamente.",
                    data = cuentasPorCobrarViewModels
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Método auxiliar para calcular la siguiente fecha de vencimiento (sin cambios)
        private DateTime CalcularSiguienteVencimiento(DateTime fechaActual, bool esUltimoDiaMes)
        {
            if (esUltimoDiaMes)
            {
                // Lógica para último día del mes
                var fecha = fechaActual.AddMonths(1);
                return new DateTime(
                    fecha.Year,
                    fecha.Month,
                    DateTime.DaysInMonth(fecha.Year, fecha.Month)
                );
            }
            else
            {
                // Mantener el mismo día numérico, ajustando si no existe
                var mesSiguiente = fechaActual.AddMonths(1);
                int dia = fechaActual.Day;
                int ultimoDiaMesSiguiente = DateTime.DaysInMonth(mesSiguiente.Year, mesSiguiente.Month);

                return new DateTime(
                    mesSiguiente.Year,
                    mesSiguiente.Month,
                    Math.Min(dia, ultimoDiaMesSiguiente)
                );
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
                    text = $"{m.Fdescripcion} - {m.Fdireccion} - Ubicación: {m.Fubicacion}",
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
                .Where(p => p.FidInquilino == tbCxc.FkidInquilino)
                .Select(p => p.Fnombre)
                .FirstOrDefaultAsync();

            // Obtener información del inmueble si existe
            string inmuebleDesc = string.Empty;
            if (tbCxc.FkidInmueble.HasValue)
            {
                inmuebleDesc = await _context.TbInmuebles
                    .Where(i => i.FidInmueble == tbCxc.FkidInmueble.Value)
                    .Select(i => i.Fdescripcion)
                    .FirstOrDefaultAsync() ?? string.Empty;
            }

            // Cargar los periodos de pago desde la base de datos
            var periodosPagos = await _context.PeriodosPagos.ToListAsync();
            var inquilinosSelect = await _context.TbInquilinos.ToListAsync();
            var inmueblesSelect = await _context.TbInmuebles.ToListAsync();

            // Crear una lista de opciones con los periodos de pago
            var opciones = periodosPagos.Select(p => new SelectListItem
            {
                Value = p.FidPeriodoPago.ToString(),
                Text = p.Fnombre,
                Selected = p.FidPeriodoPago == tbCxc.FkidPeriodoPago // Seleccionar el periodo correspondiente
            }).ToList();

            var opcionesInquilino = inquilinosSelect.Select(p => new SelectListItem
            {
                Value = p.FidInquilino.ToString(),
                Text = p.Fnombre + " " + p.Fapellidos,
                Selected = p.FidInquilino == tbCxc.FkidInquilino // Seleccionar el periodo correspondiente
            }).ToList();

            var opcionesInmueble = inmueblesSelect.Select(p => new SelectListItem
            {
                Value = p.FidInmueble.ToString(),
                Text = p.Fdescripcion,
                Selected = p.FidInmueble == tbCxc.FkidInmueble // Seleccionar el periodo correspondiente
            }).ToList();

            // Crear el SelectList para el ViewBag
            ViewBag.FkidPeriodoPago = opciones;
            ViewBag.FkidInquilino = opcionesInquilino;
            ViewBag.FkidInmueble = opcionesInmueble;

            return PartialView("_EditCxcPartial", tbCxc);
        }


        [HttpPost]
        [Authorize(Policy = "Permissions.CxC.Editar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FidCuenta,FkidInquilino,FkidInmueble,FfechaInicio,Fmonto,FdiasGracia,FtasaMora,Fnota,FkidPeriodoPago")] TbCxc tbCxc)
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
                // 1: Eliminar relaciones existentes (cobros y cuotas)
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

                var cobros = await _context.TbCobros
                    .Where(c => c.FkidCxc == id)
                    .ToListAsync();

                if (cobros.Any())
                {
                    _context.TbCobros.RemoveRange(cobros);
                    await _context.SaveChangesAsync();
                }

                var cuotasExistentes = await _context.TbCxcCuota
                    .Where(c => c.FkidCxc == id)
                    .ToListAsync();

                if (cuotasExistentes.Any())
                {
                    _context.TbCxcCuota.RemoveRange(cuotasExistentes);
                    await _context.SaveChangesAsync();
                }

                // 2: Actualizar la cuenta por cobrar
                var existingCxc = await _context.TbCxcs.FindAsync(id);
                if (existingCxc == null)
                {
                    _logger.LogWarning("No se encontró la cuenta por cobrar con ID: {Id}", id);
                    return NotFound();
                }

                existingCxc.FkidInquilino = tbCxc.FkidInquilino;
                existingCxc.FkidInmueble = tbCxc.FkidInmueble;
                existingCxc.FfechaInicio = tbCxc.FfechaInicio.Date;
                existingCxc.Fmonto = tbCxc.Fmonto;
                existingCxc.FdiasGracia = tbCxc.FdiasGracia;
                existingCxc.FtasaMora = tbCxc.FtasaMora;
                existingCxc.Fnota = tbCxc.Fnota;
                existingCxc.FkidPeriodoPago = tbCxc.FkidPeriodoPago;

                _context.Update(existingCxc);
                await _context.SaveChangesAsync();

                // 3: Generar nuevas cuotas
                var fechaActual = DateTime.Now.Date;
                var fechaInicio = existingCxc.FfechaInicio;
                bool esUltimoDiaMes = fechaInicio.Day == DateTime.DaysInMonth(fechaInicio.Year, fechaInicio.Month);

                int numeroCuota = 1;
                decimal saldoPendiente = existingCxc.Fmonto;
                var fechasVencimiento = new List<DateTime>();
                DateTime fechaVencimiento;
                DateTime fechaProxCuota;

                // Determinar si la fecha de inicio está en el mismo mes que la fecha actual
                bool mismoMes = fechaInicio.Year == fechaActual.Year && fechaInicio.Month == fechaActual.Month;

                if (mismoMes)
                {
                    // Si está en el mismo mes, primera cuota vence en un mes
                    fechaVencimiento = CalcularSiguienteVencimiento(fechaInicio, esUltimoDiaMes);
                    fechasVencimiento.Add(fechaVencimiento);
                    fechaProxCuota = CalcularSiguienteVencimiento(fechaVencimiento, esUltimoDiaMes);
                }
                else
                {
                    // Si está en un mes anterior, empezar desde la fecha de inicio
                    fechaVencimiento = fechaInicio;

                    // Generar todas las cuotas hasta el mes actual
                    while (fechaVencimiento <= fechaActual ||
                          (fechaVencimiento.Year == fechaActual.Year &&
                           fechaVencimiento.Month == fechaActual.Month))
                    {
                        fechasVencimiento.Add(fechaVencimiento);
                        fechaVencimiento = CalcularSiguienteVencimiento(fechaVencimiento, esUltimoDiaMes);
                    }
                    fechaProxCuota = fechaVencimiento;
                }

                // Generar las cuotas para cada fecha de vencimiento
                foreach (var fecha in fechasVencimiento)
                {
                    // Determinar el estado basado en la fecha de vencimiento
                    char estado = fecha < fechaActual ? 'V' : 'N';

                    var cuota = new TbCxcCuotum
                    {
                        FkidCxc = existingCxc.FidCuenta,
                        FNumeroCuota = numeroCuota,
                        Fvence = fecha,
                        Fmonto = (int)existingCxc.Fmonto,
                        Fsaldo = saldoPendiente,
                        Fmora = existingCxc.FtasaMora,
                        FfechaUltCalculo = fecha,
                        Factivo = true,
                        Fstatus = estado
                    };

                    _context.TbCxcCuota.Add(cuota);
                    numeroCuota++;
                }

                // Asignar la fecha de próxima cuota
                existingCxc.FfechaProxCuota = fechaProxCuota;
                _context.Update(existingCxc);
                await _context.SaveChangesAsync();

                var cuentas = await (from c in _context.TbCxcs
                                     join i in _context.TbInquilinos on c.FkidInquilino equals i.FidInquilino
                                     join p in _context.PeriodosPagos on c.FkidPeriodoPago equals p.FidPeriodoPago
                                     select new
                                     {
                                         Cxc = c,
                                         InquilinoNombre = $"{i.Fnombre} {i.Fapellidos}",
                                         PeriodoNombre = p.Fnombre
                                     })
                                 .ToListAsync();

                var cuentasPorCobrarViewModels = cuentas.Select(x => new CuentaPorCobrarViewModel
                {
                    FidCuenta = x.Cxc.FidCuenta,
                    FidInquilino = x.Cxc.FkidInquilino,
                    InquilinoNombre = x.InquilinoNombre,
                    FidInmueble = x.Cxc.FkidInmueble,
                    Fmonto = x.Cxc.Fmonto,
                    FfechaInicio = x.Cxc.FfechaInicio,
                    FdiasGracia = x.Cxc.FdiasGracia,
                    Factivo = x.Cxc.Factivo,
                    FtasaMora = x.Cxc.FtasaMora,
                    Fnota = x.Cxc.Fnota,
                    FidPeriodoPago = x.Cxc.FkidPeriodoPago,
                    NombrePeriodoPago = x.PeriodoNombre,
                    FfechaProxCuota = x.Cxc.FfechaProxCuota
                }).ToList();

                return Json(new
                {
                    success = true,
                    message = "Cuenta por cobrar actualizada correctamente.",
                    data = cuentasPorCobrarViewModels
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrió un error inesperado: {Message}", ex.Message);
                return StatusCode(500, new { success = false, message = "Ocurrió un error inesperado.", detail = ex.Message });
            }
        }
    }
}