using Alquileres.Models;
using Alquileres.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace Alquileres.Controllers
{
    public class ReportesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReportesController> _logger;

        public ReportesController(ApplicationDbContext context, ILogger<ReportesController> logger)
        {
            _context = context;
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
                    var inquilino = inquilinos.FirstOrDefault(i => i.FidInquilino == c.FidInquilino);
                    var inmueble = inmuebles.FirstOrDefault(m => m.FidInmueble == c.FkidInmueble);
                    var usuario = usuarios.FirstOrDefault(u => u.FidUsuario == c.FkidUsuario);

                    return new ReporteViewModel
                    {
                        FidCuenta = c.FidCuenta,
                        NombreInquilino = inquilino != null
                            ? $"{inquilino.Fnombre?.Trim()} {inquilino.Fapellidos?.Trim()}".Trim()
                            : "N/A",
                        DireccionInmueble = inmueble?.Fdireccion ?? "Desconocido",
                        UbicacionInmueble = inmueble?.Fubicacion ?? "Desconocido",
                        FechaActual = DateTime.Now.ToString("dd/MM/yyyy"),
                        HoraActual = DateTime.Now.ToString("HH:mm:ss"),
                        Fmonto = c.Fmonto,
                        FechaInicio = c.FfechaInicio.ToString("dd/MM/yyyy"),
                        NombreUsuario = usuario?.Fusuario ?? "Desconocido"
                    };
                }).ToList();

                return PartialView("_ReporteCxCPartial", reporte);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permissions.Reportes.Ver")]
        public async Task<IActionResult> ReporteCobros(string filtroUsuarioId, DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                // Construir consulta base con ordenamiento
                var query = _context.TbCobros
                 .OrderBy(c => c.FidCobro)
                 .AsQueryable();

                // Cargar todos los datos requeridos desde la base de datos
                var cobros = await _context.TbCobros.ToListAsync();
                var cxcs = await _context.TbCxcs.ToListAsync();
                var inquilinos = await _context.TbInquilinos.ToListAsync();
                var usuarios = await _context.TbUsuarios.ToListAsync();

                // Mapear a ViewModel
                var reporte = cobros.Select(c =>
                {
                    var cxc = cxcs.FirstOrDefault(x => x.FidCuenta == c.FkidCxc);
                    var inquilino = cxc != null ? inquilinos.FirstOrDefault(i => i.FidInquilino == cxc.FidInquilino) : null;
                    var usuario = usuarios.FirstOrDefault(u => u.FidUsuario == c.FkidUsuario);

                    return new ReporteViewModel
                    {
                        FidCobro = c.FidCobro,
                        NombreInquilino = inquilino != null
                            ? $"{inquilino.Fnombre?.Trim()} {inquilino.Fapellidos?.Trim()}".Trim()
                            : "N/A",
                        FechaActual = DateTime.Now.ToString("dd/MM/yyyy"),
                        HoraActual = DateTime.Now.ToString("HH:mm:ss"),
                        Fmonto = c.Fmonto,
                        Fconcepto = c.Fconcepto,
                        FechaInicio = c.Ffecha.ToString("dd/MM/yyyy"),
                        NombreUsuario = usuario?.Fusuario ?? "Desconocido"
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

                // Mapear a ViewModel y ordenar por FidCuenta
                var reporte = cuentasPorCobrar.Select(c =>
                {
                    // Obtener las cuotas vencidas para la cuenta por cobrar actual
                    var cuotasVencidas = cuotas
                        .Where(x => x.FidCxc == c.FidCuenta && x.Fstatus == 'V')
                        .ToList();

                    // Obtener el inquilino y el inmueble relacionados
                    var inquilino = inquilinos.FirstOrDefault(i => i.FidInquilino == c.FidInquilino);
                    var inmueble = inmuebles.FirstOrDefault(i => i.FidInmueble == c.FkidInmueble);
                    var usuario = usuarios.FirstOrDefault(u => u.FidUsuario == c.FkidUsuario);

                    // Calcular el monto total de atraso
                    var montoTotalAtraso = cuotasVencidas.Sum(x => x.Fmonto);

                    // Calcular la cantidad de cuotas atrasadas
                    var cantCuotasAtrasadas = cuotasVencidas.Count;

                    // Calcular el total incluyendo la mora
                    var total = c.FtasaMora > 0
                        ? montoTotalAtraso + (montoTotalAtraso * (c.FtasaMora / 100)) // %
                        : montoTotalAtraso; // Si la mora es 0%, el total es igual al monto total de atraso

                    return new ReporteViewModel
                    {
                        FidCuenta = c.FidCuenta,
                        NombreUsuario = usuario?.Fusuario ?? "Desconocido",
                        NombreInquilino = inquilino != null
                            ? $"{inquilino.Fnombre?.Trim()} {inquilino.Fapellidos?.Trim()}".Trim()
                            : "N/A",
                        DireccionInmueble = inmueble?.Fdescripcion,
                        UbicacionInmueble = inmueble?.Fdireccion,
                        CantCuotasAtrasadas = cantCuotasAtrasadas, // Cantidad de cuotas vencidas
                        MontoTotalAtraso = montoTotalAtraso,
                        Mora = c.FtasaMora,
                        Total = total
                    };
                })
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
        public async Task<IActionResult> BuscarUsuario(string term = null)
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
                        text = u.Fusuario
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
    }
}
