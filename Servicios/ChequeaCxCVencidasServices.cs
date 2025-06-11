using Microsoft.EntityFrameworkCore;
using Alquileres.Models;
using Alquileres.Context;
using Microsoft.Extensions.Logging;

public class ChequeaCxCVencidasServices
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ChequeaCxCVencidasServices> _logger;

    public ChequeaCxCVencidasServices(ApplicationDbContext context, ILogger<ChequeaCxCVencidasServices> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task VerificarCxCVencidasAsync()
    {
        try
        {
            // 1. Buscar CxC que tienen al menos una cuota vencida y no están marcadas como vencidas
            var cxcConCuotasVencidas = await _context.TbCxcs
                .Where(c => c.Factivo &&
                       c.Fstatus != 'V' && // Que no estén ya marcadas como vencidas
                       _context.TbCxcCuota.Any(
                           q => q.FidCxc == c.FidCuenta &&
                           q.Fstatus == 'V' &&
                           q.Factivo))
                .ToListAsync();

            if (!cxcConCuotasVencidas.Any())
            {
                _logger.LogInformation("No se encontraron CxC con cuotas vencidas pendientes de actualizar.");
                return;
            }

            _logger.LogInformation($"Marcando {cxcConCuotasVencidas.Count} CxC como vencidas.");

            // 2. Actualizar el estado de las CxC a "A"
            foreach (var cxc in cxcConCuotasVencidas)
            {
                cxc.Fstatus = 'A';
                _context.TbCxcs.Update(cxc);
                _logger.LogInformation($"CxC actualizada - ID: {cxc.FidCuenta}, Nuevo Estado: {cxc.Fstatus}");
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Proceso de actualización de CxC vencidas completado.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar CxC vencidas");
            throw;
        }
    }
}