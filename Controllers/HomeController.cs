using System.Diagnostics;
using Alquileres.Models;
using Alquileres.Context; // Asegúrate de incluir esto
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity; // Para usar LINQ

namespace Alquileres.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context; // Agregar el contexto

        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger,
                             ApplicationDbContext context,
                             UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult LoadCreateInquilinoPartial()
        {
            return PartialView("~/Views/TbInquilinoes/_CreateInquilinoPartial.cshtml");
        }

        public IActionResult LoadCreatePropietarioPartial()
        {
            return PartialView("~/Views/TbPropietarios/_CreatePropietarioPartial.cshtml");
        }

        public IActionResult LoadCreateInmueblePartial()
        {
            return PartialView("~/Views/TbInmuebles/_CreateInmueblePartial.cshtml");
        }

        public IActionResult LoadCreateCxCPartial()
        {
            return PartialView("~/Views/TbCuentasPorCobrar/_CreateCuentasPorCobrarPartial.cshtml");
        }

        public IActionResult LoadCreateCobroPartial()
        {
            return PartialView("~/Views/TbCobros/_CreateCobroPartial.cshtml");
        }

        public async Task<IActionResult> LoadDashboard()
        {
            var hoy = DateTime.Today;
            var cuotasVencidas = _context.TbCxcCuota
                .Where(c => c.Factivo && c.Fvence <= hoy && c.Fstatus == 'V')
                .ToList();

            int cantidadCuotasVencidas = cuotasVencidas.Count;

            ViewData["AlertaCuotasVencidas"] = cantidadCuotasVencidas > 0;
            ViewData["CantidadCuotasVencidas"] = cantidadCuotasVencidas;
            ViewData["CuotasVencidas"] = cuotasVencidas;

            // Obtener el usuario actual
            var usuario = await _userManager.GetUserAsync(User);
            var nombreMostrar = usuario?.UserName ?? "Usuario";

            // Si tienes propiedades personalizadas como NombreCompleto:
            // var nombreMostrar = usuario?.NombreCompleto ?? usuario?.UserName ?? "Usuario";

            ViewData["NombreUsuario"] = nombreMostrar;

            return PartialView("_DashboardPartial");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}