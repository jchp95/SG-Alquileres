using Alquileres.Context;
using Alquileres.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Alquileres.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GetCompFiscalesApi : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GetCompFiscalesApi(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("comprobantes-fiscales")]
        public async Task<IActionResult> ObtenerComprobantesFiscales()
        {
            try
            {
                // Obtener todos los comprobantes fiscales activos
                var comprobantes = await _context.TbComprobantesFiscales
                    .Select(cf => new
                    {
                        idComprobante = cf.FidComprobante,
                        tipoComprobante = cf.FtipoComprobante,
                        prefijo = cf.Fprefijo,
                        numeroInicial = cf.Finicia,
                        numeroFinal = cf.Ffinaliza,
                        contadorActual = cf.Fcontador,
                        comprobante = cf.Fcomprobante,
                        fechaVencimiento = cf.Fvence,
                    })
                    .OrderBy(cf => cf.tipoComprobante) // Ordenar por tipo
                    .ToListAsync();

                if (!comprobantes.Any())
                {
                    return Ok(new
                    {
                        success = false,
                        message = "No se encontraron comprobantes fiscales activos",
                        data = new List<object>()
                    });
                }

                // Estructura de respuesta estándar
                return Ok(new
                {
                    success = true,
                    message = "Comprobantes fiscales obtenidos correctamente",
                    data = comprobantes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno al procesar la solicitud",
                    error = ex.Message
                });
            }
        }
    }
}