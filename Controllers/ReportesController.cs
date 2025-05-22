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
