/////// FUNCIONES DE CALCULOS ///////
////////////// Función para calcular el Monto Total ///////////////////////////////////////////////////////////
function calcularMontoTotal() {
    console.log('Cargos raw:', $('#Fcargos').val());
    console.log('Descuento raw:', $('#Fdescuento').val());

    const monto = parseFloat($('#Fmonto').val()) || 0;
    const cargos = parseFloat($('#Fcargos').val()) || 0;
    const descuento = parseFloat($('#Fdescuento').val()) || 0;
    const montoTotal = monto + cargos - descuento;

    console.log({ monto, cargos, descuento, montoTotal });
    $('#MontoTotal').val(montoTotal.toFixed(2));
}

/////////// Función para calcular el porcentaje de descuento basado en el valor absoluto ////////////////////////////////////////////
function calcularPorcentajeDescuento() {
    const descuento = parseFloat($('#Fdescuento').val()) || 0;
    const porcentaje = descuento / 100;
    $('#FdescuentoPorcentaje').val(porcentaje.toFixed(2));
    calcularMontoTotal();
}

//////////// Función para calcular el valor absoluto de descuento basado en el porcentaje //////////////////////////////////
function calcularDescuentoAbsoluto() {
    const porcentaje = parseFloat($('#FdescuentoPorcentaje').val()) || 0;
    const descuento = porcentaje * 100;
    $('#Fdescuento').val(descuento.toFixed(2));
    calcularMontoTotal();
}

//////////// Función para calcular el Total Ingresado en el modal //////////////////////////////////
// Función para calcular el Total Ingresado en el modal - MODIFICADA
function calcularTotalIngresado() {
    let total = 0;
    const montoTotal = parseFloat($('#modalMontoTotal').val()) || 0;
    const montoRecibido = parseFloat($('#montoRecibido').val()) || 0;
    const efectivo = parseFloat($('#efectivo').val()) || 0;

    // Lógica modificada:
    if (montoRecibido > 0) {
        // Si hay monto recibido, lo sumamos y calculamos cambio
        total += montoRecibido;
        const cambio = Math.max(0, montoRecibido - efectivo);
        $('#cambio').val(cambio.toFixed(2));
    } else {
        // Si no hay monto recibido, sumamos el efectivo normal
        total += efectivo;
        $('#cambio').val('0.00');
    }

    // Sumar los otros métodos de pago
    total += parseFloat($('#transferencia').val()) || 0;
    total += parseFloat($('#tarjeta').val()) || 0;
    total += parseFloat($('#notaCredito').val()) || 0;
    total += parseFloat($('#cheque').val()) || 0;
    total += parseFloat($('#deposito').val()) || 0;
    total += parseFloat($('#debitoAutomatico').val()) || 0;

    // Calcular pendiente (nunca debe ser negativo)
    const pendiente = Math.max(0, montoTotal - total);

    // Formatear el total y el pendiente
    $('#totalIngresado').text(total.toLocaleString('en-US', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    }));
    $('#montoPendiente').text(pendiente.toLocaleString('en-US', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    }));

    // Habilitar el botón siempre
    $('#finalizarCobro').prop('disabled', false);

    // Mostrar mensaje de validación solo si hay pendiente por pagar
    if (pendiente > 0.01) {
        $('#errorValidacion').removeClass('d-none');
    } else {
        $('#errorValidacion').addClass('d-none');
    }
}

// Función ajaxRequest - MODIFICADA
async function ajaxRequest(config) {
    const defaults = {
        showLoading: true,
        retries: 3,
        retryDelay: 1000,
        suppressPermissionToasts: true
    };

    const options = { ...defaults, ...config };
    let attempts = 0;

    while (attempts < options.retries) {
        try {
            if (options.showLoading) {
                $('#loadingSpinner').show();
            }

            const response = await $.ajax({
                ...options,
                error: null
            });

            return response;
        } catch (error) {
            attempts++;

            if (attempts === options.retries ||
                (error.status && [400, 401, 403, 404].includes(error.status))) {

                switch (error.status) {
                    case 401:
                        window.location.href = '/Account/Login';
                        return;
                    case 403:
                        const errorMessage = error.responseJSON?.error || 'No tiene permisos para realizar esta acción';

                        // Solo mostrar alerta si suppressPermissionToasts es true
                        if (options.suppressPermissionToasts) {
                            throw { ...error, message: 'Permiso denegado' }; // Lanzar error especial
                        }

                        // Si no se suprime, mostrar ambos
                        showPermissionAlert(errorMessage);
                        showToast('Por favor contacte al administrador', 'error');
                        throw error;
                    case 404:
                        showToast('Recurso no encontrado', 'error');
                        throw error;
                    default:
                        if (error.responseJSON?.errors) {
                            if (options.form) {
                                handleValidationErrors($(options.form), error.responseJSON.errors);
                            } else {
                                showToast(Object.values(error.responseJSON.errors).flat().join(' '), 'error');
                            }
                        } else {
                            const userMessage = options.errorMessage || 'Error de comunicación con el servidor';
                            showToast(userMessage, 'error');
                        }
                        throw error;
                }
            }

            await new Promise(resolve => setTimeout(resolve, options.retryDelay));
        } finally {
            $('#loadingSpinner').hide();
        }
    }
}

// Función para mostrar toasts (la misma que en inmuebles)
function showToast(message, type = 'error', duration = 3000) {
    const backgroundColor = type === 'error' ? 'red' : (type === 'success' ? 'green' : 'orange');
    const toast = $(`<div class="toast" style="background-color: ${backgroundColor}; color: white;" role="alert" aria-live="assertive" aria-atomic="true">
                                                <div class="toast-body">${message}</div>
                                            </div>`);
    $('#toastContainer').append(toast);
    new bootstrap.Toast(toast[0], { delay: duration }).show();
}

/////// Funcion para cargar vista tabla Cobro ///////
async function cargarTablaCobro() {
    try {
        const data = await ajaxRequest({
            url: '/TbCobros/CargarCobro',
            type: 'GET',
            errorMessage: 'Error al cargar la lista de cobros',
            suppressPermissionToasts: true // Suprimimos toasts para manejar el 403 manualmente
        })

        $('#dataTableContainer').html(data).fadeIn();
        inicializarDataTable();

    } catch (error) {
        console.error('Error al cargar cobros:', error);

        // Manejo específico para error de permisos (403)
        if (error.status === 403 || error.message === 'Permiso denegado') {
            const errorMsg = error.responseJSON?.error || 'No tiene permisos para ver la lista de cobros';
            showPermissionAlert(errorMsg);
        }
        // Manejo de otros tipos de errores
        else {
            showToast('Error al cargar la lista de cobros', 'error');
        }
    }
}

// Función para manejar errores de validación (la misma que en inmuebles)
function handleValidationErrors(form, errors) {
    form.find('.is-invalid').removeClass('is-invalid');
    form.find('.invalid-feedback').remove();

    for (const [key, value] of Object.entries(errors)) {
        const field = form.find(`[name="${key}"]`);
        field.addClass('is-invalid');

        if (value && value.length > 0) {
            field.after(`<div class="invalid-feedback">${value.join(' ')}</div>`);
        }
    }
}

// Manejador para el formulario de anulación
$(document).on('submit', '.anularCobroForm', function (e) {
    e.preventDefault();
    const form = $(this);
    const cobroId = form.find('input[name="cobroId"]').val();

    if (!cobroId) {
        console.error('No se pudo obtener el ID del cobro');
        showToast('Error al obtener el cobro a anular', 'error');
        return;
    }

    // Mostrar modal de confirmación y guardar el ID
    $('#confirmAnularModal').data('cobro-id', cobroId).modal('show');

    // Cargar datos del usuario actual
    $.get('/api/TbCobrosApi/usuario-actual')
        .done(function (response) {
            if (response.success) {
                const userName = response.usuario.UserName || response.usuario.userName;
                $('#userName').val(userName || 'Usuario no identificado');
            }
        })
        .fail(function (xhr) {
            // Manejo específico para error 403 (Forbidden)
            if (xhr.status === 403) {
                const mensaje = xhr.responseJSON?.error || 'No tiene permisos para consultar el usuario actual.';
                showPermissionAlert(mensaje);
                return;
            }

            $('#userName').val('Error al cargar usuario');
            showToast('Error al cargar datos del usuario', 'error');
            console.error('Error al consultar usuario actual:', xhr);
        });
});


// Manejador para el botón de confirmación (usa delegación de eventos)
$(document).on('click', '#confirmAnularBtn', async function () {
    const btn = $(this);
    const cobroId = $('#confirmAnularModal').data('cobro-id');

    if (!cobroId) {
        showToast('No se pudo identificar el cobro a anular', 'error');
        return;
    }

    btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Anulando...');

    const motivo = $('#motivoAnulacion').val();
    const password = $('#usuarioPassword').val();

    if (!motivo || !password) {
        showToast('Debe completar todos los campos', 'error');
        btn.prop('disabled', false).html('<i class="fas fa-ban me-1"></i> Anular Cobro');
        return;
    }

    try {
        // Validar contraseña
        const validacion = await $.ajax({
            url: '/api/TbCobrosApi/validar-contrasena',
            type: 'POST',
            data: JSON.stringify({ password: password }),
            contentType: 'application/json',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            }
        });

        if (!validacion.success) {
            showToast(validacion.message || 'Contraseña incorrecta', 'error');
            return;
        }

        // Anular el cobro
        const response = await $.ajax({
            url: `/api/TbCobrosApi/anular/${cobroId}`,
            type: 'POST',
            data: JSON.stringify({
                motivoAnulacion: motivo,
                password: password
            }),
            contentType: 'application/json',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            }
        });

        if (response.success) {
            $('#confirmAnularModal').modal('hide');
            showToast(response.message, 'success');
            await cargarTablaCobro();
        } else {
            showToast(response.message || 'Error al anular el cobro', 'error');
        }

    } catch (error) {
        console.error('Error al anular cobro:', error);

        // Manejo específico de error 403
        if (error.status === 403) {
            showPermissionAlert(error.responseJSON?.message || 'No tiene permisos para esta acción');
        } else {
            const errorMsg = error.responseJSON?.message || 'Error al anular el cobro';
            showToast(errorMsg, 'error');
        }

    } finally {
        btn.prop('disabled', false).html('<i class="fas fa-ban me-1"></i> Anular Cobro');
        $('#motivoAnulacion').val('');
        $('#usuarioPassword').val('');
    }
});



async function mostrarDetallesCobro(idCobro) {
    console.log(`Requesting details for ID: ${idCobro}`);
    const url = `/TbCobros/Detalles/${idCobro}`;

    try {
        const html = await ajaxRequest({
            url: url,
            type: 'GET',
            errorMessage: 'Error al cargar los detalles del cobro',
            suppressPermissionToasts: true // Suprimir toasts para manejar el 403 manualmente
        });

        $('#dataTableContainer').html(html).fadeIn();

        // Inicializar datatable
        const table = $('#reciboTable').DataTable({
            searching: false,
            paging: false,
            info: false,
            ordering: false,
            responsive: true,
            scrollX: false,
            scrollCollapse: true,
            dom: '<"top"lf>Brt<"bottom"ip>',
            columnDefs: [
                {
                    targets: '_all',
                    orderable: false,
                    defaultContent: ""
                }
            ],
            buttons: [
                {
                    extend: 'pdfHtml5',
                    text: '<i class="fas fa-file-pdf"></i> PDF',
                    className: 'btn btn-outline-danger btn-sm mb-4 mt-4',
                    orientation: 'portrait',
                    pageSize: 'LETTER',
                    exportOptions: {
                        columns: ':visible',
                        modifier: {
                            page: 'all'
                        }
                    },
                    customize: function (doc) {
                        // ... (keep all your existing PDF customization code)
                    }
                }
            ],
            initComplete: function () {
                $('.dataTables_length, .dataTables_filter').hide();
                this.api().columns.adjust();
            },
            createdRow: function (row, data, index) {
                if ($('td', row).length === 1) {
                    $('td', row).attr('colspan', '2');
                }
            }
        });

        $('#reciboTable').removeClass('dataTable');

    } catch (error) {
        console.error('Error al cargar los detalles del cobro:', error);

        // Manejo específico para error de permisos (403)
        if (error.status === 403 || error.message === 'Permiso denegado') {
            const errorMsg = error.responseJSON?.error || 'No tiene permisos para ver los detalles del cobro';
            showPermissionAlert(errorMsg);
        }
        // Manejo de otros tipos de errores
        else {
            showToast('Error al cargar los detalles del cobro', 'error');
        }
    }
}



// Modificar el evento click de los botones de detalles
$(document).on('click', '.btn-detalles', function (e) {
    e.preventDefault();
    const idCobro = $(this).attr('data-id');
    mostrarDetallesCobro(idCobro);
});

function agregarInputOculto(cuotaId, monto, mora) {
    const container = $('#inputsSeleccionados');

    // Validar que el contenedor existe
    if (container.length === 0) {
        console.error('Error: El contenedor #inputsSeleccionados no existe en el DOM.');
        return;
    }

    // Limpiar inputs existentes para esta cuota
    $(`#cuota_inputs_${cuotaId}`).remove();

    const inputHtml = `
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            <div id="cuota_inputs_${cuotaId}" class="mb-2">
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                <input type="hidden" name="cuotasSeleccionadas" value="${cuotaId}" />
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                <input type="hidden" name="cuotas[${cuotaId}].Monto" id="cuota_monto_${cuotaId}" value="${monto}" />
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                <input type="hidden" name="cuotas[${cuotaId}].Mora" id="cuota_mora_${cuotaId}" value="${mora}" />
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                <input type="hidden" name="cuotas[${cuotaId}].Id" value="${cuotaId}" />
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            </div>
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        `;

    container.append(inputHtml);
    console.log(`Inputs ocultos creados para cuota ${cuotaId}:`, { monto, mora });
}


//////////////////////////// SOLICITUD PARA CREAR LOS COBROS UTILIZANDO AXIOS ///////////////////////////////////////

async function crearCobro() {
    // Obtener cuotas seleccionadas
    const cuotasSeleccionadas = Array.from(
        document.querySelectorAll('input[name="cuotasSeleccionadas"]:checked')
    ).map(el => parseInt(el.value));

    // Validar cuotas seleccionadas
    if (cuotasSeleccionadas.length === 0) {
        alert('Seleccione al menos una cuota.');
        return false;
    }

    // Calcular el total realmente ingresado
    const totalIngresado = parseFloat($('#totalIngresado').text()) || 0;
    if (totalIngresado <= 0) {
        alert('Debe ingresar al menos un monto en el desglose de pago.');
        return false;
    }

    // Obtener datos de la primera cuota seleccionada
    const primeraCuotaId = cuotasSeleccionadas[0];
    const saldoCuota = parseFloat($(`#cuota_monto_${primeraCuotaId}`).val()) || 0;
    const moraCuota = parseFloat($(`#cuota_mora_${primeraCuotaId}`).val()) || 0;

    const cobroData = {
        fkidCxc: parseInt($('#busquedaCxC').val()),
        fmontoCobro: totalIngresado,  // Enviar el monto realmente ingresado
        fdescuento: parseFloat($('#Fdescuento').val()) || 0,
        fcargos: parseFloat($('#Fcargos').val()) || 0,
        fconcepto: $('#Fconcepto').val(),
        cuotasSeleccionadas: cuotasSeleccionadas,
        fnumeroCuota: primeraCuotaId,
        fmontoCuota: saldoCuota,
        fmora: moraCuota,
        fefectivo: parseFloat($('#efectivo').val()) || 0,
        fmontoRecibido: parseFloat($('#montoRecibido').val()) || 0,
        ftransferencia: parseFloat($('#transferencia').val()) || 0,
        ftarjeta: parseFloat($('#tarjeta').val()) || 0,
        fnotaCredito: parseFloat($('#notaCredito').val()) || 0,
        fcheque: parseFloat($('#cheque').val()) || 0,
        fdeposito: parseFloat($('#deposito').val()) || 0,
        fdebitoAutomatico: parseFloat($('#debitoAutomatico').val()) || 0,
        fnoNotaCredito: parseInt($('#FnoNotaCredito').val(), 10) || 0
    };

    // Validación adicional en el cliente
    if (cobroData.FnoNotaCredito < 0) {
        alert('El número de nota de crédito no puede ser negativo');
        return false;
    }

    if (cobroData.FnotaCredito > 0 && cobroData.FnoNotaCredito <= 0) {
        alert('Debe ingresar un número de nota de crédito válido (mayor a 0) cuando hay monto');
        return false;
    }

    console.log("Datos del cobro enviados al backend:", JSON.stringify(cobroData, null, 2));

    try {
        const response = await axios.post('/api/TbCobrosApi/create', cobroData, {

            headers: {
                'Content-Type': 'application/json',
                'X-Source': 'Web',
                'RequestVerificationToken': document.getElementById('__RequestVerificationToken').value
            }

        });
        console.log("Success:", response.data);
        if (response.data.success) {
            $('#FkidCobro').val(response.data.cobroId);
            alert('Cobro registrado exitosamente');

            $('#staticBackdrop').modal('hide');


            // Generar el ticket
            generarTicket(); // Asegúrate de que el servidor devuelva los datos del cobro
            // Mostrar el modal del ticket
            $('#ticketModal').modal('show');
            limpiarDatosTicket();
            cargarVistaCrearCobro();
            return true;
        } else {
            alert('Error: ' + response.data.message);
            return false;
        }
    } catch (error) {
        console.error('Error completo:', error);
        if (error.response) {
            console.error('Detalles del error:', error.response.data);
        }
        console.error('Full error:', error.response.data);
        alert('Error al procesar la solicitud. Ver consola para detalles.');
    }
}


// Función modificada para evitar la redeclaración de variables
function generarTicket(cuentaIdParam, cobroIdParam) {
    // Obtener la fecha y hora actual
    const ahora = new Date();
    const fecha = ahora.toLocaleDateString();
    const hora = ahora.toLocaleTimeString();

    // Usar los parámetros recibidos o obtener los valores de los campos
    const cuentaId = cuentaIdParam || $('#busquedaCxC').val();
    const cobroId = cobroIdParam || $('#FkidCobro').val();

    // Obtener el concepto ingresado por el usuario (si lo hay)
    const conceptoUsuario = $('#Fconcepto').val();

    // Hacer la llamada AJAX para obtener los datos del ticket
    $.ajax({
        url: '/TbCobros/GetDatosTicket',
        type: 'GET',
        data: {
            cuentaId: cuentaId,
            idCobro: cobroId,
            esReimpresion: true
        },
        success: function (response) {
            if (response.success) {
                const datos = response.datos;

                // Obtener información de la empresa
                $.get('/Empresa/GetEmpresaInfo', function (data) {
                    // Logo
                    var $titleElement = $('#ticketContent .header .title');
                    var logoUrl = '/Empresa/Logo?' + new Date().getTime();

                    if (data.tieneLogo) {
                        $titleElement.html('<img src="' + logoUrl + '" alt="Logo" style="max-height: 100px; max-width: 100%;" onerror="this.onerror=null;this.src=\'/images/login.jpg\';" />');
                    } else {
                        $titleElement.text(data.nombre);
                    }

                    // Datos de la empresa
                    $('#ticketContent .header .rnc').text(data.rnc || "RNC no configurado");
                    $('#ticketContent .header .direccion').text(data.dir || "Dirección no configurada");

                    // Teléfono
                    const telefono = data.tel || "Teléfonos no configurados";
                    $('#ticketContent .header .telefono').text(telefono);

                }).fail(function () {
                    $('#ticketContent .header .title').text("Nombre de Empresa");
                    $('#ticketContent .header .rnc').text("RNC no configurado");
                    $('#ticketContent .header .direccion').text("Dirección no configurada");
                    $('#ticketContent .header .telefono').text("Teléfonos no configurados");
                });

                // Datos del ticket desde el backend
                $('#ticketContent .fecha-hora').text(`${fecha} ${hora}`);
                $('#ticketContent .cliente').text(datos.cliente);
                $('#ticketContent .noCobro').text(datos.noCobro);
                $('#ticketContent .concepto').text(conceptoUsuario || datos.concepto);
                $('#ticketContent .direccion').text(datos.direccion);
                $('#ticketContent .ubicacion').text(datos.ubicacion);
                $('#ticketContent .telefono').text(datos.telefono);

                // Datos numéricos formateados
                const formatOptions = { minimumFractionDigits: 2, maximumFractionDigits: 2 };
                $('#ticketContent .subtotal').text(datos.subtotal.toLocaleString('en-US', formatOptions));
                $('#ticketContent .cargos').text('+' + datos.cargos.toLocaleString('en-US', formatOptions));
                $('#ticketContent .mora').text('+' + datos.mora.toLocaleString('en-US', formatOptions));
                $('#ticketContent .descuento').text('-' + datos.descuento.toLocaleString('en-US', formatOptions));
                $('#ticketContent .total').text(datos.total.toLocaleString('en-US', formatOptions));

                // Métodos de pago (mostrar solo los que tienen valor > 0)
                const mostrarMetodoPago = (selector, valor) => {
                    const element = $(`#ticketContent .${selector}`);
                    if (valor > 0) {
                        element.text(valor.toLocaleString('en-US', formatOptions));
                        element.closest('.item-content').show();
                    } else {
                        element.closest('.item-content').hide();
                    }
                };

                mostrarMetodoPago('efectivo', datos.efectivo);
                mostrarMetodoPago('transferencia', datos.transferencia);
                mostrarMetodoPago('tarjeta', datos.tarjeta);
                mostrarMetodoPago('cheque', datos.cheque);
                mostrarMetodoPago('deposito', datos.deposito);
                mostrarMetodoPago('montoNotaCredito', datos.montoNotaCredito);
                mostrarMetodoPago('debitoAutomatico', datos.debitoAutomatico);

                // Mostrar número de nota crédito si aplica
                if (datos.noNotaCredito > 0) {
                    $('#ticketContent .noNotaCredito').text(datos.noNotaCredito);
                    $('#ticketContent .noNotaCredito').closest('.item-content').show();
                } else {
                    $('#ticketContent .noNotaCredito').closest('.item-content').hide();
                }

                // Efectivo recibido y cambio
                $('#ticketContent .efectivo-recibido').text(datos.efectivoRecibido.toLocaleString('en-US', formatOptions));
                $('#ticketContent .cambio').text(datos.cambio.toLocaleString('en-US', formatOptions));
                $('#ticketContent .resto').text("0.00");

                // QR Web - Modificado para evitar el error 404
                $.get('/Empresa/HasQrWeb', function (hasQr) {
                    if (hasQr) {
                        var qrUrl = '/Empresa/QrWeb?' + new Date().getTime();
                        $('#ticketContent .qr').html('<img src="' + qrUrl + '" alt="QR Web" style="max-height: 60px; max-width: 100%;" onerror="$(this).parent().text(\'CODIGO QR\')" />');
                    } else {
                        $('#ticketContent .qr').text('CODIGO QR');
                    }
                }).fail(function () {
                    $('#ticketContent .qr').text('CODIGO QR');
                });

                // Mostrar el modal del ticket
                $('#ticketModal').modal('show');

            } else {
                console.error('Error del backend:', response.message);
                alert('Error al cargar los datos del ticket: ' + response.message);
            }
        },
        error: function (xhr, status, error) {
            console.error('Error en la llamada AJAX:', status, error);
            alert('Error al comunicarse con el servidor');
        }
    });
}

// Manejar clic en botón de reimprimir ticket
$(document).on('click', '.btn-reimprimir', function () {
    const cobroId = $(this).data('id');
    const cuentaId = $(this).data('cuenta');

    // Llamar a la función generarTicket con los IDs
    generarTicket(cuentaId, cobroId);
});

function limpiarDatosTicket() {
    $('#ticketContent .direccion').text('');
    $('#ticketContent .ubicacion').text('');
    $('#ticketContent .telefono').text('');
    $('#ticketContent .fecha-hora').text('');
    $('#ticketContent .cliente').text('');
    $('#ticketContent .noCobro').text('');
    $('#ticketContent .concepto').text('');
    $('#ticketContent .subtotal').text('');
    $('#ticketContent .cargos').text('');
    $('#ticketContent .mora').text('');
    $('#ticketContent .descuento').text('');
    $('#ticketContent .total').text('');
    $('#ticketContent .efectivo').text('');
    $('#ticketContent .transferencia').text('');
    $('#ticketContent .tarjeta').text('');
    $('#ticketContent .cheque').text('');
    $('#ticketContent .deposito').text('');
    $('#ticketContent .montoNotaCredito').text('');
    $('#ticketContent .noNotaCredito').text('');
    $('#ticketContent .debitoAutomatico').text('');
    $('#ticketContent .efectivo-recibido').text('');
    $('#ticketContent .cambio').text('');
    $('#ticketContent .resto').text('');
    $('#ticketContent .qr').text('');
}


// Asegúrate de que el botón de imprimir funcione correctamente
$('#imprimirTicket').on('click', function () {
    window.print();
});


function validarDatosCobro() {
    const requiredFields = [
        { field: $('#busquedaCxC').val(), message: 'Seleccione una cuenta por cobrar' },
        { field: $('#MontoTotal').val(), message: 'El monto total no puede ser cero' }
    ];

    for (const item of requiredFields) {
        if (!item.field) {
            alert(item.message);
            return false;
        }
    }
    return true;
}

$('#loadCobro').on('click', function () {
    cargarTablaCobro();
});

$('#createCobro').on('click', function () {
    cargarVistaCrearCobro();
});

// Función para cargar vista formulario Create (ajustada con manejo de errores)
async function cargarVistaCrearCobro() {
    console.log('Iniciando carga de vista de creación...');

    try {
        const html = await ajaxRequest({
            url: '/TbCobros/Create',
            type: 'GET',
            errorMessage: 'Error al cargar la vista de creación'
        });

        $('#dataTableContainer').html(html).fadeIn();
        console.log('Formulario cargado con éxito en el contenedor');

        // Inicialización de componentes
        inicializarSelect2();
        aplicarMascarasNumericas();
        filtrarCuotasPorCxc();

        // Eventos de cálculo
        $('#Fmonto, #Fcargos, #Fdescuento').on('input change', calcularMontoTotal);
        $('#Fdescuento').on('input change', calcularPorcentajeDescuento);
        $('#FdescuentoPorcentaje').on('input change', calcularDescuentoAbsoluto);

        // Validación de cuotas seleccionadas
        $('#continuarBtn').off('click').on('click', function (e) {
            const cuotasSeleccionadas = $('input[name="cuotasSeleccionadas"]:checked');
            if (cuotasSeleccionadas.length === 0) {
                e.preventDefault();
                $('#cuotasError').show();
                $('#tablaCobros').addClass('is-invalid');
            } else {
                $('#cuotasError').hide();
                $('#tablaCobros').removeClass('is-invalid');
            }
        });


        // Modal de confirmación
        $('[data-bs-target="#staticBackdrop"]').on('click', function (e) {
            e.preventDefault();
            calcularMontoTotal();
            const montoPreModal = $('#MontoTotal').val();
            $('#staticBackdrop').modal('show');
        });

        $('#staticBackdrop').on('shown.bs.modal', function () {
            aplicarMascarasModal();

            const montoTotal = $('#MontoTotal').val();

            $('#modalMontoTotal').val(montoTotal);
            $('#montoPendiente').text(montoTotal);

            $('.modal-body input[type="text"]:not(#modalMontoTotal)').val('0.00');
            $('#FnoNotaCredito').val('');
            $('#totalIngresado').text('0.00');
            $('#errorValidacion').addClass('d-none');
            $('#finalizarCobro').prop('disabled', false);
        });

        // Cálculo en tiempo real en el modal
        $('.modal-body input[type="text"]').on('input', calcularTotalIngresado);

        // Finalización del cobro
        $('#finalizarCobro').off('click').on('click', async function () {
            const btn = this;
            $(btn).html('Procesando...');

            const fkidCxc = $('#busquedaCxC').val();
            const fmonto = $('#Fmonto').val();

            if (!fkidCxc || !fmonto) {
                alert('Complete los campos obligatorios');
                btn.disabled = false;
                $(btn).html('Finalizar Cobro');
                return;
            }

            await crearCobro();
            btn.disabled = false;
            $(btn).html('Finalizar Cobro');
        });

    } catch (error) {
        console.error('Error al cargar los cobros:', error);

        // Manejo específico para error de permisos (403)
        if (error.status === 403 || error.message === 'Permiso denegado') {
            const errorMsg = error.responseJSON?.error || 'No tiene permisos para ver la lista de inquilinos';
            showPermissionAlert(errorMsg);
        }
        // Manejo de otros tipos de errores
        else {
            showToast('Error al cargar la lista de cobros', 'error');
        }
    }
}


function filtrarCuotasPorCxc() {
    console.log('Configurando filtrado de cuotas por CxC...');

    $('#busquedaCxC').off('change').on('change', function () {
        const cuentaId = $(this).val();
        console.log('Cambio en selección de CxC - ID seleccionado:', cuentaId);

        if (cuentaId) {
            console.log('Solicitando cuotas para CxC:', cuentaId);

            $.ajax({
                url: '/TbCobros/GetMontoByCuenta',
                type: 'GET',
                data: { cuentaId: cuentaId },
                success: function (data) {
                    console.log('Respuesta recibida para cuotas:', data);
                    if (data.success) {
                        console.log('Datos de cuotas válidos recibidos');

                        // Destruir DataTable existente
                        if ($.fn.DataTable.isDataTable('#tablaCobros')) {
                            console.log('Destruyendo DataTable existente...');
                            $('#tablaCobros').DataTable().clear().destroy();
                        }

                        $('#tablaCobros tbody').empty();
                        console.log('Contenido de tabla limpiado');

                        if (data.cuotas?.length > 0) {
                            console.log('Creando nueva DataTable con', data.cuotas.length, 'cuotas');

                            $('#tablaCobros').DataTable({
                                data: data.cuotas.filter(c => c.fstatus === 'N' || c.fstatus === 'V'), // Filtrar cuotas en el frontend
                                lengthMenu: [12, 25, 50, 100],
                                columns: [
                                    {
                                        data: null,
                                        render: function (data, type, row) {
                                            const cuotaId = row.fnumeroCuota; // Usar solo fnumeroCuota
                                            return `<input type="checkbox" class="row-checkbox" name="cuotasSeleccionadas" value="${cuotaId}" />`;
                                        }
                                    },
                                    { data: 'fnumeroCuota' },
                                    {
                                        data: 'fvence',
                                        render: function (data) {
                                            const fecha = new Date(data);
                                            if (isNaN(fecha.getTime())) return '*';
                                            const dia = String(fecha.getDate()).padStart(2, '0');
                                            const mes = String(fecha.getMonth() + 1).padStart(2, '0');
                                            const anio = fecha.getFullYear();
                                            return `${dia}/${mes}/${anio}`; //Formato dd/mm/yyyy
                                        }
                                    },
                                    {
                                        data: 'fmonto',
                                        render: data => parseFloat(data).toFixed(2),
                                        class: 'numeric-input'
                                    },
                                    {
                                        data: 'fsaldo',
                                        render: data => parseFloat(data).toFixed(2),
                                        class: 'numeric-input'
                                    },
                                    {
                                        data: 'fmora',
                                        render: data => `${data}%`
                                    },
                                    {
                                        data: 'fstatus',
                                        render: function (data) {
                                            let estadoTexto = '';
                                            let badgeClass = '';
                                            let textColor = '#fff';
                                            let bgColor = '#6c757d';

                                            switch (data) {
                                                case 'V':
                                                    estadoTexto = 'Vencido';
                                                    badgeClass = 'badge-danger';
                                                    bgColor = '#dc3545';
                                                    break;
                                                case 'N':
                                                    estadoTexto = 'Pendiente';
                                                    badgeClass = 'badge-warning';
                                                    bgColor = '#ffc107';
                                                    textColor = '#333';
                                                    break;
                                                default:
                                                    estadoTexto = data;
                                                    badgeClass = 'badge-secondary';
                                                    break;
                                            }

                                            return `<span class="badge ${badgeClass}" style="background-color: ${bgColor}; color: ${textColor}; padding: 0.25em 0.4em; font-size: 75%; font-weight: 700; border-radius: 0.25rem;">${estadoTexto}</span>`;
                                        }
                                    }
                                ],
                                responsive: true,
                                drawCallback: function () {
                                    console.log('DataTable dibujada - configurando checkboxes');
                                    manejarCheckboxes();
                                }
                            });

                            aplicarMascarasNumericas();
                        } else {
                            console.warn('No hay cuotas para mostrar');
                            $('#tablaCobros tbody').append('<tr><td colspan="7" class="text-center">No hay cuotas para esta cuenta.</td></tr>');
                        }
                    } else {
                        console.warn('La respuesta no fue exitosa:', data);
                    }
                },
                error: function (xhr, status, error) {
                    console.error('Error al obtener cuotas:', {
                        status: status,
                        error: error,
                        responseText: xhr.responseText
                    });
                    alert('Error al obtener datos de la cuenta.');
                }
            });
        } else {
            console.log('No se seleccionó ninguna CxC');
        }
    });
}



// ✅ Disparar evento change si ya hay una cuenta seleccionada
const cuentaSeleccionada = $('#busquedaCxC').val();
if (cuentaSeleccionada) {
    $('#busquedaCxC').trigger('change');
}


function actualizarCuotasSeleccionadas() {
    const cuotasSeleccionadas = [];
    document.querySelectorAll(".row-checkbox:checked").forEach(function (checkbox) {
        cuotasSeleccionadas.push(checkbox.value);
    });

    document.getElementById("selectedCuotas").value = cuotasSeleccionadas.join(",");
}


document.addEventListener("change", function (e) {
    if (e.target.classList.contains("row-checkbox")) {
        actualizarCuotasSeleccionadas();
    }
});

function manejarCheckboxes() {
    // Establecer el primer checkbox como habilitado y los demás deshabilitados
    $('input.row-checkbox').each(function (index) {
        if (index === 0) {
            $(this).prop('disabled', false);
        } else {
            $(this).prop('disabled', true);
        }
    });

    // Manejar el evento de cambio de los checkboxes
    $('input.row-checkbox').on('change', function () {
        const row = $(this).closest('tr');
        const cuotaId = $(this).val();

        // Obtener SALDO de la columna fsaldo (5ta columna, índice 4)
        const saldo = parseFloat(
            row.find('td:eq(4)').text().replace(/[^0-9.]/g, '')
        ) || 0;

        // Obtener mora (eliminar porcentaje y caracteres no numéricos)
        const mora = parseFloat(
            row.find('td:eq(5)').text().replace(/[^0-9.]/g, '')
        ) || 0;

        if ($(this).prop('checked')) {
            // Agregar inputs ocultos usando el SALDO en lugar del monto
            agregarInputOculto(cuotaId, saldo, mora);

            // Habilitar el siguiente checkbox si existe
            const nextCheckbox = $(this).closest('tr').next().find('.row-checkbox');
            if (nextCheckbox.length) {
                nextCheckbox.prop('disabled', false);
            }
        } else {
            // Eliminar inputs ocultos cuando se deselecciona
            $(`#cuota_inputs_${cuotaId}`).remove();

            // Deshabilitar checkboxes siguientes
            $(this).closest('tr').nextAll().find('.row-checkbox').prop({
                disabled: true,
                checked: false
            }).each(function () {
                const id = $(this).val();
                $(`#cuota_inputs_${id}`).remove();
            });
        }

        // Recalcular el total usando los SALDOS de las cuotas seleccionadas
        let totalSaldo = 0;
        $('input.row-checkbox:checked').each(function () {
            const row = $(this).closest('tr');
            const saldo = parseFloat(row.find('td:nth-child(5)').text().replace(',', ''));
            if (!isNaN(saldo)) {
                totalSaldo += saldo;
            }
        });

        $('#Fmonto').val(totalSaldo.toFixed(2)).trigger('input');
        calcularMontoTotal();
    });
}

/////// Funcion para inicializar Select2 ///////
let cxCData = [];

function inicializarSelect2() {
    const $select = $('#busquedaCxC');
    const valorSeleccionado = $select.val();

    // Limpiar opciones previas y destruir Select2 si ya existe
    if ($select.hasClass('select2-hidden-accessible')) {
        $select.select2('destroy');
    }

    // Inicializar Select2 (vacío temporalmente)
    $select.empty().select2({
        placeholder: "Buscar cuenta por cobrar...",
        allowClear: true,
        matcher: matchCustom
    });

    // Cargar opciones desde el servidor
    $.ajax({
        url: '/TbCobros/BuscarCxc',
        dataType: 'json',
        success: function (data) {
            cxCData = data.results.filter(item => item.tipo === "cuenta por cobrar");

            $select.append(new Option('', '', false, false)); // Opción vacía

            cxCData.forEach(item => {
                const option = new Option(item.text, item.id, false, false);
                $select.append(option);
            });

            // Establecer el valor previamente seleccionado
            if (valorSeleccionado) {
                $select.val(valorSeleccionado).trigger('change');
            }

            // Inicializar Select2 después de agregar las opciones
            $select.select2();
        },
        error: function () {
            alert('Error al cargar las cuentas por cobrar.');
        }
    });

    // Enlazar el cambio al input oculto
    $select.on('change', function () {
        $('#FkidCxc').val($(this).val());
    });
}

/////// Funcion para inicializar Match Custom CxC ///////
function matchCustom(params, data) {
    if ($.trim(params.term) === '') {
        return data;
    }
    if (typeof data.text === 'undefined') {
        return null;
    }
    if (data.text.toLowerCase().indexOf(params.term.toLowerCase()) > -1) {
        var modifiedData = $.extend({}, data, true);
        modifiedData.text += ' (matched)';
        return modifiedData;
    }
    return null;
}

/////// Funcion para inicializar Mascara Decimal ///////
function aplicarMascarasNumericas() {
    // Eliminar máscaras previas
    $('.numeric-input, .numeric-display').inputmask('remove');

    // Máscara para inputs editables
    const maskOptions = {
        alias: "numeric",
        groupSeparator: ",",
        autoGroup: true,
        digits: 2,
        digitsOptional: false,
        radixPoint: ".",
        placeholder: "0.00",
        rightAlign: false,
        autoUnmask: true,
        removeMaskOnSubmit: true
    };
    $('.numeric-input').inputmask(maskOptions);

    // Máscara para elementos de solo lectura (display)
    const displayMaskOptions = {
        alias: "numeric",
        groupSeparator: ",",
        autoGroup: true,
        digits: 2,
        digitsOptional: false,
        radixPoint: ".",
        placeholder: "",
        rightAlign: false,
        static: true,  // Máscara estática para elementos de solo lectura
        suffix: '',    // Puedes agregar "RD$ " como prefijo si necesitas
        autoUnmask: false
    };
    $('.numeric-display').inputmask(displayMaskOptions);

    // Máscara para teléfonos (la que ya tenías)
    $('.telefono-display').inputmask({
        mask: '(999) 999-9999',
        static: true,
        placeholder: ''
    });
}

/////// Funcion para inicializar Mascara Decimal ///////
function aplicarMascarasModal() {
    const $campos = $('#modalMontoTotal, #efectivo,#montoRecibido, #transferencia, #tarjeta, #notaCredito, #cheque, #deposito, #debitoAutomatico');
    console.log("Aplicando máscaras a estos campos:", $campos.length);
    $('.decimal-input').inputmask('remove');
    const maskOptions = {
        alias: "decimal",
        groupSeparator: ",",
        autoGroup: true,
        digits: 2,
        digitsOptional: false,
        radixPoint: ".",
        placeholder: "0.00",
        rightAlign: false,
        autoUnmask: true,
        removeMaskOnSubmit: true
    };

    $('.decimal-input').inputmask(maskOptions);
}

// Inicialización de DataTable
function inicializarDataTable() {
    // Configuración de localización
    moment.locale('es', {
        months: 'Enero_Febrero_Marzo_Abril_Mayo_Junio_Julio_Agosto_Septiembre_Octubre_Noviembre_Diciembre'.split('_'),
        monthsShort: 'Ene_Feb_Mar_Abr_May_Jun_Jul_Ago_Sep_Oct_Nov_Dic'.split('_'),
        weekdays: 'Domingo_Lunes_Martes_Miércoles_Jueves_Viernes_Sábado'.split('_'),
        weekdaysShort: 'Dom_Lun_Mar_Mié_Jue_Vie_Sáb'.split('_')
    });

    // Inicializar datepickers
    var minDate = new DateTime($('#min'), {
        format: 'DD/MM/YYYY',
        i18n: {
            previous: 'Anterior',
            next: 'Siguiente',
            months: moment.localeData('es').months(),
            weekdays: moment.localeData('es').weekdaysShort()
        }
    });

    var maxDate = new DateTime($('#max'), {
        format: 'DD/MM/YYYY',
        i18n: {
            previous: 'Anterior',
            next: 'Siguiente',
            months: moment.localeData('es').months(),
            weekdays: moment.localeData('es').weekdaysShort()
        }
    });

    // Configuración de DataTable
    var table = $('#tablaCobros').DataTable({
        pageLength: 10,
        lengthMenu: [5, 10, 25, 50, 100],
        order: [[0, 'desc']],
        responsive: true,
        dom: '<"top"lf>Brt<"bottom"ip>',
        buttons: [
            {
                extend: 'pdfHtml5',
                text: '<i class="fas fa-file-pdf"></i> PDF',
                className: 'btn btn-outline-danger',
                titleAttr: 'Exportar a PDF',
                orientation: 'landscape',
                pageSize: 'A4',
                exportOptions: {
                    columns: [0, 1, 2, 3, 4, 5, 6, 7, 8],
                    modifier: {
                        page: 'all'
                    }
                },
                customize: function (doc) {
                    // Función para formatear números al estilo RD (comas para miles, punto para decimales)
                    function formatoRD(numero) {
                        return numero.toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,');
                    }

                    // Obtener fecha y hora actual
                    const now = new Date();
                    const fechaHora = now.toLocaleDateString('es-RD', {
                        year: 'numeric',
                        month: 'long',
                        day: 'numeric',
                        hour: '2-digit',
                        minute: '2-digit'
                    });

                    const table = $('#tablaCobros').DataTable();

                    // Encontrar los índices de las columnas necesarias
                    let montoIndex = -1;

                    table.columns().every(function () {
                        const headerText = this.header().textContent.trim();
                        if (headerText === 'Monto') {
                            montoIndex = this.index();
                        }
                    });

                    // Calcular el total de la columna Monto
                    let totalMonto = 0;
                    let totalRegistros = 0;

                    if (montoIndex !== -1) {
                        table.rows({ search: 'applied' }).data().each(function (row) {
                            const montoStr = row[montoIndex].toString();
                            const montoLimpio = montoStr.replace(/[^\d.-]/g, '');
                            const montoNum = parseFloat(montoLimpio) || 0;
                            totalMonto += montoNum;
                        });
                    }

                    // Contar registros (filas visibles después de búsqueda/filtrado)
                    totalRegistros = table.rows({ search: 'applied' }).count();

                    // Formatear los totales al formato RD
                    const totalFormateado = 'RD$ ' + formatoRD(totalMonto);

                    // Ajustar márgenes y centrar contenido
                    doc.pageMargins = [40, 80, 40, 60];
                    doc.defaultStyle.fontSize = 8;
                    doc.styles.tableHeader.fontSize = 9;
                    doc.styles.tableHeader.alignment = 'center';
                    doc.content[0].alignment = 'center';

                    // Añadir información de total de registros al título
                    doc.content.splice(1, 0, {
                        text: `Total de registros: ${totalRegistros.toLocaleString('es-RD')}`,
                        alignment: 'right',
                        margin: [0, 0, 40, 10],
                        fontSize: 9,
                        bold: true
                    });

                    // Añadir fila de total al cuerpo de la tabla
                    if (doc.content[2].table.body.length > 0) {
                        const columnsCount = doc.content[2].table.body[0].length;
                        const totalRow = new Array(columnsCount).fill('');

                        // Colocar "TOTAL:" en la columna correspondiente (índice 2)
                        totalRow[2] = { text: 'TOTAL:', bold: true, alignment: 'right' };

                        // Colocar el monto total en la columna Monto (índice 3)
                        totalRow[3] = { text: totalFormateado, bold: true, alignment: 'right' };

                        doc.content[2].table.body.push(totalRow);
                    }

                    // Centrar la tabla
                    doc.content[2].alignment = 'center';

                    // Ajustar el ancho de la tabla
                    doc.content[2].table.widths = Array(doc.content[2].table.body[0].length + 1).join('*').split('');

                    // Footer con paginación y fecha
                    doc['footer'] = function (page, pages) {
                        return {
                            columns: [
                                {
                                    alignment: 'left',
                                    text: `Generado: ${fechaHora}`,
                                    fontSize: 8,
                                    margin: [40, 10, 0, 0]
                                },
                                {
                                    alignment: 'center',
                                    text: [
                                        { text: 'Página ', fontSize: 10 },
                                        { text: page.toString(), fontSize: 10 },
                                        { text: ' de ', fontSize: 10 },
                                        { text: pages.toString(), fontSize: 10 }
                                    ],
                                    margin: [0, 10, 0, 0]
                                }
                            ],
                            margin: [40, 10, 40, 0]
                        };
                    };

                    // Estilo de la tabla
                    const objLayout = {};
                    objLayout['hLineWidth'] = function (i) {
                        // Línea más gruesa para la fila de total
                        return (i === doc.content[2].table.body.length - 2) ? 1 : 0.5;
                    };
                    objLayout['vLineWidth'] = function (i) { return 0.5; };
                    objLayout['hLineColor'] = function (i) { return '#aaa'; };
                    objLayout['vLineColor'] = function (i) { return '#aaa'; };
                    objLayout['paddingLeft'] = function (i) { return 4; };
                    objLayout['paddingRight'] = function (i) { return 4; };
                    doc.content[2].layout = objLayout;
                }
            },
            {
                extend: 'excelHtml5',
                text: '<i class="fas fa-file-excel"></i> Excel',
                titleAttr: 'Exportar a Excel',
                className: 'btn btn-outline-success',
                exportOptions: {
                    columns: [0, 1, 2, 3, 4, 5, 6, 7, 8], // Exportar solo columnas visibles
                }
            }
        ],
        columnDefs: [
            {
                type: 'num',  // Forzar a que la columna se trate como número
                targets: 0    // Aplicar a la primera columna (ID)
            },
            {
                targets: 1, // Columna de fecha
                type: 'date-euro',
                render: function (data) {
                    return '<span class="badge bg-light text-dark">' + data + '</span>';
                }
            },
            {
                targets: [3, 4, 5], // Columnas numéricas
                className: 'text-end'
            },
            {
                targets: 7, // Columna Origen
                render: function (data) {
                    return '<span class="badge bg-primary bg-opacity-10 text-primary">' + data + '</span>';
                }
            },
            {
                targets: 8, // Acciones
                className: 'text-center',
                orderable: false,
                searchable: false
            }
        ],
        initComplete: function () {
        }
    });

    // Eventos
    $('#min, #max').on('change', function () {
        $(this).val() ? $(this).addClass('has-value') : $(this).removeClass('has-value');
    });

    $('#btnFiltrar').click(aplicarFiltros);
    $('#btnReset').click(resetFiltros);
    $('#min-clear, #max-clear').click(limpiarFecha);

    // Funciones
    function aplicarFiltros() {
        var min = $('#min').val();
        var max = $('#max').val();

        if (min || max) {
            $.fn.dataTable.ext.search.push(function (settings, data) {
                var fechaTabla = moment(data[1], 'DD/MM/YYYY');
                var minDate = min ? moment(min, 'DD/MM/YYYY') : null;
                var maxDate = max ? moment(max, 'DD/MM/YYYY') : null;

                return (!minDate || fechaTabla.isSameOrAfter(minDate)) &&
                    (!maxDate || fechaTabla.isSameOrBefore(maxDate));
            });
        }

        table.draw();
        $.fn.dataTable.ext.search.pop();
    }

    function resetFiltros() {
        $('#min, #max').val('').removeClass('has-value');
        table.search('').columns().search('').draw();
    }

    function limpiarFecha() {
        $(this).closest('.input-group').find('input').val('').removeClass('has-value').change();
    }

    return table;
}

// Función para mostrar alerta de permisos
function showPermissionAlert(message) {
    // Cerrar cualquier toast abierto
    $('.toast').toast('hide');

    Swal.fire({
        title: 'Permiso denegado',
        text: message,
        icon: 'warning',
        confirmButtonText: 'Entendido',
        customClass: {
            confirmButton: 'btn btn-warning'
        },
        buttonsStyling: false
    });
}


$(document).ready(function () {
    // Configuración global de manejo de errores AJAX
    $(document).ajaxError(function (event, jqxhr, settings, thrownError) {
        showToast('Ocurrió un error inesperado. Por favor intente nuevamente.');
    });

    // Cargar tabla al inicio
    cargarTablaCobro();
});