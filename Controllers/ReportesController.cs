using Alquileres.Models;
using Alquileres.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace Alquileres.Controllers
{
    public class ReportesController : BaseController
    {
        private readonly ILogger<ReportesController> _logger;

        public ReportesController(ApplicationDbContext context,
        ILogger<ReportesController> logger) : base(context)
        {
            _logger = logger;
        }

        public IActionResult Index(string tipoReporte = "CxC")
        {
            ViewData["TipoReporte"] = tipoReporte;
            return View();
        }


        [HttpGet]
        [Authorize(Policy = "Permissions.Reportes.Ver")]
        public async Task<IActionResult> ReporteCxC(string filtroUsuarioId, DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                // Construir consulta base con ordenamiento
                var query = _context.TbCxcs
                 .OrderBy(c => c.FidCuenta)
                    .ThenByDescending(c => c.FfechaInicio) // Segundo criterio de orden
                    .AsQueryable();

                // Cargar todos los datos requeridos desde la base de datos
                var cuentas = await _context.TbCxcs.ToListAsync();
                var inquilinos = await _context.TbInquilinos.ToListAsync();
                var inmuebles = await _context.TbInmuebles.ToListAsync();
                var usuarios = await _context.TbUsuarios.ToListAsync();

                // Aplicar filtros
                if (!string.IsNullOrEmpty(filtroUsuarioId))
                {
                    cuentas = cuentas.Where(c => c.FkidUsuario.ToString() == filtroUsuarioId).ToList();
                }

                if (fechaInicio.HasValue)
                {
                    cuentas = cuentas.Where(c => c.FfechaInicio >= fechaInicio.Value).ToList();
                }

                if (fechaFin.HasValue)
                {
                    cuentas = cuentas.Where(c => c.FfechaInicio <= fechaFin.Value).ToList();
                }

                // Mapear a ViewModel
                var reporte = cuentas.Select(c =>
                {
                    var inquilino = inquilinos.FirstOrDefault(i => i.FidInquilino == c.FkidInquilino);
                    var inmueble = inmuebles.FirstOrDefault(m => m.FidInmueble == c.FkidInmueble);
                    var usuario = usuarios.FirstOrDefault(u => u.FidUsuario == c.FkidUsuario);

                    return new ReporteViewModel
                    {
                        FidCuenta = c.FidCuenta,
                        NombreInquilino = inquilino != null
                            ? $"{inquilino.Fnombre?.Trim()} {inquilino.Fapellidos?.Trim()}".Trim()
                            : "N/A",
                        DescripcionInmueble = inmueble?.Fdescripcion ?? "Sin inmueble",
                        DireccionInmueble = inmueble?.Fdireccion ?? "Sin dirección",
                        UbicacionInmueble = inmueble?.Fubicacion ?? "Sin ubicación",
                        FechaActual = DateTime.Now.ToString("dd/MM/yyyy"),
                        HoraActual = DateTime.Now.ToString("HH:mm:ss"),
                        Fmonto = c.Fmonto,
                        FechaInicio = c.FfechaInicio.ToString("dd/MM/yyyy"),
                        NombreUsuario = usuario?.Fusuario ?? "Desconocido"
                    };
                }).ToList();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_ReporteCxCPartial", reporte);
                }

                return View(reporte);
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return StatusCode(500, new { success = false, message = ex.Message });
                }

                return View("Error");
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Reportes.Ver")]
        public async Task<IActionResult> ReporteCobros(string filtroUsuarioId, DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                // Cargar todos los datos requeridos desde la base de datos
                var cobros = await _context.TbCobros.ToListAsync();
                var cxcs = await _context.TbCxcs.ToListAsync();
                var inquilinos = await _context.TbInquilinos.ToListAsync();
                var inmuebles = await _context.TbInmuebles.ToListAsync();
                var usuarios = await _context.TbUsuarios.ToListAsync();

                // Mapear a ViewModel
                var reporte = cobros.Select(c =>
                {
                    var cxc = cxcs.FirstOrDefault(x => x.FidCuenta == c.FkidCxc);
                    var inquilino = cxc != null ? inquilinos.FirstOrDefault(i => i.FidInquilino == cxc.FkidInquilino) : null;

                    // Obtener el inmueble a través de la cuenta por cobrar
                    var inmueble = cxc != null ? inmuebles.FirstOrDefault(m => m.FidInmueble == cxc.FkidInmueble) : null;

                    var usuario = usuarios.FirstOrDefault(u => u.FidUsuario == c.FkidUsuario);

                    return new ReporteViewModel
                    {
                        FidCobro = c.FidCobro,
                        NombreInquilino = inquilino != null
                            ? $"{inquilino.Fnombre?.Trim()} {inquilino.Fapellidos?.Trim()}".Trim()
                            : "N/A",
                        DescripcionInmueble = inmueble?.Fdescripcion ?? "Sin inmueble",
                        FechaActual = DateTime.Now.ToString("dd/MM/yyyy"),
                        HoraActual = DateTime.Now.ToString("HH:mm:ss"),
                        Fmonto = c.Fmonto,
                        Fconcepto = c.Fconcepto,
                        FechaInicio = c.Ffecha.ToString("dd/MM/yyyy"),
                        NombreUsuario = usuario?.Fusuario ?? "Desconocido",
                        Factivo = c.Factivo,
                    };
                }).ToList();

                return PartialView("_ReporteCobrosPartial", reporte);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Reportes.Ver")]
        public async Task<IActionResult> ReporteAtrasos(string filtroUsuarioId)
        {
            try
            {
                // Cargar todos los datos requeridos desde la base de datos
                var inquilinos = await _context.TbInquilinos.ToListAsync();
                var inmuebles = await _context.TbInmuebles.ToListAsync();
                var cuentasPorCobrar = await _context.TbCxcs.ToListAsync();
                var cuotas = await _context.TbCxcCuota.ToListAsync();
                var usuarios = await _context.TbUsuarios.ToListAsync();

                // Mapear a ViewModel, filtrar y ordenar por FidCuenta
                var reporte = cuentasPorCobrar.Select(c =>
                {
                    // Obtener las cuotas vencidas para la cuenta por cobrar actual
                    var cuotasVencidas = cuotas
                        .Where(x => x.FkidCxc == c.FidCuenta && x.Fstatus == 'V')
                        .ToList();

                    // Obtener el inquilino y el inmueble relacionados
                    var inquilino = inquilinos.FirstOrDefault(i => i.FidInquilino == c.FkidInquilino);
                    var inmueble = inmuebles.FirstOrDefault(i => i.FidInmueble == c.FkidInmueble);
                    var usuario = usuarios.FirstOrDefault(u => u.FidUsuario == c.FkidUsuario);

                    // Calcular el monto total de atraso (suma de Fmonto)
                    var montoTotalAtraso = cuotasVencidas.Sum(x => x.Fmonto);

                    // Calcular la mora total (suma de Fmora)
                    var moraTotal = cuotasVencidas.Sum(x => x.Fmora);

                    // Calcular la cantidad de cuotas atrasadas
                    var cantCuotasAtrasadas = cuotasVencidas.Count;

                    // Calcular el total incluyendo la mora
                    var total = montoTotalAtraso + moraTotal;

                    return new ReporteViewModel
                    {
                        FidCuenta = c.FidCuenta,
                        NombreUsuario = usuario?.Fusuario ?? "Desconocido",
                        NombreInquilino = inquilino != null
                            ? $"{inquilino.Fnombre?.Trim()} {inquilino.Fapellidos?.Trim()}".Trim()
                            : "N/A",
                        DireccionInmueble = inmueble?.Fdescripcion,
                        UbicacionInmueble = inmueble?.Fdireccion,
                        CantCuotasAtrasadas = cantCuotasAtrasadas,
                        MontoTotalAtraso = montoTotalAtraso,
                        MoraTotal = moraTotal,
                        Total = total
                    };
                })
                .Where(x => x.CantCuotasAtrasadas > 0)  // Filtrar solo registros con cuotas atrasadas
                .OrderBy(x => x.FidCuenta)  // Ordenar por FidCuenta ascendente
                .ToList();

                return PartialView("_ReporteAtrasosPartial", reporte);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Reportes.Ver")]
        public IActionResult VistaReporteEstadoCuenta()
        {
            try
            {

                return PartialView("_ReporteEstadoCuentaPartial");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Reportes.Ver")]
        public async Task<IActionResult> ReporteEstadoCuenta(int FidInquilino, DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                _logger.LogInformation($"Iniciando búsqueda de inquilino ID: {FidInquilino}");

                // Verificar conexión a la base de datos
                _logger.LogInformation($"Probando conexión a la base de datos...");
                var canConnect = await _context.Database.CanConnectAsync();
                _logger.LogInformation($"Conexión a BD: {canConnect}");

                // Consulta explícita con logging
                var inquilino = await _context.TbInquilinos
                    .Where(i => i.FidInquilino == FidInquilino)
                    .FirstOrDefaultAsync();

                if (inquilino == null)
                {
                    _logger.LogWarning($"Inquilino con ID {FidInquilino} no encontrado");
                    return Json(new { success = false, message = "Inquilino no encontrado" });
                }

                _logger.LogInformation($"Inquilino encontrado: {inquilino.Fnombre} {inquilino.Fapellidos}");

                // Obtener cuentas por cobrar del inquilino usando join
                var cuentasCxC = await _context.TbCxcs
                    .Where(c => c.FkidInquilino == FidInquilino)
                    .ToListAsync();

                if (!cuentasCxC.Any())
                {
                    return Json(new { success = false, message = "No se encontraron cuentas por cobrar para este inquilino" });
                }

                // Obtener el inmueble relacionado
                var inmueble = await _context.TbInmuebles
                    .FirstOrDefaultAsync(i => i.FidInmueble == cuentasCxC.First().FkidInmueble);

                // Obtener cobros relacionados usando join en lugar de Contains
                var cobrosQuery = from c in _context.TbCobros
                                  join cuenta in _context.TbCxcs on c.FkidCxc equals cuenta.FidCuenta
                                  where cuenta.FkidInquilino == FidInquilino
                                  select c;

                // Aplicar filtros de fecha si existen
                if (fechaInicio.HasValue)
                {
                    cobrosQuery = cobrosQuery.Where(c => c.Ffecha >= DateOnly.FromDateTime(fechaInicio.Value));
                }

                if (fechaFin.HasValue)
                {
                    cobrosQuery = cobrosQuery.Where(c => c.Ffecha <= DateOnly.FromDateTime(fechaFin.Value));
                }

                var cobros = await cobrosQuery.ToListAsync();

                // Obtener detalles de cobros usando join
                var detallesCobros = await (from d in _context.TbCobrosDetalles
                                            join c in cobrosQuery on d.FkidCobro equals c.FidCobro
                                            select d).ToListAsync();

                // Obtener cuotas pendientes (Fstatus = 'N') usando join
                var cuotasPendientes = await (from c in _context.TbCxcCuota
                                              join cuenta in _context.TbCxcs on c.FkidCxc equals cuenta.FidCuenta
                                              where cuenta.FkidInquilino == FidInquilino && c.Fstatus == 'N'
                                              select c).ToListAsync();

                // Obtener todas las cuotas relacionadas usando join
                var cuotasRelacionadas = await (from c in _context.TbCxcCuota
                                                join cuenta in _context.TbCxcs on c.FkidCxc equals cuenta.FidCuenta
                                                where cuenta.FkidInquilino == FidInquilino
                                                select c).ToListAsync();

                // Construir el modelo de vista
                var reporte = new ReporteViewModel
                {
                    NombreInquilino = $"{inquilino.Fnombre?.Trim()} {inquilino.Fapellidos?.Trim()}".Trim(),
                    DireccionInmueble = inmueble?.Fdireccion ?? "Sin dirección",
                    UbicacionInmueble = inmueble?.Fubicacion ?? "Sin ubicación",
                    DescripcionInmueble = inmueble?.Fdescripcion ?? "Sin descripción",
                    FechaActual = DateTime.Now.ToString("dd/MM/yyyy"),
                    HoraActual = DateTime.Now.ToString("HH:mm:ss"),

                    // Pagos realizados (detalles de cobros)
                    Pagos = cobros.Select(c => new TbCobro
                    {
                        FidCobro = c.FidCobro,
                        Ffecha = c.Ffecha,
                        Fhora = c.Fhora,
                        Fmonto = c.Fmonto,
                        Fconcepto = c.Fconcepto,
                        Fdescuento = c.Fdescuento,
                        Fcargos = c.Fcargos,
                        Factivo = c.Factivo
                    }).ToList(),

                    // Cuotas pendientes
                    Pendientes = cuotasPendientes.Select(c => new TbCxcCuotum
                    {
                        FidCuota = c.FidCuota,
                        FNumeroCuota = c.FNumeroCuota,
                        Fvence = c.Fvence,
                        Fmonto = c.Fmonto,
                        Fsaldo = c.Fsaldo,
                        Fmora = c.Fmora,
                        FdiasAtraso = c.FdiasAtraso,
                        Fstatus = c.Fstatus
                    }).ToList(),

                    // Todas las cuotas relacionadas
                    Cuotas = cuotasRelacionadas.Select(c => new TbCxcCuotum
                    {
                        FidCuota = c.FidCuota,
                        FNumeroCuota = c.FNumeroCuota,
                        Fvence = c.Fvence,
                        Fmonto = c.Fmonto,
                        Fsaldo = c.Fsaldo,
                        Fmora = c.Fmora,
                        FdiasAtraso = c.FdiasAtraso,
                        Fstatus = c.Fstatus
                    }).ToList(),

                    // Datos adicionales
                    Fmonto = cuentasCxC.Sum(c => c.Fmonto),
                    FechaInicio = cuentasCxC.Min(c => c.FfechaInicio).ToString("dd/MM/yyyy")
                };

                return Json(new { success = true, data = reporte });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar el reporte de estado de cuenta");
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [Authorize(Policy = "Permissions.Reportes.Ver")]
        public async Task<IActionResult> ReporteCuentaGasto(DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                // Convertir fechas a DateOnly
                DateOnly? fechaInicioDateOnly = fechaInicio.HasValue ? DateOnly.FromDateTime(fechaInicio.Value) : null;
                DateOnly? fechaFinDateOnly = fechaFin.HasValue ? DateOnly.FromDateTime(fechaFin.Value) : null;

                // Ejecutar consultas secuencialmente
                var ingresos = await ObtenerTotalIngresos(fechaInicioDateOnly, fechaFinDateOnly);
                var gastos = await ObtenerTotalGastos(fechaInicioDateOnly, fechaFinDateOnly);

                // Verificar si hay datos en el rango de fechas
                bool hayDatos = ingresos != 0 || gastos != 0;

                var resultado = new ReporteViewModel
                {
                    Ingresos = ingresos,
                    Gastos = gastos,
                    HayDatos = hayDatos
                };

                return PartialView("_ReporteEstadoGastoPartial", resultado);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private async Task<decimal> ObtenerTotalIngresos(DateOnly? fechaInicio, DateOnly? fechaFin)
        {
            var query = _context.TbCobros.AsQueryable();

            if (fechaInicio.HasValue)
            {
                query = query.Where(c => c.Ffecha >= fechaInicio.Value);
            }

            if (fechaFin.HasValue)
            {
                var fechaFinInclusive = fechaFin.Value.AddDays(1);
                query = query.Where(c => c.Ffecha < fechaFinInclusive);
            }

            return await query.SumAsync(c => c.Fmonto);
        }

        private async Task<decimal> ObtenerTotalGastos(DateOnly? fechaInicio, DateOnly? fechaFin)
        {
            var query = _context.TbGastos.AsQueryable();

            if (fechaInicio.HasValue)
            {
                query = query.Where(g => g.Ffecha >= fechaInicio.Value);
            }

            if (fechaFin.HasValue)
            {
                var fechaFinInclusive = fechaFin.Value.AddDays(1);
                query = query.Where(g => g.Ffecha < fechaFinInclusive);
            }

            return await query.SumAsync(g => g.Fmonto);
        }

        [HttpGet]
        public async Task<IActionResult> BuscarUsuario(string term = null!)
        {
            try
            {
                var usersQuery = _context.TbUsuarios.AsQueryable();

                if (!string.IsNullOrWhiteSpace(term))
                {
                    term = term.ToLower();
                    usersQuery = usersQuery.Where(u =>
                        u.Fusuario.ToLower().Contains(term));
                }

                var results = await usersQuery
                    .OrderBy(u => u.Fusuario)
                    .Select(u => new
                    {
                        id = u.Fusuario, // El value del select2 será el nombre
                        text = u.Fusuario,
                        tipo = "usuario"
                    })
                    .ToListAsync();

                return Json(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios desde Identity.");
                return Json(new List<object>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> BuscarInquilino(string term = null!)
        {
            try
            {
                var usersQuery = _context.TbInquilinos.AsQueryable();

                if (!string.IsNullOrWhiteSpace(term))
                {
                    term = term.ToLower();
                    usersQuery = usersQuery.Where(u =>
                        u.Fnombre.ToLower().Contains(term) ||
                        u.Fapellidos.ToLower().Contains(term));
                }

                var results = await usersQuery
                    .OrderBy(u => u.Fnombre)
                    .Select(u => new
                    {
                        id = u.FidInquilino, // El value del select2 será el nombre
                        text = $"{u.Fnombre} {u.Fapellidos}",
                        tipo = "inquilino"
                    })
                    .ToListAsync();

                return Json(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener inquilino.");
                return Json(new List<object>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> BuscarInmueble(string term = null!)
        {
            try
            {
                var usersQuery = _context.TbInmuebles.AsQueryable();

                if (!string.IsNullOrWhiteSpace(term))
                {
                    term = term.ToLower();
                    usersQuery = usersQuery.Where(u =>
                        u.Fdescripcion.ToLower().Contains(term));
                }

                var results = await usersQuery
                    .OrderBy(u => u.FidInmueble)
                    .Select(u => new
                    {
                        id = u.Fdescripcion, // El value del select2 será el nombre
                        text = $"{u.Fdescripcion} - {u.Fdireccion}",
                        tipo = "inmueble"
                    })
                    .ToListAsync();

                return Json(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener inmueble.");
                return Json(new List<object>());
            }
        }
    }
}
