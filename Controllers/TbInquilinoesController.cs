using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Alquileres.Models;
using Microsoft.AspNetCore.Authorization;
using Alquileres.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;  // Necesario para [Authorize]

namespace Alquileres.Controllers
{
    [Authorize] // Esto aplicará a todas las acciones del controlador, requiriendo que el usuario esté autenticado.
    public class TbInquilinoesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GeneradorDeCuotasService> _logger; // Agregar logger
        private readonly UserManager<IdentityUser> _userManager;

        public TbInquilinoesController(ApplicationDbContext context, ILogger<GeneradorDeCuotasService> logger, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager; // Asignación del UserManager
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var inquilino = await _context.TbInquilinos.FindAsync(id);
            if (inquilino == null)
            {
                return NotFound();
            }

            // Cambiar el estado del inquilino
            inquilino.Factivo = !inquilino.Factivo;
            _context.Update(inquilino);
            await _context.SaveChangesAsync();

            // Devolver la vista parcial actualizada
            var inquilinos = await _context.TbInquilinos.ToListAsync();
            return PartialView("_InquilinosPartial", inquilinos);
        }

        // GET: TbInquilinoes
        public IActionResult Index()
        {
            return View(); // Devuelve la vista principal
        }

        // GET: TbInquilinoes/CargarInquilinos
        public async Task<IActionResult> CargarInquilinos()
        {
            var inquilinos = await _context.TbInquilinos.ToListAsync();
            return PartialView("_InquilinosPartial", inquilinos); // Devuelve la vista parcial
        }

        // GET: TbInquilinoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tbInquilino = await _context.TbInquilinos
                .FirstOrDefaultAsync(m => m.FidInquilino == id);
            if (tbInquilino == null)
            {
                return NotFound();
            }

            return View(tbInquilino);
        }

        // GET: TbInquilinoes/Create
        public IActionResult Create()
        {
            // Devuelve la vista parcial en lugar de la vista completa
            return PartialView("_CreateInquilinoPartial", new TbInquilino());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FidInquilino,Fnombre,Fapellidos,Fcedula,Fdireccion,Ftelefono,Fcelular")] TbInquilino tbInquilino)
        {
            try
            {
                var identityId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(identityId))
                    return BadRequest("El usuario no está autenticado.");

                var usuario = await _context.TbUsuarios.FirstOrDefaultAsync(u => u.IdentityId == identityId);
                if (usuario == null)
                    return BadRequest($"No se encontró un usuario con el IdentityId: {identityId}");

                tbInquilino.FkidUsuario = usuario.FidUsuario;

                if (!string.IsNullOrWhiteSpace(tbInquilino.Fcedula))
                {
                    bool cedulaExiste = await _context.TbInquilinos
                        .AnyAsync(i => i.Fcedula.ToUpper() == tbInquilino.Fcedula.ToUpper());

                    if (cedulaExiste)
                    {
                        ModelState.AddModelError("Fcedula", "Esta cédula ya está registrada");
                    }
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).FirstOrDefault()
                    );

                    return BadRequest(new
                    {
                        success = false,
                        errors = errors
                    });
                }

                tbInquilino.Factivo = true;
                _context.Add(tbInquilino);
                await _context.SaveChangesAsync();

                var inquilinos = await _context.TbInquilinos.ToListAsync();
                return PartialView("_InquilinosPartial", inquilinos);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error de base de datos al crear inquilino");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error crítico en la base de datos. Contacte al administrador."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error general al crear inquilino");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor."
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> VerificarCedula(string cedula)
        {
            bool existe = await _context.TbInquilinos
                .AnyAsync(i => i.Fcedula.ToUpper() == cedula.ToUpper());
            return Json(existe);
        }


        // GET: TbInquilinoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tbInquilino = await _context.TbInquilinos.FindAsync(id);
            if (tbInquilino == null)
            {
                return NotFound();
            }

            return PartialView("_EditInquilinoPartial", tbInquilino); // <- Usamos PartialView
        }


        // POST: TbInquilinoes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
    [Bind("FidInquilino,Fnombre,Fapellidos,Fcedula,Fdireccion,Ftelefono,Fcelular,FkidUsuario")] TbInquilino tbInquilino)
        {
            if (id != tbInquilino.FidInquilino)
            {
                return NotFound();
            }

            // Conservar el estado original de Factivo
            var originalInquilino = await _context.TbInquilinos
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.FidInquilino == id);

            if (originalInquilino != null)
            {
                tbInquilino.Factivo = originalInquilino.Factivo;
                tbInquilino.FkidUsuario = originalInquilino.FkidUsuario; // Asegurar la FK
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tbInquilino);
                    await _context.SaveChangesAsync();

                    var inquilinos = await _context.TbInquilinos.ToListAsync();
                    return PartialView("_InquilinosPartial", inquilinos);
                }
                catch (DbUpdateException ex)
                {
                    // Manejar error de FK específicamente
                    if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 547)
                    {
                        ModelState.AddModelError("", "El usuario asociado no existe");
                        return PartialView("_EditInquilinoPartial", tbInquilino);
                    }
                    throw;
                }
            }
            return PartialView("_EditInquilinoPartial", tbInquilino);
        }

        private bool TbInquilinoExists(int id)
        {
            return _context.TbInquilinos.Any(e => e.FidInquilino == id);
        }

    }
}
