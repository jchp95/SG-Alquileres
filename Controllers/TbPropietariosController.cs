using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Alquileres.Models;
using Microsoft.AspNetCore.Authorization;
using Alquileres.Context;
using System.Linq.Expressions;

namespace Alquileres.Controllers
{
    [Authorize]
    public class TbPropietariosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GeneradorDeCuotasService> _logger; // Agregar logger

        public TbPropietariosController(ApplicationDbContext context, ILogger<GeneradorDeCuotasService> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var propietario = await _context.TbPropietarios.FindAsync(id);
            if (propietario == null)
            {
                return NotFound();
            }

            propietario.Factivo = !propietario.Factivo;
            _context.Update(propietario);
            await _context.SaveChangesAsync();

            var propietarios = await _context.TbPropietarios.ToListAsync();
            return PartialView("_PropietariosPartial", propietarios);
        }

        // GET: TbPropietarios
        public IActionResult Index()
        {
            return View();
        }

        // GET: TbPropietarios/CargarPropietarios
        public async Task<IActionResult> CargarPropietarios()
        {
            var propietarios = await _context.TbPropietarios.ToListAsync();
            return PartialView("_PropietariosPartial", propietarios);
        }

        // GET: TbPropietarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var propietario = await _context.TbPropietarios
                .FirstOrDefaultAsync(m => m.FidPropietario == id);
            if (propietario == null)
            {
                return NotFound();
            }

            return View(propietario);
        }

        // GET: TbPropietarios/Create
        public IActionResult Create()
        {
            return PartialView("_CreatePropietarioPartial", new TbPropietario());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FidPropietario,Fnombre,Fapellidos,Fcedula,Fdireccion,Ftelefono,Fcelular")] TbPropietario tbPropietario)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(tbPropietario.Fcedula))
                {
                    bool cedulaExiste = await _context.TbPropietarios
                        .AnyAsync(i => i.Fcedula.ToUpper() == tbPropietario.Fcedula.ToUpper());

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

                tbPropietario.Factivo = true;
                _context.Add(tbPropietario);
                await _context.SaveChangesAsync();

                var propietarios = await _context.TbPropietarios.ToListAsync();
                return PartialView("_PropietariosPartial", propietarios);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error de base de datos al crear propietario");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error crítico en la base de datos. Contacte al administrador."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error general al crear propietario");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor."
                });
            }
        }

        public async Task<IActionResult> VerificarCedula(string cedula)
        {
            bool existe = await _context.TbPropietarios
                .AnyAsync(i => i.Fcedula.ToUpper() == cedula.ToUpper());
            return Json(existe);
        }

        // GET: TbPropietarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var propietario = await _context.TbPropietarios.FindAsync(id);
            if (propietario == null)
            {
                return NotFound();
            }

            return PartialView("_EditPropietarioPartial", propietario);
        }

        // POST: TbPropietarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FidPropietario,Fnombre,Fapellidos,Fcedula,Fdireccion,Ftelefono,Fcelular")] TbPropietario tbPropietario)
        {
            if (id != tbPropietario.FidPropietario)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var propietario = await _context.TbPropietarios.FindAsync(id);
                    if (propietario == null)
                    {
                        return NotFound();
                    }

                    tbPropietario.Factivo = true;
                    _context.Entry(propietario).CurrentValues.SetValues(tbPropietario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TbPropietarioExists(tbPropietario.FidPropietario))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                var propietarios = await _context.TbPropietarios.ToListAsync();
                return PartialView("_PropietariosPartial", propietarios);
            }

            return PartialView("_EditPropietarioPartial", tbPropietario);
        }

        // GET: TbPropietarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var propietario = await _context.TbPropietarios
                .FirstOrDefaultAsync(m => m.FidPropietario == id);
            if (propietario == null)
            {
                return NotFound();
            }

            return View(propietario);
        }

        // POST: TbPropietarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var propietario = await _context.TbPropietarios.FindAsync(id);
            if (propietario != null)
            {
                _context.TbPropietarios.Remove(propietario);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TbPropietarioExists(int id)
        {
            return _context.TbPropietarios.Any(e => e.FidPropietario == id);
        }
    }
}
