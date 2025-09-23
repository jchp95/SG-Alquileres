using System.Globalization;
using System.Text.Json;
using Alquileres.Context;
using Alquileres.DTO;
using Alquileres.Enums;
using Alquileres.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics; // Para Stopwatch y Activity
using System.Diagnostics.CodeAnalysis; // Opcional para Activity

namespace Alquileres.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TbCobrosApiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GeneradorDeCuotasService> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public TbCobrosApiController(ApplicationDbContext context, ILogger<GeneradorDeCuotasService> logger, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        [IgnoreAntiforgeryToken]
        [HttpPost("create")]
        [Authorize(Policy = "Permissions.Cobros.Crear")]
        public async Task<IActionResult> Create([FromBody] CobroRequest request)
        {
            // Identificador único para esta solicitud
            var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("[{TraceId}] ====== INICIO DE SOLICITUD CREATE ======", traceId);
                _logger.LogInformation("[{TraceId}] Headers recibidos: {@Headers}", traceId, Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()));
                _logger.LogInformation("[{TraceId}] Body recibido: {@Body}", traceId, request);
                _logger.LogInformation("[{TraceId}] Usuario autenticado: {User}", traceId, User.Identity?.Name);

                // 1. Validación de usuario
                var identityId = _userManager.GetUserId(User);
                _logger.LogInformation("[{TraceId}] IdentityId del usuario: {IdentityId}", traceId, identityId);

                if (string.IsNullOrEmpty(identityId))
                {
                    _logger.LogWarning("[{TraceId}] El usuario no está autenticado (identityId es nulo o vacío)", traceId);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Usuario no autenticado",
                        Detail = "El usuario no está autenticado correctamente",
                        Status = StatusCodes.Status400BadRequest,
                        Extensions = { ["traceId"] = traceId }
                    });
                }

                var usuario = await _context.TbUsuarios.FirstOrDefaultAsync(u => u.IdentityId == identityId);
                if (usuario == null)
                {
                    _logger.LogWarning("[{TraceId}] No se encontró usuario en TbUsuarios con IdentityId: {IdentityId}", traceId, identityId);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Usuario no encontrado",
                        Detail = $"No se encontró un usuario con el IdentityId: {identityId}",
                        Status = StatusCodes.Status400BadRequest,
                        Extensions = { ["traceId"] = traceId }
                    });
                }

                // 2. Validación del modelo
                _logger.LogInformation("[{TraceId}] Validando modelo...", traceId);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToList()
                    );

                    _logger.LogError("[{TraceId}] Errores de validación del modelo: {@Errors}", traceId, errors);

                    return BadRequest(new ValidationProblemDetails(ModelState)
                    {
                        Title = "Error de validación",
                        Status = StatusCodes.Status400BadRequest,
                        Extensions = { ["traceId"] = traceId }
                    });
                }

                // 3. Validación de cuotas
                _logger.LogInformation("[{TraceId}] Validando cuotas seleccionadas: {@Cuotas}", traceId, request.CuotasSeleccionadas);

                if (request.CuotasSeleccionadas == null || request.CuotasSeleccionadas.Count == 0)
                {
                    _logger.LogWarning("[{TraceId}] No se seleccionaron cuotas", traceId);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Cuotas no seleccionadas",
                        Detail = "Seleccione al menos una cuota",
                        Status = StatusCodes.Status400BadRequest,
                        Extensions = { ["traceId"] = traceId }
                    });
                }

                // 4. Validación de cuenta
                _logger.LogInformation("[{TraceId}] Buscando cuenta con ID: {CuentaId}", traceId, request.FkidCxc);
                var cuenta = await _context.TbCxcs.FirstOrDefaultAsync(c => c.FidCuenta == request.FkidCxc);

                if (cuenta == null)
                {
                    _logger.LogWarning("[{TraceId}] Cuenta no encontrada con ID: {CuentaId}", traceId, request.FkidCxc);
                    return NotFound(new ProblemDetails
                    {
                        Title = "Cuenta no encontrada",
                        Detail = $"No se encontró la cuenta con ID: {request.FkidCxc}",
                        Status = StatusCodes.Status404NotFound,
                        Extensions = { ["traceId"] = traceId }
                    });
                }

                // 5. Validación de comprobante fiscal
                _logger.LogInformation("[{TraceId}] Validando comprobante fiscal: {Comprobante}", traceId, request.ComprobanteFiscalSeleccionado);

                var comprobanteFiscal = await _context.TbComprobantesFiscales
                    .FirstOrDefaultAsync(c => c.Fcomprobante == request.ComprobanteFiscalSeleccionado);

                if (comprobanteFiscal == null)
                {
                    _logger.LogWarning("[{TraceId}] Comprobante fiscal no válido: {Comprobante}", traceId, request.ComprobanteFiscalSeleccionado);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Comprobante fiscal inválido",
                        Detail = $"El comprobante fiscal {request.ComprobanteFiscalSeleccionado} no es válido",
                        Status = StatusCodes.Status400BadRequest,
                        Extensions = { ["traceId"] = traceId }
                    });
                }

                // 6. Validación de montos
                _logger.LogInformation("[{TraceId}] Validando montos ingresados...", traceId);

                decimal montoIngresado = request.Fefectivo + request.Ftransferencia +
                                       request.Ftarjeta + request.FnotaCredito + request.Fcheque +
                                       request.Fdeposito + request.FdebitoAutomatico;

                _logger.LogInformation("[{TraceId}] Monto total ingresado: {Monto}", traceId, montoIngresado);

                if (montoIngresado <= 0)
                {
                    _logger.LogWarning("[{TraceId}] Monto ingresado no válido: {Monto}", traceId, montoIngresado);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Monto inválido",
                        Detail = "Debe ingresar al menos un monto en el desglose de pago",
                        Status = StatusCodes.Status400BadRequest,
                        Extensions = { ["traceId"] = traceId }
                    });
                }

                // 7. Validación de cuotas pendientes
                _logger.LogInformation("[{TraceId}] Buscando cuotas pendientes para cuenta ID: {CuentaId}", traceId, request.FkidCxc);

                var todasCuotasPendientes = await _context.TbCxcCuota
                    .Where(c => c.FkidCxc == request.FkidCxc && c.Fstatus != 'S')
                    .OrderBy(c => c.Fvence)
                    .ToListAsync();

                _logger.LogInformation("[{TraceId}] Cuotas pendientes encontradas: {Count}", traceId, todasCuotasPendientes.Count);

                if (todasCuotasPendientes.Count == 0)
                {
                    _logger.LogWarning("[{TraceId}] No hay cuotas pendientes para la cuenta ID: {CuentaId}", traceId, request.FkidCxc);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "No hay cuotas pendientes",
                        Detail = "No existen cuotas pendientes para esta cuenta",
                        Status = StatusCodes.Status400BadRequest,
                        Extensions = { ["traceId"] = traceId }
                    });
                }

                // 8. Validación de cuotas seleccionadas vs existentes
                var cuotasSeleccionadas = todasCuotasPendientes
                    .Where(c => request.CuotasSeleccionadas.Contains(c.FNumeroCuota))
                    .OrderBy(c => c.Fvence)
                    .ToList();

                _logger.LogInformation("[{TraceId}] Cuotas seleccionadas válidas: {@Cuotas}", traceId,
                    cuotasSeleccionadas.Select(c => new { c.FNumeroCuota, c.Fvence, c.Fsaldo }));

                if (cuotasSeleccionadas.Count != request.CuotasSeleccionadas.Count)
                {
                    var cuotasNoEncontradas = request.CuotasSeleccionadas.Except(cuotasSeleccionadas.Select(c => c.FNumeroCuota));
                    _logger.LogWarning("[{TraceId}] Algunas cuotas seleccionadas no existen o ya están pagadas: {@CuotasNoEncontradas}",
                        traceId, cuotasNoEncontradas);

                    return BadRequest(new ProblemDetails
                    {
                        Title = "Cuotas no válidas",
                        Detail = $"Las siguientes cuotas no existen o ya están pagadas: {string.Join(", ", cuotasNoEncontradas)}",
                        Status = StatusCodes.Status400BadRequest,
                        Extensions = {
                    ["traceId"] = traceId,
                    ["cuotasNoEncontradas"] = cuotasNoEncontradas
                }
                    });
                }

                // 9. Procesamiento de descuentos
                _logger.LogInformation("[{TraceId}] Procesando descuentos...", traceId);
                decimal moraTotal = request.Fmora;
                decimal descuentoTotal = request.Fdescuento;
                decimal descuentoAplicadoAMora = 0;
                decimal descuentoAplicadoACapital = 0;

                if (descuentoTotal > 0 && moraTotal > 0)
                {
                    descuentoAplicadoAMora = Math.Min(descuentoTotal, moraTotal);
                    moraTotal -= descuentoAplicadoAMora;
                    descuentoTotal -= descuentoAplicadoAMora;
                    _logger.LogInformation("[{TraceId}] Descuento aplicado a mora: {Monto}", traceId, descuentoAplicadoAMora);
                }

                descuentoAplicadoACapital = descuentoTotal;
                _logger.LogInformation("[{TraceId}] Descuento restante aplicado a capital: {Monto}", traceId, descuentoAplicadoACapital);

                // 10. Determinar origen del cobro
                var fuente = Request.Headers["X-Source"].FirstOrDefault()?.ToLower();
                var origen = fuente switch
                {
                    "android" => OrigenCobro.Android,
                    _ => OrigenCobro.Web
                };
                _logger.LogInformation("[{TraceId}] Origen del cobro detectado: {Origen}", traceId, origen);

                // 11. Actualización de comprobante fiscal
                _logger.LogInformation("[{TraceId}] Procesando comprobante fiscal...", traceId);
                if (comprobanteFiscal.FidComprobante == 0)
                {
                    comprobanteFiscal.Fcomprobante = "00000000000";
                    _logger.LogInformation("[{TraceId}] Comprobante fiscal con ID 0, asignado valor por defecto", traceId);
                }
                else if (comprobanteFiscal.Fcontador != null)
                {
                    comprobanteFiscal.Fcontador += 1;
                    _context.Update(comprobanteFiscal);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("[{TraceId}] Contador de comprobante incrementado a: {Contador}", traceId, comprobanteFiscal.Fcontador);
                }

                // 12. Creación del cobro principal
                _logger.LogInformation("[{TraceId}] Creando registro de cobro principal...", traceId);
                var cobro = new TbCobro
                {
                    FkidCxc = request.FkidCxc,
                    Fmonto = montoIngresado,
                    Fdescuento = request.Fdescuento,
                    Fcargos = request.Fcargos,
                    Ffecha = DateOnly.FromDateTime(DateTime.Now),
                    Fhora = TimeOnly.FromDateTime(DateTime.Now),
                    FkidUsuario = usuario.FidUsuario,
                    FkidOrigen = (int)origen,
                    Fncf = comprobanteFiscal.Fcomprobante,
                    FncfVence = DateOnly.FromDateTime(comprobanteFiscal.Fvence ?? DateTime.Now),
                    Factivo = true,
                };

                // 13. Generación de concepto
                _logger.LogInformation("[{TraceId}] Generando concepto del cobro...", traceId);
                var conceptos = new List<string>();
                var culture = new CultureInfo("es-ES");
                decimal montoRestanteConcepto = montoIngresado;

                foreach (var cuota in cuotasSeleccionadas)
                {
                    if (montoRestanteConcepto <= 0) break;

                    string accion = montoRestanteConcepto >= cuota.Fsaldo ? "Saldó" : "Abono a";
                    montoRestanteConcepto = montoRestanteConcepto >= cuota.Fsaldo ?
                        montoRestanteConcepto - cuota.Fsaldo : 0;

                    conceptos.Add($"{accion} cuota {cuota.FNumeroCuota} con vencimiento {cuota.Fvence:dd/MM/yyyy}");
                }

                if (montoRestanteConcepto > 0)
                {
                    var cuotasSiguientes = todasCuotasPendientes
                        .Where(c => !request.CuotasSeleccionadas.Contains(c.FNumeroCuota))
                        .OrderBy(c => c.Fvence)
                        .ToList();

                    foreach (var cuota in cuotasSiguientes)
                    {
                        if (montoRestanteConcepto <= 0) break;

                        string accion = montoRestanteConcepto >= cuota.Fsaldo ? "Saldó" : "Abono a";
                        montoRestanteConcepto = montoRestanteConcepto >= cuota.Fsaldo ?
                            montoRestanteConcepto - cuota.Fsaldo : 0;

                        conceptos.Add($"{accion} cuota {cuota.FNumeroCuota} con vencimiento {cuota.Fvence:dd/MM/yyyy}");
                    }
                }

                cobro.Fconcepto = conceptos.Any() ? string.Join(", ", conceptos) : "Pago de cuotas";
                _logger.LogInformation("[{TraceId}] Concepto generado: {Concepto}", traceId, cobro.Fconcepto);

                _context.TbCobros.Add(cobro);
                await _context.SaveChangesAsync();
                _logger.LogInformation("[{TraceId}] Cobro principal creado con ID: {CobroId}", traceId, cobro.FidCobro);

                // 14. Procesamiento de cuotas
                _logger.LogInformation("[{TraceId}] Procesando cuotas...", traceId);
                int nuevoIdCobro = cobro.FidCobro;
                decimal montoRestante = montoIngresado;

                foreach (var cuota in cuotasSeleccionadas)
                {
                    if (montoRestante <= 0) break;

                    decimal capitalCuota = cuota.Fsaldo - cuota.Fmora;
                    decimal descuentoEnEstaCuota = Math.Min(descuentoAplicadoACapital, capitalCuota);
                    capitalCuota -= descuentoEnEstaCuota;
                    descuentoAplicadoACapital -= descuentoEnEstaCuota;

                    decimal montoAplicado = 0;
                    decimal moraAplicada = 0;

                    if (montoRestante >= (capitalCuota + moraTotal))
                    {
                        montoAplicado = capitalCuota + moraTotal;
                        moraAplicada = moraTotal;
                        montoRestante -= montoAplicado;
                        cuota.Fsaldo = 0;
                        cuota.Fstatus = 'S';
                    }
                    else
                    {
                        if (montoRestante >= capitalCuota)
                        {
                            montoAplicado = montoRestante;
                            decimal capitalPagado = capitalCuota;
                            moraAplicada = montoRestante - capitalPagado;
                            montoRestante = 0;
                            cuota.Fsaldo = 0;
                        }
                        else
                        {
                            montoAplicado = montoRestante;
                            moraAplicada = 0;
                            montoRestante = 0;
                            cuota.Fsaldo -= montoAplicado;
                        }
                    }

                    _context.TbCxcCuota.Update(cuota);

                    var detalle = new TbCobrosDetalle
                    {
                        FkidCobro = nuevoIdCobro,
                        FnumeroCuota = cuota.FNumeroCuota,
                        Fmonto = montoAplicado,
                        Fmora = moraAplicada
                    };
                    _context.TbCobrosDetalles.Add(detalle);

                    _logger.LogInformation("[{TraceId}] Cuota {NumeroCuota} procesada - Monto: {Monto}, Mora: {Mora}, Saldo restante: {Saldo}",
                        traceId, cuota.FNumeroCuota, montoAplicado, moraAplicada, montoRestante);
                }

                // 15. Procesamiento de cuotas siguientes si queda monto
                if (montoRestante > 0)
                {
                    _logger.LogInformation("[{TraceId}] Procesando cuotas siguientes con monto restante: {MontoRestante}", traceId, montoRestante);

                    var cuotasSiguientes = todasCuotasPendientes
                        .Where(c => !request.CuotasSeleccionadas.Contains(c.FNumeroCuota))
                        .OrderBy(c => c.Fvence)
                        .ToList();

                    foreach (var cuota in cuotasSiguientes)
                    {
                        if (montoRestante <= 0) break;

                        decimal montoAplicado = 0;
                        decimal moraAplicada = 0;

                        if (montoRestante >= cuota.Fsaldo)
                        {
                            montoAplicado = cuota.Fsaldo;
                            montoRestante -= cuota.Fsaldo;

                            if (cuota.Fmora > 0)
                            {
                                decimal proporcionMora = cuota.Fmora / (cuota.Fsaldo + cuota.Fmora);
                                moraAplicada = cuota.Fmora * proporcionMora;
                            }

                            cuota.Fsaldo = 0;
                            cuota.Fstatus = 'S';
                        }
                        else
                        {
                            montoAplicado = montoRestante;

                            if (cuota.Fmora > 0)
                            {
                                decimal proporcionPago = montoRestante / (cuota.Fsaldo + cuota.Fmora);
                                moraAplicada = cuota.Fmora * proporcionPago;
                            }

                            cuota.Fsaldo -= montoRestante;
                            montoRestante = 0;
                        }

                        _context.TbCxcCuota.Update(cuota);

                        var detalle = new TbCobrosDetalle
                        {
                            FkidCobro = nuevoIdCobro,
                            FnumeroCuota = cuota.FNumeroCuota,
                            Fmonto = montoAplicado,
                            Fmora = moraAplicada
                        };
                        _context.TbCobrosDetalles.Add(detalle);

                        _logger.LogInformation("[{TraceId}] Cuota siguiente {NumeroCuota} procesada - Monto: {Monto}, Mora: {Mora}",
                            traceId, cuota.FNumeroCuota, montoAplicado, moraAplicada);
                    }
                }

                // 16. Creación del desglose de pago
                _logger.LogInformation("[{TraceId}] Creando desglose de pago...", traceId);
                var desglose = new TbCobrosDesglose
                {
                    FkidCobro = nuevoIdCobro,
                    Fefectivo = request.Fefectivo,
                    FmontoRecibido = request.FmontoRecibido,
                    Ftransferencia = request.Ftransferencia,
                    Ftarjeta = request.Ftarjeta,
                    FnotaCredito = request.FnotaCredito,
                    Fcheque = request.Fcheque,
                    Fdeposito = request.Fdeposito,
                    FdebitoAutomatico = request.FdebitoAutomatico,
                    FnoNotaCredito = request.FnoNotaCredito,
                    FkidUsuario = usuario.FidUsuario,
                    Factivo = true
                };
                _context.TbCobrosDesgloses.Add(desglose);

                // 17. Actualización estado de la cuenta
                _logger.LogInformation("[{TraceId}] Actualizando estado de la cuenta...", traceId);
                var todasLasCuotas = await _context.TbCxcCuota
                    .Where(c => c.FkidCxc == request.FkidCxc)
                    .ToListAsync();

                bool todasSaldadas = todasLasCuotas.All(c => c.Fstatus == 'S');
                bool tieneCuotasVencidas = todasLasCuotas.Any(c => c.Fstatus == 'V');

                if (todasSaldadas)
                {
                    cuenta.Fstatus = 'S';
                    _context.TbCxcs.Update(cuenta);
                    _logger.LogInformation("[{TraceId}] Todas las cuotas saldadas, marcando cuenta como saldada", traceId);
                }
                else if (!tieneCuotasVencidas)
                {
                    cuenta.Fstatus = 'N';
                    _context.TbCxcs.Update(cuenta);
                    _logger.LogInformation("[{TraceId}] Cuenta actualizada a estado normal (sin cuotas vencidas)", traceId);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("[{TraceId}] Todos los cambios guardados en base de datos", traceId);

                _logger.LogInformation("[{TraceId}] ====== PROCESAMIENTO EXITOSO EN {ElapsedMs}ms ======",
                    traceId, stopwatch.ElapsedMilliseconds);

                return Ok(new
                {
                    success = true,
                    message = "Cobro registrado exitosamente.",
                    cobroId = nuevoIdCobro,
                    montoAplicado = montoIngresado - montoRestante,
                    saldoPendiente = montoRestante > 0 ? montoRestante : 0,
                    ncf = cobro.Fncf,
                    traceId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{TraceId}] ====== ERROR EN CREATE (Elapsed: {ElapsedMs}ms) ======",
                    traceId, stopwatch.ElapsedMilliseconds);

                _logger.LogError("[{TraceId}] Request completo: {@Request}", traceId, request);
                _logger.LogError("[{TraceId}] Headers: {@Headers}", traceId, Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()));
                _logger.LogError("[{TraceId}] Usuario: {User}", traceId, User.Identity?.Name);
                _logger.LogError("[{TraceId}] Stack Trace: {StackTrace}", traceId, ex.StackTrace);

                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Error interno del servidor",
                    Detail = "Ocurrió un error al procesar la solicitud",
                    Status = StatusCodes.Status500InternalServerError,
                    Extensions = {
                ["traceId"] = traceId,
                ["errorDetails"] = ex.Message
            }
                });
            }
        }

        [IgnoreAntiforgeryToken]
        [HttpPost("anular/{id}")]
        [Authorize(Policy = "Permissions.Cobros.Anular")]
        public async Task<IActionResult> AnularCobro(int id, [FromBody] AnularCobroRequest request)
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
                if (string.IsNullOrWhiteSpace(request.MotivoAnulacion))
                    return BadRequest(new { success = false, message = "Debe especificar un motivo para la anulación." });

                // Obtener el cobro principal
                var cobro = await _context.TbCobros.FindAsync(id);

                if (cobro == null)
                    return NotFound(new { success = false, message = "Cobro no encontrado." });

                // Verificar si ya está anulado
                if (!cobro.Factivo)
                    return BadRequest(new { success = false, message = "El cobro ya está anulado." });

                // Obtener usuario actual
                var identityId = _userManager.GetUserId(User);
                var usuario = await _context.TbUsuarios.FirstOrDefaultAsync(u => u.IdentityId == identityId);

                // Crear registro en TbCobroNulo
                var cobroNulo = new TbCobroNulo
                {
                    FkidCobro = id, // ID del cobro original que se está anulando
                    FkidUsuario = usuario?.FidUsuario ?? cobro.FkidUsuario,
                    Fhora = TimeOnly.FromDateTime(DateTime.Now),
                    FmotivoAnulacion = request.MotivoAnulacion,
                    FfechaAnulacion = DateOnly.FromDateTime(DateTime.Now)
                };

                _context.TbCobrosNulos.Add(cobroNulo);

                // Actualizar solo el estado activo del cobro principal
                cobro.Factivo = false;

                // Obtener todos los detalles del cobro
                var detalles = await _context.TbCobrosDetalles
                    .Where(d => d.FkidCobro == id)
                    .ToListAsync();

                // Obtener todos los desgloses del cobro y marcarlos como inactivos
                var desgloses = await _context.TbCobrosDesgloses
                    .Where(d => d.FkidCobro == id)
                    .ToListAsync();

                // Marcar todos los desgloses como inactivos
                foreach (var desglose in desgloses)
                {
                    desglose.Factivo = false;
                    _context.TbCobrosDesgloses.Update(desglose);
                }

                // Revertir cada cuota afectada
                foreach (var detalle in detalles)
                {
                    var cuota = await _context.TbCxcCuota
                        .FirstOrDefaultAsync(c => c.FkidCxc == cobro.FkidCxc && c.FNumeroCuota == detalle.FnumeroCuota);

                    if (cuota != null)
                    {
                        // Revertir el saldo
                        cuota.Fsaldo += detalle.Fmonto;

                        // Revertir la mora si fue aplicada
                        if (detalle.Fmora > 0)
                        {
                            cuota.Fmora -= detalle.Fmora;
                        }

                        // Actualizar el estado según la fecha de vencimiento
                        if (cuota.Fvence.Date < DateTime.Today)
                        {
                            cuota.Fstatus = 'V'; // Vencida
                        }
                        else
                        {
                            cuota.Fstatus = 'N'; // Pendiente
                        }

                        _context.TbCxcCuota.Update(cuota);
                    }

                    // Marcar el detalle como inactivo
                    detalle.Factivo = false;
                    _context.TbCobrosDetalles.Update(detalle);
                }

                // Guardar todos los cambios en una sola transacción
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Cobro anulado exitosamente. Todas las entidades relacionadas han sido actualizadas."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al anular el cobro.");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno al anular el cobro.",
                    error = ex.Message
                });
            }
        }

        [IgnoreAntiforgeryToken]
        [HttpGet("usuario-actual")]
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

        [IgnoreAntiforgeryToken]
        [HttpPost("validar-contrasena")]
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
    }
}