using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging; // Asegúrate de incluir esto
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Alquileres.Models;
using Microsoft.EntityFrameworkCore;
using Alquileres.Context;

public class GeneradorDeCuotasService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GeneradorDeCuotasService> _logger; // Agregar logger

    public GeneradorDeCuotasService(IServiceProvider serviceProvider, ILogger<GeneradorDeCuotasService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger; // Inicializar logger
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var hoy = DateTime.Today;

        // Verificar cuotas vencidas al inicio (solo las que tienen estado 'N' y están vencidas)
        var cuotasVencidas = await context.TbCxcCuota
            .Where(c => c.Factivo && c.Fvence <= hoy && c.Fstatus == 'N') // Solo cuotas con estado 'N'
            .ToListAsync(stoppingToken);

        if (cuotasVencidas.Any())
        {
            // Registrar alerta si hay cuotas vencidas
            _logger.LogWarning("Existen cuotas vencidas. Por favor, revise las cuentas correspondientes.");
            Console.WriteLine("Existen cuotas vencidas. Por favor, revise las cuentas correspondientes.");

            // Actualizar el estado de las cuotas vencidas a "V"
            foreach (var cuota in cuotasVencidas)
            {
                cuota.Fstatus = 'V'; // Cambiar el estado a "V"
                context.TbCxcCuota.Update(cuota);
                _logger.LogInformation($"Cuota actualizada: {cuota.FNumeroCuota}, Estado: {cuota.Fstatus}");
                Console.WriteLine($"Cuota actualizada: {cuota.FNumeroCuota}, Estado: {cuota.Fstatus}");
            }

            // Guardar cambios para actualizar el estado de las cuotas vencidas
            await context.SaveChangesAsync(stoppingToken);
        }

        // Generar nuevas cuotas si es necesario
        foreach (var cuota in cuotasVencidas)
        {
            var existeSiguiente = await context.TbCxcCuota.AnyAsync(q =>
                q.FidCxc == cuota.FidCxc && q.FNumeroCuota == cuota.FNumeroCuota + 1, stoppingToken);

            if (existeSiguiente) continue;

            var cuenta = await context.TbCxcs.FindAsync(cuota.FidCxc);
            if (cuenta == null) continue;

            var periodo = await context.PeriodosPagos.FindAsync(cuenta.FidPeriodoPago);
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
                Fstatus = 'N' // Establecer el estado en "N" (normal)
            };

            _logger.LogInformation($"Creando nueva cuota: {nuevaCuota.FNumeroCuota}, Estado: {nuevaCuota.Fstatus}");
            Console.WriteLine($"Creando nueva cuota: {nuevaCuota.FNumeroCuota}, Estado: {nuevaCuota.Fstatus}");

            context.TbCxcCuota.Add(nuevaCuota);

            // Actualizar la fecha de próxima cuota
            cuenta.FfechaProxCuota = nuevaFechaVencimiento;
            context.TbCxcs.Update(cuenta);
        }

        await context.SaveChangesAsync(stoppingToken);
    }
}