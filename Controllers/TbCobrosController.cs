using System.Security.Claims;
using System.Text.Json;
using Alquileres.Context;
using Alquileres.DTO;
using Alquileres.Enums;
using Alquileres.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Alquileres.Controllers
{
    [Authorize] // Esto aplicará a todas las acciones del controlador, requiriendo que el usuario esté autenticado.
    public class TbCobrosController : BaseController
    {
        private readonly ILogger<TbCobrosController> _logger;

        public TbCobrosController(ApplicationDbContext context,
        ILogger<TbCobrosController> logger) : base(context)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index(string vista = "crear")
        {
            var empresa = await _context.Empresas.FirstOrDefaultAsync();
            var tipoCobro = empresa?.ActivarCobroRapido == true ? "rapido" : "detallado";
            ViewData["TipoCobro"] = tipoCobro;
            ViewData["Vista"] = vista;
            return View();
        }

        [HttpPost]
        [Authorize(Policy = "Permissions.Cobros.Anular")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var cobro = await _context.TbCobros.FindAsync(id);
            if (cobro == null)
            {
                return NotFound();
            }

            cobro.Factivo = !cobro.Factivo;
            _context.Update(cobro);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, nuevoEstado = cobro.Factivo });
        }


        [HttpGet]
        [Authorize(Policy = "Permissions.Cobros.Ver")]
        public async Task<IActionResult> CargarCobro()
        {
            try
            {
                var cobros = await _context.TbCobros
                    .OrderBy(c => c.FidCobro)
                    .ToListAsync();

                var cobroViewModels = new List<CobroViewModel>();

                foreach (var cobro in cobros)
                {
                    // Consulta directa para cada relación (sin Contains)
                    var cxc = await _context.TbCxcs.FirstOrDefaultAsync(c => c.FidCuenta == cobro.FkidCxc);
                    var inquilino = cxc != null ? await _context.TbInquilinos.FirstOrDefaultAsync(i => i.FidInquilino == cxc.FkidInquilino) : null;
                    var inmueble = cxc != null ? await _context.TbInmuebles.FirstOrDefaultAsync(i => i.FidInmueble == cxc.FkidInmueble) : null;

                    cobroViewModels.Add(new CobroViewModel
                    {
                        FidCobro = cobro.FidCobro,
                        FkidCxc = cobro.FkidCxc,
                        Ffecha = cobro.Ffecha,
                        Fhora = cobro.Fhora,
                        Fmonto = cobro.Fmonto,
                        Fdescuento = cobro.Fdescuento,
                        Fcargos = cobro.Fcargos,
                        Fconcepto = cobro.Fconcepto,
                        NombreOrigen = ((OrigenCobro)cobro.FkidOrigen).ToString(),
                        Factivo = cobro.Factivo,
                        NombreInquilino = inquilino != null
                            ? $"{inquilino.Fnombre?.Trim()} {inquilino.Fapellidos?.Trim()}".Trim()
                            : "N/A",
                        DescripcionInmueble = inmueble?.Fdescripcion ?? "N/A"
                    });
                }

                return PartialView("_CobroPartial", cobroViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar cobros");
                return PartialView("_CobroPartial", new List<CobroViewModel>());
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Cobros.VerDetalles")]
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Obtener el cobro basado en el ID
                var cobro = await _context.TbCobros
                    .FirstOrDefaultAsync(m => m.FidCobro == id);

                if (cobro == null)
                {
                    return NotFound();
                }

                // Obtener los detalles del cobro
                var detallesCobro = await _context.TbCobrosDetalles
                    .Where(d => d.FkidCobro == cobro.FidCobro)
                    .ToListAsync();

                // Obtener el desglose del cobro
                var desgloseCobro = await _context.TbCobrosDesgloses
                    .FirstOrDefaultAsync(d => d.FkidCobro == cobro.FidCobro);

                // Obtener la cuenta por cobrar
                var cuentaPorCobrar = await _context.TbCxcs
                    .FirstOrDefaultAsync(c => c.FidCuenta == cobro.FkidCxc);

                if (cuentaPorCobrar == null)
                {
                    return NotFound();
                }

                // Obtener el inquilino
                var inquilino = await _context.TbInquilinos
                    .FirstOrDefaultAsync(i => i.FidInquilino == cuentaPorCobrar.FkidInquilino);

                // Obtener el inmueble
                var inmueble = await _context.TbInmuebles
                    .FirstOrDefaultAsync(i => i.FidInmueble == cuentaPorCobrar.FkidInmueble);

                var cuotasIds = detallesCobro.Select(d => d.FnumeroCuota).ToList();

                var cuotasQuery = _context.TbCxcCuota.AsQueryable();
                foreach (var cuotaId in cuotasIds)
                {
                    cuotasQuery = cuotasQuery.Where(c => c.FNumeroCuota == cuotaId && c.FkidCxc == cobro.FkidCxc);
                }

                var cuotas = await cuotasQuery.ToListAsync();

                // Crear el ViewModel
                var viewModel = new DetallesCobroViewModel
                {
                    Cobro = cobro,
                    Detalles = detallesCobro,
                    Desglose = desgloseCobro,
                    Cxc = cuentaPorCobrar,
                    Inquilino = inquilino,
                    Inmueble = inmueble,
                    Cuotas = cuotas
                };

                // Retornar la vista parcial con el ViewModel
                return PartialView("_DetallesCobroPartial", viewModel);
            }
            catch (Exception ex)
            {
                // Loguear el error en el sistema de logs
                _logger.LogError(ex, "Error al obtener los detalles del cobro con ID {Id}", id);

                // Retornar un error genérico
                return StatusCode(500, "Ocurrió un error al procesar la solicitud.");
            }
        }


        [HttpGet]
        public async Task<IActionResult> BuscarCxc(string? searchTerm = null)
        {
            var query = from cxc in _context.TbCxcs
                        join inmueble in _context.TbInmuebles on cxc.FkidInmueble equals inmueble.FidInmueble into inmuebleJoin
                        from inm in inmuebleJoin.DefaultIfEmpty()
                        join inquilino in _context.TbInquilinos on cxc.FkidInquilino equals inquilino.FidInquilino
                        select new { cxc, inquilino, inm };

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(x => x.cxc.FidCuenta.ToString().Contains(searchTerm) ||
                                        x.cxc.Fmonto.ToString().Contains(searchTerm) ||
                                        x.inquilino.Fnombre.Contains(searchTerm) ||
                                        x.inquilino.Fapellidos.Contains(searchTerm) ||
                                        (x.inm != null && x.inm.Fdescripcion.Contains(searchTerm)) ||
                                        x.cxc.FfechaInicio.ToString("dd/MM/yyyy").Contains(searchTerm));
            }

            var resultados = await query
            .Select(x => new ResultadoBusquedaCxCViewModel
            {
                Id = x.cxc.FidCuenta,
                Text = $"Cuenta #{x.cxc.FidCuenta} - {x.inquilino.Fnombre} {x.inquilino.Fapellidos} - " +
                    $"Inmueble: {(x.inm != null ? x.inm.Fdescripcion : "Sin inmueble")} - " +
                    $"Monto: {x.cxc.Fmonto:C} - " +
                    $"Fecha Inicio: {x.cxc.FfechaInicio.ToString("dd/MM/yyyy")}",
                Tipo = "cuenta por cobrar",
                FfechaInicio = DateOnly.FromDateTime(x.cxc.FfechaInicio) // Conversión explícita
            })
            .ToListAsync();

            return Json(new { results = resultados });
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Cobros.Crear")]
        public IActionResult Create()
        {
            try
            {
                // 1. Obtener el usuario actual
                var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var usuario = _context.TbUsuarios.FirstOrDefault(u => u.IdentityId == identityUserId);

                if (usuario == null)
                {
                    return BadRequest("Usuario no encontrado");
                }

                // 2. Verificar si hay comprobantes fiscales
                var tieneComprobantes = _context.TbComprobantesFiscales.Any();
                if (!tieneComprobantes)
                {
                    return BadRequest(new
                    {
                        success = false,
                        errorType = "no_fiscal_documents",
                        message = "No hay comprobantes fiscales configurados en el sistema",
                        requiresAction = true
                    });
                }

                // 2.1 Obtener la empresa del usuario (manteniendo tu lógica actual)
                var empresaId = _context.TbComprobantesFiscales
                                    .Select(c => c.FkidEmpresa)
                                    .FirstOrDefault();

                if (empresaId == null || empresaId == 0)
                {
                    return BadRequest("El usuario no tiene empresa asignada");
                }

                // 3. Obtener la empresa y su comprobante predeterminado
                var empresa = _context.Empresas.Find(empresaId);
                if (empresa == null)
                {
                    return BadRequest("Empresa no encontrada");
                }

                // 4. Obtener todos los comprobantes disponibles
                var comprobantes = _context.TbComprobantesFiscales
                                        .Where(c => c.FkidEmpresa == empresaId || c.FkidEmpresa == null)
                                        .ToList();

                // 5. Determinar el comprobante predeterminado
                string comprobanteSeleccionado = null;

                if (empresa.TipoComprobantePorDefecto > 0)
                {
                    // Buscar el comprobante que coincide con el ID del tipo predeterminado
                    var comprobantePredeterminado = comprobantes
                        .FirstOrDefault(c => c.FidTipoComprobante == empresa.TipoComprobantePorDefecto);

                    comprobanteSeleccionado = comprobantePredeterminado?.Fcomprobante;
                }

                // 6. Crear el modelo
                var model = new CobroViewModel
                {
                    Comprobantes = comprobantes.Select(c => new SelectListItem
                    {
                        Value = c.Fcomprobante,
                        Text = c.FtipoComprobante,
                        Selected = c.Fcomprobante == comprobanteSeleccionado
                    }).ToList(),
                    ComprobanteSeleccionado = comprobanteSeleccionado
                };

                return PartialView("_CreateCobroPartial", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el formulario de cobros");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        public IActionResult CreateCobroRapido()
        {
            try
            {
                var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var usuario = _context.TbUsuarios.FirstOrDefault(u => u.IdentityId == identityUserId);

                if (usuario == null)
                {
                    return BadRequest("Usuario no encontrado");
                }

                // Lista de métodos de pago
                var metodosPago = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Efectivo", Text = "Efectivo" },
                    new SelectListItem { Value = "Transferencia", Text = "Transferencia" },
                    new SelectListItem { Value = "Tarjeta", Text = "Tarjeta" },
                    new SelectListItem { Value = "Cheque", Text = "Cheque" },
                    new SelectListItem { Value = "Depósito", Text = "Depósito" },
                    new SelectListItem { Value = "NotaCredito", Text = "Nota de Crédito" },
                    new SelectListItem { Value = "DébitoAutomático", Text = "Débito Automático" }
                };

                var model = new CobroViewModel
                {
                    MetodosPago = metodosPago,
                    MetodoPagoSeleccionado = "Efectivo" // puedes dejarlo nulo si no deseas default
                };

                return PartialView("_CobroRapidoPartial", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el formulario de cobros");
                return StatusCode(500, "Error interno del servidor");
            }
        }


        ////////////////////// Get Monto By Cuenta ///////////////////////////////////////

        public async Task<IActionResult> GetMontoByCuenta(int cuentaId)
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
                    .Where(c => c.FkidCxc == cuenta.FidCuenta && (c.Fstatus == 'N' || c.Fstatus == 'V')) // Filtrar por estado
                    .Select(c => new
                    {
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

        [HttpGet("Cobros/obtener-datos-ticket")]
        public async Task<IActionResult> GetDatosTicket(int cuentaId, int idCobro)
        {
            try
            {
                // Obtener la cuenta por cobrar
                var cuenta = await _context.TbCxcs
                    .FirstOrDefaultAsync(c => c.FidCuenta == cuentaId);

                if (cuenta == null)
                {
                    return Json(new { success = false, message = "Cuenta no encontrada" });
                }

                // Obtener el inquilino
                var inquilino = await _context.TbInquilinos
                    .FirstOrDefaultAsync(i => i.FidInquilino == cuenta.FkidInquilino);

                if (inquilino == null)
                {
                    return Json(new { success = false, message = "Inquilino no encontrado" });
                }

                // Obtener el inquilino
                var inmueble = await _context.TbInmuebles
                    .FirstOrDefaultAsync(i => i.FidInmueble == cuenta.FkidInmueble);

                if (inmueble == null)
                {
                    return Json(new { success = false, message = "Inmueble no encontrado" });
                }

                // Obtener el cobro específico
                var cobroActual = await _context.TbCobros
                    .FirstOrDefaultAsync(c => c.FidCobro == idCobro);

                if (cobroActual == null)
                {
                    return Json(new { success = false, message = "Cobro no encontrado" });
                }

                var comprobante = cobroActual.Fncf;
                var tipoComprobante = _context.TbComprobantesFiscales
                    .Where(c => c.Fcomprobante == comprobante)
                    .Select(c => c.FtipoComprobante)
                    .FirstOrDefault();

                // Obtener el usuario que realizó el cobro
                var usuario = await _context.TbUsuarios
                    .FirstOrDefaultAsync(u => u.FidUsuario == cobroActual.FkidUsuario);

                // Obtener el desglose de cobros para el cobro actual
                var desgloseCobros = await _context.TbCobrosDesgloses
                    .Where(d => d.FkidCobro == cobroActual.FidCobro)
                    .ToListAsync();

                // Obtener los detalles del cobro para calcular la mora cobrada
                var detallesCobro = await _context.TbCobrosDetalles
                    .Where(d => d.FkidCobro == cobroActual.FidCobro)
                    .ToListAsync();

                // Calcular la mora cobrada sumando los valores de Fmora en los detalles
                var moraCobrada = detallesCobro.Sum(d => d.Fmora);

                var subtotalCobros = cobroActual.Fmonto; // Aquí tomamos el Fmonto de TbCobros como subtotal
                var cargos = cobroActual.Fcargos;
                var descuento = cobroActual.Fdescuento;

                // Ajustar el subtotal (monto base sin cargos/mora/descuentos)
                var subtotal = subtotalCobros - cargos - moraCobrada + descuento;

                // El total debe incluir la mora cobrada
                var total = subtotalCobros;

                // Obtener los datos de desglose de cobros
                var efectivo = desgloseCobros.Sum(d => d.Fefectivo);
                var transferencia = desgloseCobros.Sum(d => d.Ftransferencia);
                var tarjeta = desgloseCobros.Sum(d => d.Ftarjeta);
                var cheque = desgloseCobros.Sum(d => d.Fcheque);
                var deposito = desgloseCobros.Sum(d => d.Fdeposito);
                var montoNotaCredito = desgloseCobros.Sum(d => d.FnotaCredito);
                var noNotaCredito = desgloseCobros.Sum(d => d.FnoNotaCredito);
                var debitoAutomatico = desgloseCobros.Sum(d => d.FdebitoAutomatico);

                // Obtener el monto recibido del desglose de cobros
                var montoRecibido = desgloseCobros.Sum(d => d.FmontoRecibido);

                // Calcular el cambio correctamente
                var cambio = Math.Max(0, montoRecibido - efectivo);

                var datosTicket = new
                {
                    fncfVence = cobroActual.FncfVence?.ToString("dd/MM/yyyy"),
                    cliente = $"{inquilino.Fnombre} {inquilino.Fapellidos}",
                    inmueble = $"{inmueble.Fdescripcion}",
                    noCobro = cobroActual.FidCobro,
                    concepto = cobroActual.Fconcepto,
                    subtotal = subtotal,
                    cargos = cargos,
                    mora = moraCobrada,
                    descuento = descuento,
                    total = total,
                    efectivo = efectivo,
                    efectivoRecibido = montoRecibido,
                    cambio = cambio,
                    transferencia = transferencia,
                    tarjeta = tarjeta,
                    cheque = cheque,
                    deposito = deposito,
                    montoNotaCredito = montoNotaCredito,
                    noNotaCredito = noNotaCredito,
                    debitoAutomatico = debitoAutomatico,
                    usuario = usuario != null ? $"{usuario.Fnombre}" : "Usuario no encontrado",
                    usuarioId = usuario?.FidUsuario,
                    comprobante = comprobante,
                    tipoComprobante = tipoComprobante
                };

                return Json(new { success = true, datos = datosTicket });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Cobros.VerEstadoCobro")]
        public async Task<IActionResult> GetResumenCobros()
        {
            try
            {
                var hoyDateOnly = DateOnly.FromDateTime(DateTime.Today);
                var hoyDateTime = DateTime.Today;
                var inicioMes = new DateOnly(hoyDateOnly.Year, hoyDateOnly.Month, 1);
                var finMes = inicioMes.AddMonths(1).AddDays(-1);

                // 1. Cobros de hoy
                var cobrosHoy = await _context.TbCobros
                    .Where(c => c.Ffecha == hoyDateOnly && c.Factivo)
                    .SumAsync(c => c.Fmonto);

                // 2. Cobros del mes
                var cobrosMes = await _context.TbCobros
                    .Where(c => c.Ffecha >= inicioMes && c.Ffecha <= finMes && c.Factivo)
                    .SumAsync(c => c.Fmonto);

                // 3. Monto pendiente (suma de saldos de cuotas no pagadas)
                var pendientes = await _context.TbCxcCuota
                    .Where(c => (c.Fstatus == 'N' || c.Fstatus == 'V') && c.Factivo)
                    .SumAsync(c => c.Fsaldo);

                // 4. Monto vencido (cuotas con estado 'V'=Vencido)
                var vencidas = await _context.TbCxcCuota
                    .Where(c => c.Fstatus == 'V' && c.Factivo && DateOnly.FromDateTime(c.Fvence) < hoyDateOnly)
                    .SumAsync(c => c.Fsaldo + c.Fmora);

                return Json(new
                {
                    success = true,
                    hoy = cobrosHoy,
                    mes = cobrosMes,
                    pendientes = pendientes,
                    vencidas = vencidas
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener resumen de cobros");
                return Json(new
                {
                    success = false,
                    message = "Error al cargar los datos",
                    error = ex.Message
                });
            }
        }
    }
}
