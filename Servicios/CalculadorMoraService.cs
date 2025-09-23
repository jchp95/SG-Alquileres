using Microsoft.EntityFrameworkCore;
using Alquileres.Models;
using Alquileres.Context;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

public class ActualizadorMoraService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ActualizadorMoraService> _logger;

    public ActualizadorMoraService(ApplicationDbContext context, ILogger<ActualizadorMoraService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task ActualizarMorasAsync()
    {
        try
        {
            var hoy = DateTime.Now;
            _logger.LogInformation($"Iniciando proceso de actualización de moras a las {hoy}");

            // Paso 1: Obtener cuotas vencidas con estado 'V' (Vencidas) que cumplen con los criterios
            var cuotasParaActualizar = await _context.TbCxcCuota
                .Where(c => c.Factivo &&
                           c.Fstatus == 'V' &&
                           c.FfechaUltCalculo < hoy &&
                           _context.TbCxcs.Any(cxc => cxc.FidCuenta == c.FkidCxc &&
                                                     cxc.Factivo &&
                                                     cxc.Fstatus != 'S'))
                .ToListAsync();

            if (!cuotasParaActualizar.Any())
            {
                _logger.LogInformation("No se encontraron cuotas para actualizar mora.");
                return;
            }

            _logger.LogInformation($"Encontradas {cuotasParaActualizar.Count} cuotas para actualizar mora.");

            // Paso 2: Procesar cada cuota
            foreach (var cuota in cuotasParaActualizar)
            {
                // Obtener la cuenta por cobrar relacionada
                var cuentaCxc = await _context.TbCxcs
                    .FirstOrDefaultAsync(c => c.FidCuenta == cuota.FkidCxc);

                if (cuentaCxc == null)
                {
                    _logger.LogWarning($"No se encontró la cuenta por cobrar para la cuota {cuota.FidCuota}. Se omite.");
                    continue;
                }

                // Verificar si ya pasó el período de gracia
                var fechaConGracia = cuota.Fvence.AddDays(cuentaCxc.FdiasGracia);
                if (fechaConGracia > hoy)
                {
                    _logger.LogInformation($"Cuota {cuota.FidCuota} está en período de gracia ({cuentaCxc.FdiasGracia} días). No se aplica mora.");
                    continue;
                }

                // Calcular días de atraso desde el último cálculo
                var diasAtraso = (hoy - cuota.FfechaUltCalculo).Days;
                if (diasAtraso <= 0)
                {
                    _logger.LogInformation($"No hay días de atraso para la cuota {cuota.FidCuota}. Se omite.");
                    continue;
                }

                // 1. Actualizar la mora (usando CEILING como en la lógica SQL)
                decimal incrementoMora = (cuota.Fsaldo / 100m * cuentaCxc.FtasaMora / 30m) * diasAtraso;
                cuota.Fmora += Math.Ceiling(incrementoMora);

                // 2. Actualizar días de atraso
                cuota.FdiasAtraso += diasAtraso;

                // 3. Actualizar fecha de último cálculo
                cuota.FfechaUltCalculo = hoy;

                _logger.LogInformation($"Actualizando cuota {cuota.FidCuota}: " +
                    $"Mora incrementada en {incrementoMora}, " +
                    $"Días atraso incrementados en {diasAtraso}, " +
                    $"Nueva mora total: {cuota.Fmora}, " +
                    $"Nuevos días atraso: {cuota.FdiasAtraso}");

                _context.TbCxcCuota.Update(cuota);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Actualización de moras completada exitosamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en el servicio de actualización de moras");
            throw;
        }
    }
}