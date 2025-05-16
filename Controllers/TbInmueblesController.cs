using Alquileres.Context;
using Alquileres.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Alquileres.Controllers
{
    [Authorize]
    public class TbInmueblesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TbInmueblesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var inmueble = await _context.TbInmuebles.FindAsync(id);
            if (inmueble == null)
            {
                return NotFound();
            }

            // Cambiar el estado del inmueble
            inmueble.Factivo = !inmueble.Factivo;

            _context.Update(inmueble);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: TbInmuebles
        public IActionResult Index()
        {
            return View();
        }

        // GET: TbInmuebles/CargarInmuebles
        public async Task<IActionResult> CargarInmuebles()
        {
            var inmuebles = await _context.TbInmuebles.ToListAsync();
            var propietarios = await _context.TbPropietarios
                .Where(p => p.Factivo)
                .ToDictionaryAsync(p => p.FidPropietario, p => p.Fnombre);

            var inmuebleViewModels = inmuebles.Select(i => new InmuebleViewModel
            {
                FidInmueble = i.FidInmueble,
                Fdescripcion = i.Fdescripcion,
                Fdireccion = i.Fdireccion,
                Fubicacion = i.Fubicacion,
                Fprecio = i.Fprecio,
                Factivo = i.Factivo,
                PropietarioNombre = propietarios.ContainsKey(i.FkidPropietario) ? propietarios[i.FkidPropietario] : "Desconocido"
            }).ToList();

            return PartialView("_InmueblesPartial", inmuebleViewModels);
        }

        // GET: TbInmuebles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tbInmueble = await _context.TbInmuebles.FirstOrDefaultAsync(m => m.FidInmueble == id);
            if (tbInmueble == null)
            {
                return NotFound();
            }

            return View(tbInmueble);
        }

        // GET: TbInmuebles/Create
        public async Task<IActionResult> Create()
        {
            var propietarios = await _context.TbPropietarios
                .Where(p => p.Factivo)
                .Select(p => new { p.FidPropietario, p.Fnombre })
                .ToListAsync();

            var propietarioList = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Seleccionar propietario" }
            };

            propietarioList.AddRange(propietarios.Select(p => new SelectListItem
            {
                Value = p.FidPropietario.ToString(),
                Text = p.Fnombre
            }));

            ViewBag.FkidPropietario = propietarioList;

            return PartialView("_CreateInmueblePartial", new TbInmueble());
        }

        // POST: TbInmuebles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FidInmueble,FkidPropietario,Fdescripcion,Fdireccion,Fubicacion,Fprecio")] TbInmueble tbInmueble)
        {
            if (ModelState.IsValid)
            {
                tbInmueble.Factivo = true;
                _context.Add(tbInmueble);
                await _context.SaveChangesAsync();

                // Cargar los inmuebles y crear la lista de InmuebleViewModel
                var inmuebles = await _context.TbInmuebles.ToListAsync();
                var propietarios = await _context.TbPropietarios
                    .Where(p => p.Factivo)
                    .ToDictionaryAsync(p => p.FidPropietario, p => p.Fnombre);

                var inmuebleViewModels = inmuebles.Select(i => new InmuebleViewModel
                {
                    FidInmueble = i.FidInmueble,
                    Fdescripcion = i.Fdescripcion,
                    Fdireccion = i.Fdireccion,
                    Fubicacion = i.Fubicacion,
                    Fprecio = i.Fprecio,
                    Factivo = i.Factivo,
                    PropietarioNombre = propietarios.ContainsKey(i.FkidPropietario) ? propietarios[i.FkidPropietario] : "Desconocido"
                }).ToList();

                return PartialView("_InmueblesPartial", inmuebleViewModels);
            }

            return PartialView("_CreateInmueblePartial", tbInmueble);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tbInmueble = await _context.TbInmuebles.FindAsync(id);
            if (tbInmueble == null)
            {
                return NotFound();
            }

            // Obtener el nombre del propietario
            var propietario = await _context.TbPropietarios
                .Where(p => p.FidPropietario == tbInmueble.FkidPropietario)
                .Select(p => p.Fnombre)
                .FirstOrDefaultAsync();

            ViewBag.PropietarioNombre = propietario; // Pasar el nombre del propietario a la vista

            return PartialView("_EditInmueblePartial", tbInmueble);
        }

        // POST: TbInmuebles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FidInmueble,FkidPropietario,Fdescripcion,Fdireccion,Fubicacion,Fprecio")] TbInmueble tbInmueble)
        {
            if (id != tbInmueble.FidInmueble)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var inmueble = await _context.TbInmuebles.FindAsync(id);
                    if (inmueble == null)
                    {
                        return NotFound();
                    }

                    tbInmueble.Factivo = true;
                    _context.Entry(inmueble).CurrentValues.SetValues(tbInmueble);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TbInmuebleExists(tbInmueble.FidInmueble))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                var inmuebles = await _context.TbInmuebles.ToListAsync();
                var propietarios = await _context.TbPropietarios
                    .Where(p => p.Factivo)
                    .ToDictionaryAsync(p => p.FidPropietario, p => p.Fnombre);

                var inmuebleViewModels = inmuebles.Select(i => new InmuebleViewModel
                {
                    FidInmueble = i.FidInmueble,
                    Fdescripcion = i.Fdescripcion,
                    Fdireccion = i.Fdireccion,
                    Fubicacion = i.Fubicacion,
                    Fprecio = i.Fprecio,
                    Factivo = i.Factivo,
                    PropietarioNombre = propietarios.ContainsKey(i.FkidPropietario) ? propietarios[i.FkidPropietario] : "Desconocido"
                }).ToList();

                return PartialView("_InmueblesPartial", inmuebleViewModels);
            }

            return PartialView("_EditInmueblePartial", tbInmueble);
        }

        // GET: TbInmuebles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tbInmueble = await _context.TbInmuebles.FirstOrDefaultAsync(m => m.FidInmueble == id);
            if (tbInmueble == null)
            {
                return NotFound();
            }

            return View(tbInmueble);
        }

        // POST: TbInmuebles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tbInmueble = await _context.TbInmuebles.FindAsync(id);
            if (tbInmueble != null)
            {
                _context.TbInmuebles.Remove(tbInmueble);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TbInmuebleExists(int id)
        {
            return _context.TbInmuebles.Any(e => e.FidInmueble == id);
        }

        // GET: TbInmuebles/BuscarPropietario/
        [HttpGet]
        public async Task<IActionResult> BuscarPropietario(string searchTerm = null)
        {
            //  DbSet de Propietarios en el contexto
            var propietariosQuery = _context.TbPropietarios.AsQueryable();
            

            // Filtrar propietarios si hay un término de búsqueda
            if (!string.IsNullOrEmpty(searchTerm))
            {
                propietariosQuery = propietariosQuery.Where(i => i.Fnombre.Contains(searchTerm) || i.Fapellidos.Contains(searchTerm));
            }

            // Obtener resultados de propietarios
            var propietariosResultados = await propietariosQuery
                .Select(i => new
                {
                    id = i.FidPropietario,
                    text = $"{i.Fnombre} {i.Fapellidos}",
                    tipo = "propietario" // Añadir un campo para identificar el tipo
                })
                .ToListAsync();

            // Combinar resultados
            var resultados = propietariosResultados.ToList();

            return Json(new { results = resultados });
        }
    }
}