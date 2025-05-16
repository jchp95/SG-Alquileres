using Alquileres.Context;
using Alquileres.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Alquileres.Controllers
{
    [Authorize]
    public class TbCuentasPorCobrarController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TbCuentasPorCobrarController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager; // Asignación del UserManager
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var cxC = await _context.TbCxcs.FindAsync(id);
            if (cxC == null)
            {
                return NotFound();
            }

            // Cambiar el estado del inmueble
            cxC.Factivo = !cxC.Factivo;

            _context.Update(cxC);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Carga vista principal
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CargarCuentasPorCobrar()
        {
            try
            {
                var cuentas = await _context.TbCxcs.ToListAsync();
                var inquilinos = await _context.TbInquilinos.ToListAsync();
                var periodos = await _context.PeriodosPagos.ToListAsync();

                var cuentasPorCobrarViewModels = new List<CuentaPorCobrarViewModel>();

                foreach (var c in cuentas)
                {
                    // Obtener la última cuota de la cuenta
                    var ultimaCuota = await _context.TbCxcCuota
                        .Where(q => q.FidCxc == c.FidCuenta)
                        .OrderByDescending(q => q.Fvence)
                        .FirstOrDefaultAsync();

                    // Calcular la fecha de la próxima cuota
                    DateTime? fechaProxCuota = null;
                    if (ultimaCuota != null)
                    {
                        fechaProxCuota = CalcularFechaProxCuota(ultimaCuota.Fvence, c.FidPeriodoPago);
                    }

                    cuentasPorCobrarViewModels.Add(new CuentaPorCobrarViewModel
                    {
                        FidCuenta = c.FidCuenta,
                        FidInquilino = c.FidInquilino,
                        InquilinoNombre = c.FidInquilino.HasValue
                            ? inquilinos.FirstOrDefault(i => i.FidInquilino == c.FidInquilino)?.Fnombre + " " +
                              inquilinos.FirstOrDefault(i => i.FidInquilino == c.FidInquilino)?.Fapellidos
                            : "Desconocido",
                        FidInmueble = c.FkidInmueble,
                        Fmonto = c.Fmonto,
                        FfechaInicio = c.FfechaInicio,
                        FdiasGracia = c.FdiasGracia,
                        Factivo = c.Factivo,
                        FtasaMora = c.FtasaMora,
                        Fnota = c.Fnota,
                        FidPeriodoPago = c.FidPeriodoPago,
                        NombrePeriodoPago = periodos.FirstOrDefault(p => p.Id == c.FidPeriodoPago)?.Nombre ?? "Desconocido",
                        FfechaProxCuota = fechaProxCuota // Asignar la fecha de la próxima cuota calculada
                    });
                }

                return PartialView("_CuentasPorCobrarPartial", cuentasPorCobrarViewModels);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Método para calcular la fecha de la próxima cuota
        private DateTime? CalcularFechaProxCuota(DateTime fechaVencimiento, int? periodoPagoId)
        {
            if (!periodoPagoId.HasValue)
                return null;

            int dias = 0;

            // Determinar el número de días según el periodo de pago
            switch (periodoPagoId.Value)
            {
                case 1: // Semanal
                    dias = 7;
                    break;
                case 2: // Quincenal
                    dias = 15;
                    break;
                case 3: // Mensual
                    dias = 30;
                    break;
                default:
                    throw new ArgumentException("Periodo de pago no válido");
            }

            // Calcular y devolver la fecha de la próxima cuota
            return fechaVencimiento.AddDays(dias);
        }

        // GET: Cargar formulario de creación
        public async Task<IActionResult> Create()
        {
            // Cargar los periodos de pago desde la base de datos
            var periodosPagos = await _context.PeriodosPagos.ToListAsync();

            // Crear una lista de opciones que incluya la opción predeterminada
            var opciones = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Seleccionar método de pago" } // Opción predeterminada
            };

            // Agregar las opciones de periodos de pago
            opciones.AddRange(periodosPagos.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Nombre
            }));

            // Crear el SelectList para el ViewBag
            ViewBag.FidPeriodoPago = opciones;

            // Retornar la vista parcial
            return PartialView("_CreateCuentasPorCobrarPartial");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FidInquilino,FkidInmueble,FfechaInicio,Fmonto,FdiasGracia,FtasaMora,Fnota,FidPeriodoPago")] TbCxc tbCxc)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            try
            {
                var identityId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(identityId))
                    return BadRequest("El usuario no está autenticado.");

                var usuario = await _context.TbUsuarios.FirstOrDefaultAsync(u => u.IdentityId == identityId);
                if (usuario == null)
                    return BadRequest($"No se encontró un usuario con el IdentityId: {identityId}");

                tbCxc.FkidUsuario = usuario.FidUsuario;
                tbCxc.Factivo = true;

                _context.Add(tbCxc);
                await _context.SaveChangesAsync();

                var numeroCuotaMaxima = await _context.TbCxcCuota
                    .Where(c => c.FidCxc == tbCxc.FidCuenta)
                    .MaxAsync(c => (int?)c.FNumeroCuota) ?? 0;

                var fechaVencimiento = CalcularFechaVencimiento(tbCxc.FfechaInicio, tbCxc.FidPeriodoPago);

                var cuota = new TbCxcCuotum
                {
                    FidCxc = tbCxc.FidCuenta,
                    FNumeroCuota = numeroCuotaMaxima + 1,
                    Fvence = fechaVencimiento,
                    Fmonto = (int)tbCxc.Fmonto,
                    Fsaldo = tbCxc.Fmonto,
                    Fmora = tbCxc.FtasaMora,
                    FfechaUltCalculo = fechaVencimiento,
                    Factivo = true,
                    Fstatus = 'N'
                };

                _context.TbCxcCuota.Add(cuota);
                await _context.SaveChangesAsync();

                var cuentasPorCobrar = await _context.TbCxcs.ToListAsync();
                var cuentasPorCobrarViewModels = cuentasPorCobrar.Select(c => new CuentaPorCobrarViewModel
                {
                    FidCuenta = c.FidCuenta,
                    FidInquilino = c.FidInquilino,
                    FidInmueble = c.FkidInmueble,
                    Fmonto = c.Fmonto,
                    FfechaInicio = c.FfechaInicio,
                    FdiasGracia = c.FdiasGracia,
                    Factivo = c.Factivo,
                    FtasaMora = c.FtasaMora,
                    Fnota = c.Fnota,
                    FidPeriodoPago = c.FidPeriodoPago
                }).ToList();

                return PartialView("_CuentasPorCobrarPartial", cuentasPorCobrarViewModels);
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(new { success = false, message = argEx.Message });
            }
            catch (DbUpdateException dbEx)
            {
                // Logear si tienes sistema de logs
                return StatusCode(500, new { success = false, message = "Error al guardar en base de datos.", detail = dbEx.Message });
            }
            catch (Exception ex)
            {
                // Logear también aquí
                return StatusCode(500, new { success = false, message = "Ocurrió un error inesperado.", detail = ex.Message });
            }
        }



        // Método para calcular la fecha de vencimiento
        private DateTime CalcularFechaVencimiento(DateTime fechaInicio, int periodoPagoId)
        {
            int dias = 0;

            // Determinar el número de días según el periodo de pago
            switch (periodoPagoId)
            {
                case 1: // Suponiendo que 1 es semanal
                    dias = 7;
                    break;
                case 2: // Suponiendo que 2 es quincenal
                    dias = 15;
                    break;
                case 3: // Suponiendo que 3 es mensual
                    dias = 30;
                    break;
                default:
                    throw new ArgumentException("Periodo de pago no válido");
            }

            // Calcular y devolver la fecha de vencimiento
            return fechaInicio.AddDays(dias);
        }



        [HttpGet]
        public async Task<IActionResult> BuscarInquilinosOInmuebles(string searchTerm = null)
        {
            // Suponiendo que tienes un DbSet de Inquilinos y de Inmuebles en tu contexto
            var inquilinosQuery = _context.TbInquilinos.AsQueryable();
            var inmueblesQuery = _context.TbInmuebles.AsQueryable(); // No necesitas incluir el propietario aquí

            // Filtrar inquilinos si hay un término de búsqueda
            if (!string.IsNullOrEmpty(searchTerm))
            {
                inquilinosQuery = inquilinosQuery.Where(i => i.Fnombre.Contains(searchTerm) || i.Fapellidos.Contains(searchTerm));
            }

            // Filtrar inmuebles si hay un término de búsqueda
            if (!string.IsNullOrEmpty(searchTerm))
            {
                inmueblesQuery = inmueblesQuery.Where(m => m.Fdescripcion.Contains(searchTerm) || m.Fdireccion.Contains(searchTerm) || m.Fubicacion.Contains(searchTerm));
            }

            // Obtener resultados de inquilinos
            var inquilinosResultados = await inquilinosQuery
                .Select(i => new
                {
                    id = i.FidInquilino,
                    text = $"{i.Fnombre} {i.Fapellidos}",
                    tipo = "inquilino" // Añadir un campo para identificar el tipo
                })
                .ToListAsync();

            // Obtener resultados de inmuebles
            var inmueblesResultados = await inmueblesQuery
                .Select(m => new
                {
                    m.FidInmueble, // Mantener el ID del inmueble
                    m.Fdescripcion, // Mantener la descripción
                    m.Fdireccion, // Mantener la dirección
                    m.Fubicacion, // Mantener la ubicación
                    m.FkidPropietario, // Mantener el ID del propietario
                    tipo = "inmueble" // Añadir un campo para identificar el tipo
                })
                .ToListAsync();

            // Obtener los propietarios asociados a los inmuebles
            var propietarios = await _context.TbPropietarios
                .Where(p => inmueblesResultados.Select(m => m.FkidPropietario).Contains(p.FidPropietario))
                .ToListAsync();

            // Combinar resultados de inmuebles con los nombres de los propietarios
            var inmueblesConPropietarios = inmueblesResultados.Select(m => new
            {
                id = m.FidInmueble,
                text = $"{m.Fdireccion} - Ubicación: {m.Fubicacion}",
                tipo = m.tipo
            }).ToList();

            // Combinar resultados
            var resultados = inquilinosResultados.Concat(inmueblesConPropietarios).ToList();

            return Json(new { results = resultados });
        }
    }
}