using System.Diagnostics;
using Alquileres.Models;
using Alquileres.Context; // Asegúrate de incluir esto
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore; // Para usar LINQ

namespace Alquileres.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context; // Agregar el contexto

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context; // Inicializar el contexto
        }

        public IActionResult Index()
        {
            var hoy = DateTime.Today;
            var cuotasVencidas = _context.TbCxcCuota
                .Where(c => c.Factivo && c.Fvence <= hoy && c.Fstatus == 'V') // Solo cuotas con estado 'N'
                .ToList();

            int cantidadCuotasVencidas = cuotasVencidas.Count;

            ViewData["AlertaCuotasVencidas"] = cantidadCuotasVencidas > 0;
            ViewData["CantidadCuotasVencidas"] = cantidadCuotasVencidas;
            ViewData["CuotasVencidas"] = cuotasVencidas;

            return View();
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