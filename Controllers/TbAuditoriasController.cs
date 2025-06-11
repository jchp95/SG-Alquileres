using Alquileres.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Alquileres.Controllers
{
    public class TbAuditoriasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TbAuditoriasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Acción para cargar la vista inicial
        public IActionResult Index()
        {
            return View(); // Devuelve la vista principal
        }

        // Acción para cargar los datos de auditoría dinámicamente
        public async Task<IActionResult> CargarAuditoria()
        {
            var auditorias = await _context.TbAuditoria
                .Join(_context.TbUsuarios,
                    auditoria => auditoria.FkidUsuario,
                    usuario => usuario.FidUsuario,
                    (auditoria, usuario) => new
                    {
                        Auditoria = auditoria,
                        NombreUsuario = usuario.Fusuario
                    })
                .OrderByDescending(x => x.Auditoria.Fid)
                .ToListAsync();

            return PartialView("_AuditoriaPartial", auditorias);
        }
    }
}
