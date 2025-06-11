using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Alquileres.Models;
using Microsoft.EntityFrameworkCore;
using Alquileres.Context;

public class ActualizadorMoraService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ActualizadorMoraService> _logger;

    public ActualizadorMoraService(IServiceProvider serviceProvider, ILogger<ActualizadorMoraService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var hoy = DateTime.Today;

                // Paso 1: Obtener cuotas vencidas con estado 'V' (Vencidas)
                var cuotasVencidas = await context.TbCxcCuota
                    .AsNoTracking()
                    .Where(c => c.Factivo && c.Fstatus == 'V' && c.Fvence < hoy)
                    .ToListAsync(stoppingToken);

                if (cuotasVencidas.Any())
                {
                    _logger.LogInformation($"Encontradas {cuotasVencidas.Count} cuotas vencidas para actualizar mora.");

                    // Paso 2: Obtener las cuentas por cobrar relacionadas usando JOIN
                    var cuentasCxc = await (from cuota in context.TbCxcCuota
                                            join cuenta in context.TbCxcs on cuota.FidCxc equals cuenta.FidCuenta
                                            where cuota.Factivo && cuota.Fstatus == 'V' && cuota.Fvence < hoy
                                            select new
                                            {
                                                cuenta.FidCuenta,
                                                cuenta.FdiasGracia,
                                                cuenta.FtasaMora
                                            })
                                             .Distinct()
                                             .ToListAsync(stoppingToken);

                    // Paso 3: Procesar cada cuota
                    foreach (var cuota in cuotasVencidas)
                    {
                        var cuentaCxc = cuentasCxc.FirstOrDefault(c => c.FidCuenta == cuota.FidCxc);
                        if (cuentaCxc == null)
                        {
                            _logger.LogWarning($"No se encontró la cuenta por cobrar para la cuota {cuota.FidCuota}. Se omite.");
                            continue;
                        }

                        // Calcular días de atraso
                        var fechaReferencia = cuota.FfechaUltCalculo > cuota.Fvence ?
                            cuota.FfechaUltCalculo : cuota.Fvence;

                        var diasAtraso = (hoy - fechaReferencia).Days;

                        // Aplicar días de gracia
                        if (cuentaCxc.FdiasGracia > 0 && diasAtraso <= cuentaCxc.FdiasGracia)
                        {
                            _logger.LogInformation($"Cuota {cuota.FidCuota} está en período de gracia ({cuentaCxc.FdiasGracia} días). No se aplica mora.");
                            continue;
                        }

                        // Calcular días de atraso después de gracia
                        diasAtraso = Math.Max(0, diasAtraso - cuentaCxc.FdiasGracia);

                        if (diasAtraso > 0)
                        {
                            // Calcular mora según fórmula
                            decimal incrementoMora = (cuota.Fsaldo * (cuentaCxc.FtasaMora / 100m) / 30m) * diasAtraso;

                            _logger.LogInformation($"Aplicando mora a cuota {cuota.FidCuota}: " +
                                $"Saldo: {cuota.Fsaldo}, Tasa: {cuentaCxc.FtasaMora}%, " +
                                $"Días atraso: {diasAtraso}, Incremento: {incrementoMora}");

                            // Actualizar saldo 
                            cuota.Fsaldo += incrementoMora;
                            cuota.FfechaUltCalculo = hoy;

                            context.TbCxcCuota.Update(cuota);
                        }
                    }

                    await context.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Actualización de moras completada.");
                }

                // Esperar hasta el próximo día para volver a ejecutar
                var tiempoParaMedianoche = DateTime.Today.AddDays(1) - DateTime.Now;
                await Task.Delay(tiempoParaMedianoche, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el servicio de actualización de moras");
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
