using Alquileres.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Alquileres.Controllers
{
    [Authorize] // Esto aplicará a todas las acciones del controlador, requiriendo que el usuario esté autenticado.
    public class TbCuotasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TbCuotasController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        // GET: TbCuotas/CargarCuotas
        public async Task<IActionResult> CargarCuota()
        {
            var cuotas = await _context.TbCxcCuota.ToListAsync();
            return PartialView("_CuotasPartial", cuotas); // Devuelve la vista parcial
        }
    }
}
