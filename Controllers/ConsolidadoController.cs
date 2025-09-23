using Alquileres.Context;
using Alquileres.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Alquileres.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ConsolidadoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ConsolidadoController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{idInquilino}")]
        public async Task<IActionResult> GetConsolidado(int idInquilino)
        {
            try
            {
                // 1. Verificar que el inquilino existe
                var inquilino = await _context.TbInquilinos
                    .FirstOrDefaultAsync(i => i.FidInquilino == idInquilino);

                if (inquilino == null)
                {
                    return NotFound(new { success = false, message = "Inquilino no encontrado" });
                }

                // 2. Obtener todas las CXC activas del inquilino con JOIN para periodo de pago
                var cuentasPorCobrarQuery =
                    from cxc in _context.TbCxcs
                    join periodo in _context.PeriodosPagos on cxc.FkidPeriodoPago equals periodo.FidPeriodoPago
                    where cxc.FkidInquilino == idInquilino && cxc.Factivo == true
                    select new
                    {
                        Cxc = cxc,
                        PeriodoPago = periodo
                    };

                var cuentasPorCobrar = await cuentasPorCobrarQuery.ToListAsync();

                // 3. Obtener todos los IDs de inmuebles para hacer una sola consulta
                var inmuebleIds = cuentasPorCobrar.Select(x => x.Cxc.FkidInmueble).Distinct().ToList();

                var inmuebles = new Dictionary<int, TbInmueble>();
                foreach (var id in inmuebleIds)
                {
                    var inmueble = await _context.TbInmuebles.FirstOrDefaultAsync(i => i.FidInmueble == id);
                    if (inmueble != null)
                    {
                        inmuebles[inmueble.FidInmueble] = inmueble;
                    }
                }

                // 4. Obtener todas las cuotas relacionadas usando JOIN
                var cuotasQuery = from cuota in _context.TbCxcCuota
                                  join cxc in _context.TbCxcs on cuota.FkidCxc equals cxc.FidCuenta
                                  where cxc.FkidInquilino == idInquilino
                                  select cuota;

                var cuotas = await cuotasQuery.ToListAsync();
                var cuotasPorCxc = cuotas.GroupBy(c => c.FkidCxc)
                    .ToDictionary(g => g.Key, g => g.OrderBy(c => c.FNumeroCuota).ToList());

                // 5. Obtener todos los cobros relacionados usando JOIN
                var cobrosQuery = from cobro in _context.TbCobros
                                  join cxc in _context.TbCxcs on cobro.FkidCxc equals cxc.FidCuenta
                                  where cxc.FkidInquilino == idInquilino
                                  select cobro;

                var cobros = await cobrosQuery.ToListAsync();
                var cobrosPorCxc = cobros.GroupBy(c => c.FkidCxc)
                    .ToDictionary(g => g.Key, g => g.OrderBy(c => c.Ffecha).ToList());

                // 6. Obtener todas las anulaciones relacionadas usando JOIN
                var anulacionesQuery = from anulacion in _context.TbCxcNulos
                                       join cxc in _context.TbCxcs on anulacion.FkidCuenta equals cxc.FidCuenta
                                       where cxc.FkidInquilino == idInquilino
                                       select anulacion;

                var anulaciones = await anulacionesQuery.ToListAsync();
                var anulacionPorCxc = anulaciones.ToDictionary(a => a.FkidCuenta);

                // Construir la respuesta
                var resultado = new
                {
                    Inquilino = new
                    {
                        id = inquilino.FidInquilino,
                        nombre = inquilino.Fnombre,
                        apellidos = inquilino.Fapellidos,
                        cedula = inquilino.Fcedula,
                        direccion = inquilino.Fdireccion,
                        telefono = inquilino.Ftelefono,
                        celular = inquilino.Fcelular
                    },
                    CuentasPorCobrar = cuentasPorCobrar.Select(cxcData =>
                    {
                        var cxc = cxcData.Cxc;
                        var inmuebleKey = cxc.FkidInmueble.GetValueOrDefault();
                        var inmueble = inmuebles.TryGetValue(inmuebleKey, out var i) ? i : null;

                        var cuotasCxc = cuotasPorCxc.TryGetValue(cxc.FidCuenta, out var c) ? c : new List<TbCxcCuotum>();
                        var cobrosCxc = cobrosPorCxc.TryGetValue(cxc.FidCuenta, out var cb) ? cb : new List<TbCobro>();
                        var anulacionCxc = anulacionPorCxc.TryGetValue(cxc.FidCuenta, out var a) ? a : null;

                        var totalPagado = cobrosCxc.Sum(c => c.Fmonto);
                        var saldoPendiente = cxc.Fmonto - totalPagado;
                        var cuotasVencidas = cuotasCxc.Count(c => c.Fstatus == 'V');
                        var cuotasPendientes = cuotasCxc.Count(c => c.Fstatus == 'N');

                        return new
                        {
                            id = cxc.FidCuenta,
                            montoTotal = cxc.Fmonto,
                            diasGracia = cxc.FdiasGracia,
                            tasaMora = cxc.FtasaMora,
                            estado = cxc.Fstatus.ToString(),
                            fechaInicio = cxc.FfechaInicio,
                            fechaProxCuota = cxc.FfechaProxCuota,
                            nota = cxc.Fnota,
                            periodoPago = new
                            {
                                id = cxc.FkidPeriodoPago,
                                nombre = cxcData.PeriodoPago.Fnombre,
                                dias = cxcData.PeriodoPago.Fdias
                            },
                            inmueble = inmueble != null ? new
                            {
                                id = inmueble.FidInmueble,
                                descripcion = inmueble.Fdescripcion,
                                direccion = inmueble.Fdireccion,
                                ubicacion = inmueble.Fubicacion,
                                precio = inmueble.Fprecio
                            } : null,
                            cuotas = cuotasCxc.Select(c => new
                            {
                                id = c.FidCuota,
                                numeroCuota = c.FNumeroCuota,
                                fechaVencimiento = c.Fvence,
                                monto = c.Fmonto,
                                saldo = c.Fsaldo,
                                mora = c.Fmora,
                                estado = c.Fstatus.ToString(),
                                fechaUltCalculo = c.FfechaUltCalculo,
                                activo = c.Factivo
                            }),
                            cobros = cobrosCxc.Select(c => new
                            {
                                id = c.FidCobro,
                                fecha = c.Ffecha,
                                monto = c.Fmonto,
                                estado = c.Factivo
                            }),
                            anulacion = anulacionCxc != null ? new
                            {
                                fechaAnulacion = anulacionCxc.FfechaAnulacion,
                                motivo = anulacionCxc.FmotivoAnulacion,
                                usuario = anulacionCxc.FkidUsuario
                            } : null,
                            totales = new
                            {
                                pagado = totalPagado,
                                pendiente = saldoPendiente,
                                cuotasVencidas = cuotasVencidas,
                                cuotasPendientes = cuotasPendientes
                            }
                        };
                    }).ToList()
                };

                return Ok(new { success = true, data = resultado });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener datos consolidados",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("resumen/{idInquilino}")]
        public async Task<IActionResult> GetResumen(int idInquilino)
        {
            try
            {
                // Obtener todas las CXC activas del inquilino usando JOIN
                var cxcQuery = from cxc in _context.TbCxcs
                               where cxc.FkidInquilino == idInquilino && cxc.Factivo == true
                               select cxc;

                var cuentasCxC = await cxcQuery.ToListAsync();

                if (!cuentasCxC.Any())
                {
                    return Ok(new
                    {
                        success = true,
                        data = new
                        {
                            totalCuentas = 0,
                            totalMonto = 0,
                            totalPagado = 0,
                            cuotasVencidas = 0,
                            cuotasPendientes = 0
                        }
                    });
                }

                // Calcular totales de CXC
                var totalCuentas = cuentasCxC.Count;
                var totalMonto = cuentasCxC.Sum(c => c.Fmonto);

                // Calcular totales de cobros usando JOIN
                var cobrosQuery = from cobro in _context.TbCobros
                                  join cxc in cxcQuery on cobro.FkidCxc equals cxc.FidCuenta
                                  select cobro;

                var totalPagado = await cobrosQuery.SumAsync(c => c.Fmonto);

                // Calcular cuotas vencidas y pendientes usando JOIN
                var cuotasQuery = from cuota in _context.TbCxcCuota
                                  join cxc in cxcQuery on cuota.FkidCxc equals cxc.FidCuenta
                                  select cuota;

                var cuotas = await cuotasQuery.ToListAsync();
                var cuotasVencidas = cuotas.Count(c => c.Fstatus == 'V');
                var cuotasPendientes = cuotas.Count(c => c.Fstatus == 'N');

                var resumen = new
                {
                    totalCuentas = totalCuentas,
                    totalMonto = totalMonto,
                    totalPagado = totalPagado,
                    cuotasVencidas = cuotasVencidas,
                    cuotasPendientes = cuotasPendientes
                };

                return Ok(new { success = true, data = resumen });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener resumen",
                    error = ex.Message
                });
            }
        }

        [HttpGet("lista-inquilinos")]
        public async Task<IActionResult> GetListInquilino()
        {
            var inquilinos = await _context.TbInquilinos
                                          .OrderBy(i => i.FidInquilino)
                                          .Select(i => new
                                          {
                                              inquilino = i.FidInquilino,
                                              nombre = i.Fnombre,
                                              apellidos = i.Fapellidos,
                                              cedula = i.Fcedula,
                                              direccion = i.Fdireccion,
                                              telefono = i.Ftelefono,
                                              celular = i.Fcelular,
                                              activo = i.Factivo
                                          })
                                          .ToListAsync();

            return Ok(new { success = true, data = inquilinos });
        }

        [HttpGet("cuentas-por-cobrar")]
        public async Task<IActionResult> GetAllCuentasPorCobrar()
        {
            try
            {
                // 1. Obtener todas las CXC activas con JOINs necesarios
                var cuentasQuery = from cxc in _context.TbCxcs
                                   join inquilino in _context.TbInquilinos on cxc.FkidInquilino equals inquilino.FidInquilino
                                   join inmueble in _context.TbInmuebles on cxc.FkidInmueble equals inmueble.FidInmueble into inmuebleJoin
                                   from inmueble in inmuebleJoin.DefaultIfEmpty()
                                   join periodo in _context.PeriodosPagos on cxc.FkidPeriodoPago equals periodo.FidPeriodoPago
                                   where cxc.Factivo == true
                                   select new
                                   {
                                       Cuenta = cxc,
                                       Inquilino = inquilino,
                                       Inmueble = inmueble,
                                       PeriodoPago = periodo
                                   };

                var cuentas = await cuentasQuery.ToListAsync();

                if (!cuentas.Any())
                {
                    return Ok(new
                    {
                        success = true,
                        data = new
                        {
                            resumen = new
                            {
                                totalDeuda = 0,
                                clientesConDeuda = 0,
                                cuotasVencidas = 0,
                                porcentajeCobrado = 0,
                                distribucionDeuda = new List<object>(),
                                cuentas = new List<object>()
                            }
                        }
                    });
                }

                // 2. Obtener todas las cuotas relacionadas usando JOIN
                var cuotasQuery = from cuota in _context.TbCxcCuota
                                  join cxc in _context.TbCxcs on cuota.FkidCxc equals cxc.FidCuenta
                                  where cxc.Factivo == true
                                  select cuota;

                var cuotas = await cuotasQuery.ToListAsync();
                var cuotasPorCxc = cuotas.GroupBy(c => c.FkidCxc)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // 3. Obtener todos los cobros relacionados usando JOIN
                var cobrosQuery = from cobro in _context.TbCobros
                                  join cxc in _context.TbCxcs on cobro.FkidCxc equals cxc.FidCuenta
                                  where cxc.Factivo == true
                                  select cobro;

                var cobros = await cobrosQuery.ToListAsync();
                var cobrosPorCxc = cobros.GroupBy(c => c.FkidCxc)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // 4. Calcular totales generales
                var totalDeuda = cuentas.Sum(c => c.Cuenta.Fmonto);
                var totalPagado = cobros.Sum(c => c.Fmonto);
                var porcentajeCobrado = totalDeuda > 0 ? Math.Min(100, (totalPagado / totalDeuda) * 100) : 0;
                var clientesConDeuda = cuentas.Select(c => c.Cuenta.FkidInquilino).Distinct().Count();
                var cuotasVencidas = cuotas.Count(c => c.Fstatus == 'V');

                // 5. Agrupar por cliente para distribución de deuda
                var deudaPorCliente = cuentas
                    .GroupBy(c => new { c.Inquilino.FidInquilino, c.Inquilino.Fnombre, c.Inquilino.Fapellidos })
                    .Select(g => new
                    {
                        clienteId = g.Key.FidInquilino,
                        nombre = $"{g.Key.Fnombre} {g.Key.Fapellidos}",
                        totalDeuda = g.Sum(x => x.Cuenta.Fmonto),
                        totalPagado = g.Sum(x => cobrosPorCxc.TryGetValue(x.Cuenta.FidCuenta, out var cb) ? cb.Sum(c => c.Fmonto) : 0),
                        cuentas = g.Count()
                    })
                    .OrderByDescending(x => x.totalDeuda)
                    .ToList();

                // 6. Preparar respuesta detallada de cuentas
                var cuentasDetalle = cuentas.Select(c =>
                {
                    var cuentaId = c.Cuenta.FidCuenta;
                    var cuotasCxc = cuotasPorCxc.TryGetValue(cuentaId, out var ct) ? ct : new List<TbCxcCuotum>();
                    var cobrosCxc = cobrosPorCxc.TryGetValue(cuentaId, out var cb) ? cb : new List<TbCobro>();

                    var totalPagado = cobrosCxc.Sum(x => x.Fmonto);
                    var saldoPendiente = c.Cuenta.Fmonto - totalPagado;
                    var cuotasVencidas = cuotasCxc.Count(x => x.Fstatus == 'V');

                    var cuotasList = cuotasCxc
                        .Select(x => new
                        {
                            id = x.FidCuota, // Asegúrate que la propiedad existe
                            numeroCuota = x.FNumeroCuota,
                            fechaVencimiento = x.Fvence,
                            monto = x.Fmonto,
                            saldo = x.Fsaldo,
                            mora = x.Fmora,
                            estado = x.Fstatus.ToString()
                        }).ToList();

                    return new
                    {
                        id = cuentaId,
                        montoTotal = c.Cuenta.Fmonto,
                        saldoPendiente = saldoPendiente,
                        cuotasVencidas = cuotasVencidas,
                        fechaInicio = c.Cuenta.FfechaInicio,
                        estado = c.Cuenta.Fstatus.ToString(),
                        activo = c.Cuenta.Factivo,
                        inquilino = new
                        {
                            id = c.Inquilino.FidInquilino,
                            nombre = $"{c.Inquilino.Fnombre} {c.Inquilino.Fapellidos}",
                            cedula = c.Inquilino.Fcedula
                        },
                        inmueble = c.Inmueble != null ? new
                        {
                            id = c.Inmueble.FidInmueble,
                            descripcion = c.Inmueble.Fdescripcion,
                            direccion = c.Inmueble.Fdireccion
                        } : null,
                        periodoPago = new
                        {
                            id = c.PeriodoPago.FidPeriodoPago,
                            nombre = c.PeriodoPago.Fnombre,
                            dias = c.PeriodoPago.Fdias
                        },
                        cuotas = cuotasList
                    };
                }).ToList();

                // 7. Construir respuesta final
                var resultado = new
                {
                    resumen = new
                    {
                        totalDeuda = totalDeuda,
                        totalPagado = totalPagado,
                        clientesConDeuda = clientesConDeuda,
                        cuotasVencidas = cuotasVencidas,
                        porcentajeCobrado = Math.Round((decimal)porcentajeCobrado, 2),
                        distribucionDeuda = deudaPorCliente.Select(d => new
                        {
                            name = d.nombre,
                            amount = d.totalDeuda,
                        })
                    },
                    cuentas = cuentasDetalle
                };

                var fechaHoy = DateTime.Now;

                return Ok(new { success = true, fechaHoy, data = resultado });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener cuentas por cobrar",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("cobros")]
public async Task<IActionResult> GetAllCobros()
{
    try
    {
        // 1. Obtener todos los cobros (activos e inactivos) con JOINs necesarios
        var cobrosQuery = from cobro in _context.TbCobros
                          join cxc in _context.TbCxcs on cobro.FkidCxc equals cxc.FidCuenta
                          join inquilino in _context.TbInquilinos on cxc.FkidInquilino equals inquilino.FidInquilino
                          join inmueble in _context.TbInmuebles on cxc.FkidInmueble equals inmueble.FidInmueble into inmuebleJoin
                          from inmueble in inmuebleJoin.DefaultIfEmpty()
                          join usuario in _context.TbUsuarios on cobro.FkidUsuario equals usuario.FidUsuario
                          join cobroNulo in _context.TbCobrosNulos on cobro.FidCobro equals cobroNulo.FkidCobro into cobroNuloJoin
                          from cobroNulo in cobroNuloJoin.DefaultIfEmpty()
                          select new
                          {
                              Cobro = cobro,
                              CuentaPorCobrar = cxc,
                              Inquilino = inquilino,
                              Inmueble = inmueble,
                              Usuario = usuario,
                              CobroNulo = cobroNulo
                          };

        var cobros = await cobrosQuery.ToListAsync();

        if (!cobros.Any())
        {
            return Ok(new
            {
                success = true,
                data = new
                {
                    resumen = new
                    {
                        totalCobros = 0,
                        totalMonto = 0,
                        cobrosAnulados = 0,
                        distribucionPorMetodo = new List<object>(),
                        cobros = new List<object>()
                    }
                }
            });
        }

        // 2. Obtener todos los detalles de cobros (solo activos)
        var detallesQuery = from detalle in _context.TbCobrosDetalles
                            join cobro in _context.TbCobros on detalle.FkidCobro equals cobro.FidCobro
                            where cobro.Factivo == true && detalle.Factivo == true
                            select detalle;

        var detalles = await detallesQuery.ToListAsync();
        var detallesPorCobro = detalles.GroupBy(d => d.FkidCobro)
            .ToDictionary(g => g.Key, g => g.ToList());

        // 3. Obtener todos los desgloses de cobros (solo activos)
        var desglosesQuery = from desglose in _context.TbCobrosDesgloses
                             join cobro in _context.TbCobros on desglose.FkidCobro equals cobro.FidCobro
                             where cobro.Factivo == true && desglose.Factivo == true
                             select desglose;

        var desgloses = await desglosesQuery.ToListAsync();
        var desglosesPorCobro = desgloses.GroupBy(d => d.FkidCobro)
            .ToDictionary(g => g.Key, g => g.FirstOrDefault());

        // 4. Obtener información de anulaciones
        var cobrosAnuladosInfo = cobros
            .Where(c => c.CobroNulo != null)
            .ToDictionary(
                c => c.Cobro.FidCobro,
                c => c.CobroNulo
            );

        // 5. Calcular totales generales
        var totalCobros = cobros.Count;
        var totalMonto = cobros.Where(c => c.Cobro.Factivo).Sum(c => c.Cobro.Fmonto);
        var cobrosAnulados = cobros.Count(c => !c.Cobro.Factivo || c.CobroNulo != null);

        // 6. Distribución por método de pago (solo cobros activos)
        var distribucionPorMetodo = new
        {
            efectivo = desgloses.Sum(d => d?.Fefectivo ?? 0),
            transferencia = desgloses.Sum(d => d?.Ftransferencia ?? 0),
            tarjeta = desgloses.Sum(d => d?.Ftarjeta ?? 0),
            notaCredito = desgloses.Sum(d => d?.FnotaCredito ?? 0),
            cheque = desgloses.Sum(d => d?.Fcheque ?? 0),
            deposito = desgloses.Sum(d => d?.Fdeposito ?? 0),
            debitoAutomatico = desgloses.Sum(d => d?.FdebitoAutomatico ?? 0)
        };

        // 7. Preparar respuesta detallada de cobros
        var cobrosDetalle = cobros.Select(c =>
        {
            var cobroId = c.Cobro.FidCobro;
            var detallesCobro = detallesPorCobro.TryGetValue(cobroId, out var dt) ? dt : new List<TbCobrosDetalle>();
            var desgloseCobro = desglosesPorCobro.TryGetValue(cobroId, out var dg) ? dg : null;
            var infoAnulacion = cobrosAnuladosInfo.TryGetValue(cobroId, out var anulacion) ? anulacion : null;

            return new
            {
                id = cobroId,
                fecha = c.Cobro.Ffecha,
                hora = c.Cobro.Fhora,
                monto = c.Cobro.Fmonto,
                descuento = c.Cobro.Fdescuento,
                cargos = c.Cobro.Fcargos,
                concepto = c.Cobro.Fconcepto,
                ncf = c.Cobro.Fncf,
                ncfVence = c.Cobro.FncfVence,
                activo = c.Cobro.Factivo,
                origen = c.Cobro.Origen.ToString(),
                anulado = infoAnulacion != null,
                infoAnulacion = infoAnulacion != null ? new
                {
                    motivo = infoAnulacion.FmotivoAnulacion,
                    fechaAnulacion = infoAnulacion.FfechaAnulacion,
                    horaAnulacion = infoAnulacion.Fhora,
                    usuarioAnulacion = c.Usuario.Fnombre // Asumiendo que el usuario que anuló es el mismo que creó
                } : null,
                detalles = detallesCobro.Select(d => new
                {
                    id = d.FidCobroDetalle,
                    numeroCuota = d.FnumeroCuota,
                    monto = d.Fmonto,
                    mora = d.Fmora
                }),
                desglose = desgloseCobro != null ? new
                {
                    efectivo = desgloseCobro.Fefectivo,
                    transferencia = desgloseCobro.Ftransferencia,
                    tarjeta = desgloseCobro.Ftarjeta,
                    notaCredito = desgloseCobro.FnotaCredito,
                    cheque = desgloseCobro.Fcheque,
                    deposito = desgloseCobro.Fdeposito,
                    debitoAutomatico = desgloseCobro.FdebitoAutomatico,
                    noNotaCredito = desgloseCobro.FnoNotaCredito
                } : null,
                cuentaPorCobrar = new
                {
                    id = c.CuentaPorCobrar.FidCuenta,
                    montoTotal = c.CuentaPorCobrar.Fmonto
                },
                inquilino = new
                {
                    id = c.Inquilino.FidInquilino,
                    nombre = $"{c.Inquilino.Fnombre} {c.Inquilino.Fapellidos}",
                    cedula = c.Inquilino.Fcedula
                },
                inmueble = c.Inmueble != null ? new
                {
                    id = c.Inmueble.FidInmueble,
                    descripcion = c.Inmueble.Fdescripcion,
                    direccion = c.Inmueble.Fdireccion
                } : null,
                usuario = new
                {
                    id = c.Usuario.FidUsuario,
                    nombre = $"{c.Usuario.Fnombre}"
                }
            };
        }).ToList();

        // 8. Construir respuesta final
        var resultado = new
        {
            resumen = new
            {
                totalCobros = totalCobros,
                totalMonto = totalMonto,
                cobrosAnulados = cobrosAnulados,
                distribucionPorMetodo = distribucionPorMetodo
            },
            cobros = cobrosDetalle
        };

        return Ok(new { success = true, data = resultado });
    }
    catch (System.Exception ex)
    {
        return StatusCode(500, new
        {
            success = false,
            message = "Error al obtener los cobros",
            error = ex.Message,
            innerError = ex.InnerException?.Message
        });
    }
}
    }
}