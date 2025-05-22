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

        [HttpPost("create")]
        [Authorize(Policy = "Permissions.Cobros.Crear")]
        public async Task<IActionResult> Create([FromBody] CobroRequest request)
        {
            try
            {
                _logger.LogInformation("Solicitud recibida: {@Request}", request);

                // Validaciones iniciales (usuario, modelo, cuotas seleccionadas)
                var identityId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(identityId))
                    return BadRequest(new { success = false, message = "El usuario no está autenticado." });

                var usuario = await _context.TbUsuarios.FirstOrDefaultAsync(u => u.IdentityId == identityId);
                if (usuario == null)
                    return BadRequest(new { success = false, message = $"No se encontró un usuario con el IdentityId: {identityId}" });

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Modelo inválido: {@Errors}", ModelState.Values.SelectMany(v => v.Errors));
                    return BadRequest(new
                    {
                        success = false,
                        message = "Datos inválidos.",
                        errors = ModelState.Values.SelectMany(v => v.Errors)
                    });
                }

                if (request.CuotasSeleccionadas == null || request.CuotasSeleccionadas.Count == 0)
                    return BadRequest(new { success = false, message = "Seleccione al menos una cuota." });

                var cuenta = await _context.TbCxcs.FirstOrDefaultAsync(c => c.FidCuenta == request.FkidCxc);
                if (cuenta == null)
                    return NotFound(new { success = false, message = "Cuenta no válida." });

                // Calcular monto ingresado
                decimal montoIngresado = request.Fefectivo + request.FmontoRecibido + request.Ftransferencia +
                                           request.Ftarjeta + request.FnotaCredito + request.Fcheque +
                                           request.Fdeposito + request.FdebitoAutomatico;

                if (montoIngresado <= 0)
                    return BadRequest(new { success = false, message = "Debe ingresar al menos un monto en el desglose de pago." });

                var fuente = Request.Headers["X-Source"].FirstOrDefault()?.ToLower();
                var origen = fuente switch
                {
                    "android" => OrigenCobro.Android,
                    _ => OrigenCobro.Web
                };

                // Obtener TODAS las cuotas pendientes ordenadas por fecha de vencimiento
                var todasCuotasPendientes = await _context.TbCxcCuota
                    .Where(c => c.FidCxc == request.FkidCxc && c.Fstatus != 'S')
                    .OrderBy(c => c.Fvence)
                    .ToListAsync();

                if (todasCuotasPendientes.Count == 0)
                    return BadRequest(new { success = false, message = "No existen cuotas pendientes para esta cuenta." });

                // Procesar primero las cuotas seleccionadas (en orden de vencimiento)
                var cuotasSeleccionadas = todasCuotasPendientes
                    .Where(c => request.CuotasSeleccionadas.Contains(c.FNumeroCuota))
                    .OrderBy(c => c.Fvence)
                    .ToList();

                // Crear registro del cobro
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
                    Factivo = true,
                };

                // Generar concepto basado en todas las cuotas seleccionadas
                if (cuotasSeleccionadas.Any())
                {
                    var culture = new CultureInfo("es-ES");
                    var conceptos = new List<string>();

                    foreach (var cuota in cuotasSeleccionadas)
                    {
                        var mes = culture.DateTimeFormat.GetMonthName(cuota.Fvence.Month);
                        var concepto = $"Pago de la cuota {cuota.FNumeroCuota} correspondiente al mes de {mes} con vencimiento el {cuota.Fvence.ToString("d", culture)}";
                        conceptos.Add(concepto);
                    }

                    // Unir todos los conceptos en una sola cadena
                    cobro.Fconcepto = string.Join(", ", conceptos);
                }
                else
                {
                    cobro.Fconcepto = "Pago de cuotas "; // Valor por defecto si no hay cuotas
                }

                _context.TbCobros.Add(cobro);
                await _context.SaveChangesAsync();


                int nuevoIdCobro = cobro.FidCobro;
                decimal montoRestante = montoIngresado;

                foreach (var cuota in cuotasSeleccionadas)
                {
                    if (montoRestante <= 0) break;

                    decimal montoAplicado = 0;

                    if (montoRestante >= cuota.Fsaldo)
                    {
                        montoAplicado = cuota.Fsaldo;
                        montoRestante -= cuota.Fsaldo;
                        cuota.Fsaldo = 0;
                        cuota.Fstatus = 'S';
                    }
                    else
                    {
                        montoAplicado = montoRestante;
                        cuota.Fsaldo -= montoRestante;
                        montoRestante = 0;
                    }
                    _context.TbCxcCuota.Update(cuota);

                    var detalle = new TbCobrosDetalle
                    {
                        FkidCobro = nuevoIdCobro,
                        FnumeroCuota = cuota.FNumeroCuota,
                        Fmonto = montoAplicado,
                        Fmora = cuota.Fmora
                    };
                    _context.TbCobrosDetalles.Add(detalle);
                }

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

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Cobro registrado exitosamente.",
                    cobroId = nuevoIdCobro,
                    montoAplicado = montoIngresado - montoRestante,
                    saldoPendiente = montoRestante > 0 ? montoRestante : 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el cobro.");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno al procesar el cobro.",
                    error = ex.Message
                });
            }
        }

        [HttpPost("anular/{id}")]
        [Authorize(Policy = "Permissions.Cobros.Anular")]
        [ValidateAntiForgeryToken]
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

                // Actualizar datos de anulación del cobro principal
                cobro.FmotivoAnulacion = request.MotivoAnulacion;
                cobro.FfechaAnulacion = DateOnly.FromDateTime(DateTime.Now);
                cobro.FkidUsuario = usuario?.FidUsuario ?? cobro.FkidUsuario;
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
                        .FirstOrDefaultAsync(c => c.FidCxc == cobro.FkidCxc && c.FNumeroCuota == detalle.FnumeroCuota);

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

        public class AnularCobroRequest
        {
            public string MotivoAnulacion { get; set; }
            public string Password { get; set; }
        }

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

        public class ValidarContrasenaRequest
        {
            public string Password { get; set; }
        }
    }
}