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
        private readonly ILogger<GeneradorDeCuotasService> _logger; // Agregar logger


        public TbCobrosController(ApplicationDbContext context, ILogger<GeneradorDeCuotasService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
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


        // GET: TbCobros/CargarCobro
        public async Task<IActionResult> CargarCobro()
        {
            var cobros = await _context.TbCobros.ToListAsync();

            // Crear una lista de modelos de vista
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
                NombreOrigen = ((OrigenCobro)c.FkidOrigen).ToString(), // Asignar el nombre del origen
                Factivo = c.Factivo
            }).ToList();

            return PartialView("_CobroPartial", cobroViewModels); // Devuelve la vista parcial
        }

        // GET: TbCobros/Detalles/5
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Obtener el cobro basado en el ID
            var cobro = await _context.TbCobros
                .FirstOrDefaultAsync(m => m.FkidCxc == id);

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

            // Obtener las cuotas pagadas
            var cuotasIds = detallesCobro.Select(d => d.FnumeroCuota).ToList();
            var cuotas = await _context.TbCxcCuota
                .Where(c => cuotasIds.Contains(c.FNumeroCuota) && c.FidCxc == cobro.FkidCxc)
                .ToListAsync();

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

        // GET: TbCobros/BuscarCxc
        [HttpGet]
        public async Task<IActionResult> BuscarCxc(string? searchTerm = null)
        {
            var cxCQuery = _context.TbCxcs.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                cxCQuery = cxCQuery.Where(i => i.FidCuenta.ToString().Contains(searchTerm) ||
                                                i.Fmonto.ToString().Contains(searchTerm) ||
                                                i.FdiasGracia.ToString().Contains(searchTerm) ||
                                                i.FtasaMora.ToString().Contains(searchTerm));
            }

            var cxCResultados = await cxCQuery
                .Select(i => new
                {
                    i.FidCuenta,
                    i.FidInquilino,
                    i.Fmonto,
                    i.FdiasGracia,
                    i.FtasaMora
                })
                .ToListAsync();

            var inquilinos = await _context.TbInquilinos
                .Where(p => cxCResultados.Select(m => m.FidInquilino).Contains(p.FidInquilino))
                .Select(p => new
                {
                    p.FidInquilino,
                    p.Fnombre,
                    p.Fapellidos
                })
                .ToListAsync();

            var resultados = cxCResultados.Select(m =>
            {
                var inquilino = inquilinos.FirstOrDefault(i => i.FidInquilino == m.FidInquilino);
                var nombreInquilino = inquilino != null ? $"{inquilino.Fnombre} {inquilino.Fapellidos}" : "Inquilino desconocido";

                return new ResultadoBusquedaCxCViewModel
                {
                    Id = m.FidCuenta,
                    Text = $"Cuenta #{m.FidCuenta} - {nombreInquilino} - Monto: {m.Fmonto:C} - Día de Gracia: {m.FdiasGracia} días - Mora: {m.FtasaMora}%",
                    Tipo = "cuenta por cobrar"
                };
            });


            var resultadosFinales = resultados.ToList();

            return Json(new { results = resultadosFinales });
        }

        // GET: TbCobros/Create
        [HttpGet]
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
        public async Task<IActionResult> GetDatosTicket(int cuentaId)
        {
            try
            {
                Console.WriteLine($"Buscando datos para cuentaId: {cuentaId}"); // Debug backend 1

                // Obtener la cuenta por cobrar
                var cuenta = await _context.TbCxcs
                    .FirstOrDefaultAsync(c => c.FidCuenta == cuentaId);

                if (cuenta == null)
                {
                    Console.WriteLine("Cuenta no encontrada"); // Debug backend 2
                    return Json(new { success = false, message = "Cuenta no encontrada" });
                }

                Console.WriteLine($"Cuenta encontrada. Inquilino ID: {cuenta.FidInquilino}"); // Debug backend 3

                // Obtener el inquilino
                var inquilino = await _context.TbInquilinos
                    .FirstOrDefaultAsync(i => i.FidInquilino == cuenta.FidInquilino);

                if (inquilino == null)
                {
                    Console.WriteLine("Inquilino no encontrado"); // Debug backend 4
                    return Json(new { success = false, message = "Inquilino no encontrado" });
                }

                Console.WriteLine($"Inquilino encontrado: {inquilino.Fnombre} {inquilino.Fapellidos}"); // Debug backend 5

                // Resto del código...
                var inmueble = await _context.TbInmuebles
                    .FirstOrDefaultAsync(i => i.FidInmueble == cuenta.FkidInmueble);

                if (inmueble == null)
                {
                    return Json(new { success = false, message = "Inmueble no encontrado" });
                }

                var cobro = await _context.TbCobros
                    .Where(c => c.FkidCxc == cuentaId)
                    .OrderByDescending(c => c.FidCobro)
                    .FirstOrDefaultAsync();

                var datosTicket = new
                {
                    direccion = inmueble.Fdireccion,
                    ubicacion = inmueble.Fubicacion,
                    telefono = inquilino.Ftelefono ?? "No disponible", // Manejar nulos
                    cliente = $"{inquilino.Fnombre} {inquilino.Fapellidos}",
                    noCobro = cobro?.FidCobro,
                    concepto = cobro?.Fconcepto,
                    subtotal = (cobro?.Fmonto) - (cobro?.Fcargos ?? 0) + (cobro?.Fdescuento ?? 0),
                    cargos = cobro?.Fcargos,
                    mora = 0,
                    descuento = cobro?.Fdescuento,
                    total = (cobro?.Fmonto),
                };

                Console.WriteLine("Datos del ticket preparados:"); // Debug backend 6
                Console.WriteLine(JsonSerializer.Serialize(datosTicket)); // Debug backend 7

                return Json(new { success = true, datos = datosTicket });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}"); // Debug backend 8
                _logger.LogError(ex, "Error al obtener datos para el ticket");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
