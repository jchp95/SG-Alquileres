using System.Diagnostics;
using Alquileres.Models;
using Alquileres.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Alquileres.Services; // Añade este using para usar Entity Framework

namespace Alquileres.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
            : base(context) // ← necesario para pasar ApplicationDbContext a BaseController
        {
            _logger = logger;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult CheckCuotasVencidas()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Json(new { tieneCuotasVencidas = false, cantidad = 0, montoTotal = 0 });
            }

            var cuotasVencidas = _context.TbCxcCuota
                .Where(c => c.Fstatus == 'V')
                .Select(c => new
                {
                    c.FidCuota,
                    c.FNumeroCuota,
                    c.Fmonto,
                    c.Fvence,
                })
                .ToList();

            var montoTotal = cuotasVencidas.Sum(c => c.Fmonto);

            return Json(new
            {
                tieneCuotasVencidas = cuotasVencidas.Count > 0,
                cantidad = cuotasVencidas.Count,
                montoTotal = montoTotal,
                cuotas = cuotasVencidas
            });
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Detectar si es móvil y el usuario no está autenticado
            if (!(User.Identity?.IsAuthenticated ?? false) && IsMobileDevice())
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var hoy = DateTime.Now;
            var inicioMesActual = new DateTime(hoy.Year, hoy.Month, 1);
            var inicioMesAnterior = inicioMesActual.AddMonths(-1);
            var finMesAnterior = inicioMesActual.AddDays(-1);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var nombreUsuario = string.Empty;

            bool tutorialVisto = false;

            if (!string.IsNullOrEmpty(userId))
            {
                // Consulta directa a la base de datos sin usar el servicio
                var usuario = await _context.TbUsuarios
                    .FirstOrDefaultAsync(u => u.IdentityId == userId);

                nombreUsuario = usuario?.Fnombre ?? User.Identity?.Name;
                tutorialVisto = usuario?.FTutorialVisto ?? false;
            }

            ViewBag.NombreUsuario = nombreUsuario;
            ViewBag.MostrarTutorial = !tutorialVisto;

            // Total de inmuebles
            var totalInmuebles = _context.TbInmuebles.Count();

            // Inmuebles activos
            var inmueblesActivosActual = _context.TbInmuebles
                .Where(i => i.Factivo)
                .ToList();

            // Inmuebles activos mes pasado
            var inmueblesActivosMesPasado = _context.TbInmuebles
                .Where(i => i.Factivo && i.FfechaRegistro <= finMesAnterior)
                .ToList();

            int countActual = inmueblesActivosActual.Count;
            int countAnterior = inmueblesActivosMesPasado.Count;

            // Cálculo de ocupación
            double ocupacionActual = totalInmuebles > 0 ? ((double)countActual / totalInmuebles) * 100 : 0;
            double ocupacionMesPasado = totalInmuebles > 0 ? ((double)countAnterior / totalInmuebles) * 100 : 0;

            // Cambio en la ocupación
            double? cambioOcupacion = null;
            string textoCambioOcupacion;
            string claseCambioOcupacion;

            if (countAnterior > 0)
            {
                cambioOcupacion = ocupacionActual - ocupacionMesPasado;
                textoCambioOcupacion = $"{(cambioOcupacion >= 0 ? "+" : "")}{cambioOcupacion.Value:F2}% vs mes pasado";
                claseCambioOcupacion = cambioOcupacion >= 0 ? "positive" : "negative";
            }
            else if (countActual > 0)
            {
                textoCambioOcupacion = "Nuevo este mes";
                claseCambioOcupacion = "positive";
            }
            else
            {
                textoCambioOcupacion = "Sin cambios";
                claseCambioOcupacion = "neutral";
            }

            // Cuotas vencidas
            var cuotasVencidasData = _context.TbCxcCuota
                .Where(c => c.Fstatus == 'V')
                .Select(c => new { c.Fmonto })
                .ToList();
            
            int cuotasVencidasCount = cuotasVencidasData.Count;
            decimal montoTotalVencidas = cuotasVencidasData.Sum(c => c.Fmonto);

            // Cobros del día
            decimal cobrosHoy = _context.TbCobros
                .Where(c => c.Ffecha == DateOnly.FromDateTime(DateTime.Today) && c.Factivo)
                .Sum(c => (decimal?)c.Fmonto) ?? 0;

            // Ingresos actuales
            decimal ingresosActual = _context.TbCobros
                .AsEnumerable()
                .Where(c => new DateTime(c.Ffecha.Year, c.Ffecha.Month, c.Ffecha.Day) >= inicioMesActual && c.Factivo)
                .Sum(c => (decimal?)c.Fmonto) ?? 0;

            // Ingresos mes anterior
            decimal ingresosAnterior = _context.TbCobros
                .AsEnumerable()
                .Where(c =>
                {
                    var fecha = new DateTime(c.Ffecha.Year, c.Ffecha.Month, c.Ffecha.Day);
                    return fecha >= inicioMesAnterior && fecha < inicioMesActual && c.Factivo;
                })
                .Sum(c => (decimal?)c.Fmonto) ?? 0;

            // Ingresos mensuales esperados (suma de todas las CxC activas)
            decimal ingresosMensualesEsperados = _context.TbCxcs
                .Where(c => c.Factivo)
                .Sum(c => (decimal?)c.Fmonto) ?? 0;

            // Cambio en ingresos
            decimal? cambioIngresos = null;
            string textoCambioIngresos;
            string claseCambioIngresos;

            if (ingresosAnterior > 0)
            {
                cambioIngresos = ((ingresosActual - ingresosAnterior) / ingresosAnterior) * 100;
                textoCambioIngresos = $"{(cambioIngresos >= 0 ? "+" : "")}{cambioIngresos.Value:F2}% vs mes pasado";
                claseCambioIngresos = cambioIngresos >= 0 ? "positive" : "negative";
            }
            else if (ingresosActual > 0)
            {
                textoCambioIngresos = "Nuevo este mes";
                claseCambioIngresos = "positive";
            }
            else
            {
                textoCambioIngresos = "Sin cambios";
                claseCambioIngresos = "neutral";
            }

            // =========================
            // Actividades recientes
            // =========================
            var actividades = new List<ActividadReciente>();

            var inquilinosRecientes = _context.TbInquilinos
                .OrderByDescending(i => i.FfechaRegistro)
                .Take(5)
                .Select(i => new ActividadReciente
                {
                    Tipo = "Inquilino",
                    Descripcion = $"Inquilino agregado: {i.Fnombre} {i.Fapellidos}",
                    Fecha = i.FfechaRegistro
                });

            var propietariosRecientes = _context.TbPropietarios
                .OrderByDescending(p => p.FfechaRegistro)
                .Take(5)
                .Select(p => new ActividadReciente
                {
                    Tipo = "Propietario",
                    Descripcion = $"Propietario registrado: {p.Fnombre} {p.Fapellidos}",
                    Fecha = p.FfechaRegistro
                });

            var inmueblesRecientes = _context.TbInmuebles
                .OrderByDescending(i => i.FfechaRegistro)
                .Take(5)
                .Select(i => new ActividadReciente
                {
                    Tipo = "Inmueble",
                    Descripcion = $"Nuevo inmueble: {i.Fdescripcion}",
                    Fecha = i.FfechaRegistro
                });

            var cxCRecientes = _context.TbCxcs
                .Where(i => i.Factivo)
                .OrderByDescending(i => i.FfechaInicio)
                .Take(5)
                .Join(
                    _context.TbInmuebles,
                    cxc => cxc.FkidInmueble,
                    inmueble => inmueble.FidInmueble,
                    (cxc, inmueble) => new { cxc, inmueble })
                .AsEnumerable() // <-- fuerza evaluación en memoria
                .Select(x => new ActividadReciente
                {
                    Tipo = "CxC",
                    Descripcion = $"Cuenta por cobrar: Monto: {x.cxc.Fmonto.ToString("C")}",
                    Fecha = x.cxc.FfechaInicio,
                    DetalleAdicional = $"Inmueble: {x.inmueble.Fdescripcion ?? "Sin descripción"}, Fecha próxima cuota: {x.cxc.FfechaProxCuota.ToShortDateString()}",
                    EsUrgente = (x.cxc.FfechaProxCuota - DateTime.Now).Days <= 3
                })
                .ToList();


            var cobrosRecientes = _context.TbCobros
                .Where(i => i.Factivo)
                .AsEnumerable() // Necesario para usar la propiedad calculada
                .OrderByDescending(i => i.FechaCompleta)
                .Take(5)
                .Select(i => new ActividadReciente
                {
                    Tipo = "Cobro",
                    Descripcion = $"Cobro: Monto: {i.Fmonto.ToString("C")} - Concepto: {i.Fconcepto}",
                    Fecha = i.FechaCompleta,
                })
                .ToList();

            actividades.AddRange(inquilinosRecientes);
            actividades.AddRange(propietariosRecientes);
            actividades.AddRange(inmueblesRecientes);
            actividades.AddRange(cxCRecientes);
            actividades.AddRange(cobrosRecientes);

            var actividadesOrdenadas = actividades
                .OrderByDescending(a => a.Fecha)
                .Take(5)
                .ToList();

            // =========================
            // ViewModel Final
            // =========================
            var viewModel = new DashboardViewModel
            {
                InmueblesActivosCount = countActual,
                InmueblesActivosMesPasado = countAnterior,
                PorcentajeCambioInmuebles = cambioOcupacion,
                TextoCambioInmuebles = textoCambioOcupacion,
                ClaseCambioInmuebles = claseCambioOcupacion,
                InmueblesActivos = inmueblesActivosActual,
                CuotasVencidasCount = cuotasVencidasCount,
                MontoTotalCuotasVencidas = montoTotalVencidas,
                OcupacionActual = ocupacionActual,
                OcupacionMesPasado = ocupacionMesPasado,
                TotalInmuebles = totalInmuebles,
                IngresosMensuales = ingresosActual,
                IngresosMesAnterior = ingresosAnterior,
                IngresosMensualesEsperados = ingresosMensualesEsperados,
                CambioIngresos = cambioIngresos,
                TextoCambioIngresos = textoCambioIngresos,
                ClaseCambioIngresos = claseCambioIngresos,
                ActividadesRecientes = actividadesOrdenadas,
                CobrosHoy = cobrosHoy
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetActivities(int page = 1, int pageSize = 10)
        {
            var actividades = await ObtenerTodasLasActividadesRecientes(page, pageSize);
            return PartialView("_ActivitiesPartial", actividades);
        }

        private async Task<List<ActividadReciente>> ObtenerTodasLasActividadesRecientes(int page = 1, int pageSize = 10)
        {
            var actividades = new List<ActividadReciente>();

            // Obtener todas las actividades (similar a tu método Index pero sin Take(5))
            var inquilinosRecientes = _context.TbInquilinos
                .OrderByDescending(i => i.FfechaRegistro)
                .Select(i => new ActividadReciente
                {
                    Tipo = "Inquilino",
                    Descripcion = $"Inquilino agregado: {i.Fnombre} {i.Fapellidos}",
                    Fecha = i.FfechaRegistro
                });

            var propietariosRecientes = _context.TbPropietarios
                .OrderByDescending(p => p.FfechaRegistro)
                .Select(p => new ActividadReciente
                {
                    Tipo = "Propietario",
                    Descripcion = $"Propietario registrado: {p.Fnombre} {p.Fapellidos}",
                    Fecha = p.FfechaRegistro
                });

            var inmueblesRecientes = _context.TbInmuebles
                .OrderByDescending(i => i.FfechaRegistro)
                .Select(i => new ActividadReciente
                {
                    Tipo = "Inmueble",
                    Descripcion = $"Nuevo inmueble: {i.Fdescripcion}",
                    Fecha = i.FfechaRegistro
                });

            var cxCRecientes = _context.TbCxcs
                .Where(i => i.Factivo)
                .OrderByDescending(i => i.FfechaInicio)
                .Join(
                    _context.TbInmuebles,
                    cxc => cxc.FkidInmueble,
                    inmueble => inmueble.FidInmueble,
                    (cxc, inmueble) => new ActividadReciente
                    {
                        Tipo = "CxC",
                        Descripcion = $"Cuenta por cobrar: Monto: {cxc.Fmonto.ToString("C")}",
                        Fecha = cxc.FfechaInicio,
                        DetalleAdicional = $"Inmueble: {inmueble.Fdescripcion ?? "Sin descripción"}, Fecha próxima cuota: {cxc.FfechaProxCuota.ToShortDateString()}",
                        EsUrgente = EF.Functions.DateDiffDay(DateTime.Now, cxc.FfechaProxCuota) <= 3
                    });

            var cobrosRecientes = _context.TbCobros
                .Where(i => i.Factivo)
                .AsEnumerable()
                .OrderByDescending(i => i.FechaCompleta)
                .Select(i => new ActividadReciente
                {
                    Tipo = "Cobro",
                    Descripcion = $"Cobro: Monto: {i.Fmonto.ToString("C")} - Concepto: {i.Fconcepto}",
                    Fecha = i.FechaCompleta,
                });

            actividades.AddRange(await inquilinosRecientes.ToListAsync());
            actividades.AddRange(await propietariosRecientes.ToListAsync());
            actividades.AddRange(await inmueblesRecientes.ToListAsync());
            actividades.AddRange(await cxCRecientes.ToListAsync());
            actividades.AddRange(cobrosRecientes);

            // Ordenar y paginar
            return actividades
                .OrderByDescending(a => a.Fecha)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarTutorialVisto()
        {
            try
            {
                // Obtener el IdentityId del usuario actual desde los claims
                var identityId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(identityId))
                {
                    _logger.LogWarning("Intento de marcar tutorial visto sin IdentityId en los claims");
                    return BadRequest(new { success = false, message = "No se pudo identificar al usuario" });
                }

                // Buscar el usuario en TbUsuario usando el IdentityId
                var usuario = await _context.TbUsuarios
                    .FirstOrDefaultAsync(u => u.IdentityId == identityId);

                if (usuario == null)
                {
                    _logger.LogWarning($"Usuario con IdentityId {identityId} no encontrado en TbUsuarios");
                    return NotFound(new { success = false, message = "Usuario no encontrado" });
                }

                // Si ya estaba marcado, retornar sin cambios
                if (usuario.FTutorialVisto)
                {
                    return Ok(new { success = true, message = "El tutorial ya estaba marcado como visto" });
                }

                // Marcar el tutorial como visto
                usuario.FTutorialVisto = true;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Tutorial marcado como visto para el usuario ID: {usuario.FidUsuario}");
                return Ok(new { success = true, message = "Tutorial marcado como visto" });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Error al actualizar la base de datos");
                return StatusCode(500, new { success = false, message = "Error al guardar en base de datos" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al marcar tutorial como visto");
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        // Método para detectar si el dispositivo es móvil
        private bool IsMobileDevice()
        {
            var userAgent = Request.Headers["User-Agent"].ToString().ToLower();
            
            string[] mobileKeywords = { 
                "mobile", "android", "iphone", "ipad", "ipod", 
                "blackberry", "windows phone", "opera mini", 
                "iemobile", "webos", "palm"
            };
            
            return mobileKeywords.Any(keyword => userAgent.Contains(keyword));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}