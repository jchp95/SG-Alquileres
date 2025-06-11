using Microsoft.EntityFrameworkCore;
using Alquileres.Models;
using Alquileres.Context;
using Microsoft.Extensions.Logging;

public class GeneradorDeCuotasService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GeneradorDeCuotasService> _logger;

    public GeneradorDeCuotasService(ApplicationDbContext context, ILogger<GeneradorDeCuotasService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task VerificarCuotasVencidasAsync()
    {
        var hoy = DateTime.Today;

        // Verificar cuotas vencidas
        var cuotasVencidas = await _context.TbCxcCuota
            .Where(c => c.Factivo && c.Fvence <= hoy && c.Fstatus == 'N')
            .ToListAsync();

        if (cuotasVencidas.Any())
        {
            _logger.LogWarning("Existen cuotas vencidas. Por favor, revise las cuentas correspondientes.");

            // Actualizar el estado de las cuotas vencidas a "V"
            foreach (var cuota in cuotasVencidas)
            {
                cuota.Fstatus = 'V';
                _context.TbCxcCuota.Update(cuota);
                _logger.LogInformation($"Cuota actualizada: {cuota.FNumeroCuota}, Estado: {cuota.Fstatus}");
            }

            await _context.SaveChangesAsync();

            // Generar nuevas cuotas si es necesario
            foreach (var cuota in cuotasVencidas)
            {
                var existeSiguiente = await _context.TbCxcCuota.AnyAsync(q =>
                    q.FidCxc == cuota.FidCxc && q.FNumeroCuota == cuota.FNumeroCuota + 1);

                if (existeSiguiente) continue;

                var cuenta = await _context.TbCxcs.FindAsync(cuota.FidCxc);
                if (cuenta == null) continue;

                var periodo = await _context.PeriodosPagos.FindAsync(cuenta.FidPeriodoPago);
                if (periodo == null) continue;

                var nuevaFechaVencimiento = cuota.Fvence.AddDays(periodo.Dias);

                var nuevaCuota = new TbCxcCuotum
                {
                    FidCxc = cuenta.FidCuenta,
                    FNumeroCuota = cuota.FNumeroCuota + 1,
                    Fvence = nuevaFechaVencimiento,
                    Fmonto = (int)cuenta.Fmonto,
                    Fsaldo = cuenta.Fmonto,
                    Fmora = cuenta.FtasaMora,
                    FfechaUltCalculo = nuevaFechaVencimiento,
                    Factivo = true,
                    Fstatus = 'N'
                };

                _logger.LogInformation($"Creando nueva cuota: {nuevaCuota.FNumeroCuota}, Estado: {nuevaCuota.Fstatus}");

                _context.TbCxcCuota.Add(nuevaCuota);

                // Actualizar la fecha de próxima cuota
                cuenta.FfechaProxCuota = nuevaFechaVencimiento;
                _context.TbCxcs.Update(cuenta);
            }

            await _context.SaveChangesAsync();
        }
    }
}