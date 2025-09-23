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
    public class TbPropietariosController : BaseController
    {

        private readonly ILogger<GeneradorDeCuotasService> _logger; // Agregar logger

        public TbPropietariosController(ApplicationDbContext context,
        ILogger<GeneradorDeCuotasService> logger) : base(context)
        {
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Policy = "Permissions.Propietarios.Anular")]
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
        public IActionResult Index(string vista = "crear")
        {
            ViewData["Vista"] = vista;
            return View();
        }


        // GET: TbPropietarios/CargarPropietarios
        [HttpGet]
        [Authorize(Policy = "Permissions.Propietarios.Ver")]
        public async Task<IActionResult> CargarPropietarios()
        {
            var propietarios = await _context.TbPropietarios
                .OrderBy(i => i.FidPropietario)
                .ToListAsync();
            return PartialView("_PropietariosPartial", propietarios);
        }

        // GET: TbPropietarios/Create
        [HttpGet]
        [Authorize(Policy = "Permissions.Propietarios.Crear")]
        public IActionResult Create()
        {
            return PartialView("_CreatePropietarioPartial", new TbPropietario());
        }

        [HttpPost]
        [Authorize(Policy = "Permissions.Propietarios.Crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FidPropietario,Fnombre,Fapellidos,Fcedula,Fdireccion,Ftelefono,Fcelular")] TbPropietario tbPropietario)
        {
            try
            {
                // Verificar si la cédula ya existe
                if (!string.IsNullOrWhiteSpace(tbPropietario.Fcedula))
                {
                    bool cedulaExiste = await _context.TbPropietarios
                        .AnyAsync(i => i.Fcedula.ToUpper() == tbPropietario.Fcedula.ToUpper());

                    if (cedulaExiste)
                    {
                        ModelState.AddModelError("Fcedula", "Esta cédula ya está registrada");
                    }
                }

                // Validación
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                    return BadRequest(new
                    {
                        success = false,
                        errors = errors
                    });
                }

                tbPropietario.FfechaRegistro = DateTime.Now;
                tbPropietario.Factivo = true;

                _context.Add(tbPropietario);
                await _context.SaveChangesAsync();

                // 🚀 En vez de PartialView devolvemos JSON
                return Ok(new { success = true });
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
        [HttpGet]
        [Authorize(Policy = "Permissions.Propietarios.Editar")]
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
        [Authorize(Policy = "Permissions.Propietarios.Editar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FidPropietario,Fnombre,Fapellidos,Fcedula,Fdireccion,Ftelefono,Fcelular")] TbPropietario tbPropietario)
        {
            if (id != tbPropietario.FidPropietario)
            {
                return NotFound();
            }

            // Verificar si la cédula ya existe, excluyendo el propietario actual
            if (!string.IsNullOrWhiteSpace(tbPropietario.Fcedula))
            {
                bool cedulaExiste = await _context.TbPropietarios
                    .AnyAsync(p => p.Fcedula.ToUpper() == tbPropietario.Fcedula.ToUpper() && p.FidPropietario != id);

                if (cedulaExiste)
                {
                    ModelState.AddModelError("Fcedula", "Esta cédula ya está registrada");
                }
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

                    var propietarios = await _context.TbPropietarios.ToListAsync();
                    return PartialView("_PropietariosPartial", propietarios);
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
            }

            // Si hay errores de validación, devolver el formulario con errores
            var errors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );

            return BadRequest(new
            {
                success = false,
                errors = errors
            });
        }

        private bool TbPropietarioExists(int id)
        {
            return _context.TbPropietarios.Any(e => e.FidPropietario == id);
        }
    }
}
