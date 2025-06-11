using System.Text.Json;
using Alquileres.Context;
using Alquileres.DTO;
using Alquileres.Enums;
using Alquileres.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Alquileres.Controllers
{
    [Authorize] // Esto aplicará a todas las acciones del controlador, requiriendo que el usuario esté autenticado.
    public class TbCobrosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TbCobrosController> _logger;

        public TbCobrosController(ApplicationDbContext context, ILogger<TbCobrosController> logger)
        {
            _context = context;
            _logger = logger;
        }
        public IActionResult Index()
        {
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
                // Ordenar los cobros por FidCobro
                var cobros = await _context.TbCobros
                    .OrderBy(c => c.FidCobro)
                    .ToListAsync();

                var cobroViewModels = cobros.Select(c => new CobroViewModel
                {
                    FidCobro = c.FidCobro,
                    FkidCxc = c.FkidCxc,
                    Ffecha = c.Ffecha,
                    Fhora = c.Fhora,
                    Fmonto = c.Fmonto,
                    Fdescuento = c.Fdescuento,
                    Fcargos = c.Fcargos,
                    Fconcepto = c.Fconcepto,
                    NombreOrigen = ((OrigenCobro)c.FkidOrigen).ToString(),
                    Factivo = c.Factivo
                })
                .OrderBy(c => c.FidCobro)
                .ToList();

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
                    .FirstOrDefaultAsync(i => i.FidInquilino == cuentaPorCobrar.FidInquilino);

                // Obtener el inmueble
                var inmueble = await _context.TbInmuebles
                    .FirstOrDefaultAsync(i => i.FidInmueble == cuentaPorCobrar.FkidInmueble);

                var cuotasIds = detallesCobro.Select(d => d.FnumeroCuota).ToList();

                var cuotasQuery = _context.TbCxcCuota.AsQueryable();
                foreach (var cuotaId in cuotasIds)
                {
                    cuotasQuery = cuotasQuery.Where(c => c.FNumeroCuota == cuotaId && c.FidCxc == cobro.FkidCxc);
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


        // GET: TbCobros/BuscarCxc
        [HttpGet]
        public async Task<IActionResult> BuscarCxc(string? searchTerm = null)
        {
            var query = from cxc in _context.TbCxcs
                        join inquilino in _context.TbInquilinos on cxc.FidInquilino equals inquilino.FidInquilino
                        select new { cxc, inquilino };

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(x => x.cxc.FidCuenta.ToString().Contains(searchTerm) ||
                                        x.cxc.Fmonto.ToString().Contains(searchTerm) ||
                                        x.cxc.FdiasGracia.ToString().Contains(searchTerm) ||
                                        x.cxc.FtasaMora.ToString().Contains(searchTerm));
            }

            var resultados = await query
                .Select(x => new ResultadoBusquedaCxCViewModel
                {
                    Id = x.cxc.FidCuenta,
                    Text = $"Cuenta #{x.cxc.FidCuenta} - {x.inquilino.Fnombre} {x.inquilino.Fapellidos} - " +
                           $"Monto: {x.cxc.Fmonto:C} - Día de Gracia: {x.cxc.FdiasGracia} días - Mora: {x.cxc.FtasaMora}%",
                    Tipo = "cuenta por cobrar"
                })
                .ToListAsync();

            return Json(new { results = resultados });
        }

        // GET: TbCobros/Create
        [HttpGet]
        [Authorize(Policy = "Permissions.Cobros.Crear")]
        public IActionResult Create()
        {

            return PartialView("_CreateCobroPartial");
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
                    .Where(c => c.FidCxc == cuenta.FidCuenta && (c.Fstatus == 'N' || c.Fstatus == 'V')) // Filtrar por estado
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

        [HttpGet]
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
                    .FirstOrDefaultAsync(i => i.FidInquilino == cuenta.FidInquilino);

                if (inquilino == null)
                {
                    return Json(new { success = false, message = "Inquilino no encontrado" });
                }

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

                // Obtener el desglose de cobros para el cobro actual
                var desgloseCobros = await _context.TbCobrosDesgloses
                    .Where(d => d.FkidCobro == cobroActual.FidCobro)
                    .ToListAsync();

                // Calcular los totales - IMPORTANTE: El subtotal es el monto original de la cuota
                var subtotal = cuenta.Fmonto; // Usamos el monto de la cuenta, no del cobro
                var cargos = cobroActual.Fcargos;
                var descuento = cobroActual.Fdescuento;
                var total = subtotal + cargos - descuento;
                var mora = 0; // Asumiendo que no hay mora en este caso

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
                    direccion = inmueble.Fdireccion,
                    ubicacion = inmueble.Fubicacion,
                    telefono = inquilino.Ftelefono ?? "No disponible",
                    cliente = $"{inquilino.Fnombre} {inquilino.Fapellidos}",
                    noCobro = cobroActual.FidCobro,
                    concepto = cobroActual.Fconcepto,
                    subtotal = subtotal,
                    cargos = cargos,
                    mora = mora,
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
                };

                return Json(new { success = true, datos = datosTicket });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
