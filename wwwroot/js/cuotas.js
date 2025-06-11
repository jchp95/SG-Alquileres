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
                error: null // Deshabilitamos el manejo automático de errores
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
                        showPermissionAlert(errorMessage);

                        // Suprimir toast si está configurado
                        if (options.suppressPermissionToasts) {
                            throw new Error('Permiso denegado'); // Lanzar excepción para detener el flujo
                        }
                        showToast('Por favor contacte al administrador', 'error');
                        throw error;
                    case 404:
                        showToast('Recurso no encontrado');
                        return;
                    default:
                        if (error.responseJSON && error.responseJSON.errors) {
                            if (options.form) {
                                handleValidationErrors($(options.form), error.responseJSON.errors);
                            } else {
                                showToast(Object.values(error.responseJSON.errors).flat().join(' '));
                            }
                        } else {
                            showToast(options.errorMessage || error.responseText || 'Error de comunicación con el servidor');
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

// Función para cargar la tabla de cuentas por cobrar (adaptada de inmuebles)
async function cargarTablaCuotas() {
    try {
        const data = await ajaxRequest({
            url: '/TbCuotas/CargarCuota',
            type: 'GET',
            errorMessage: 'Error al cargar la lista de cuentas por cobrar'
        });

        $('#dataTableContainer').html(data).fadeIn();
        inicializarDataTable();
    } catch (error) {
        if (error.status === 403) {
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
        }
        console.error('Error al cargar cuentas por cobrar:', error);
        showToast('Error al cargar las cuotas: ' + (error.responseJSON?.message || error.message), 'error');
    }
}

// Manejador para crear nuevo inmueble
$('#createCuota').on('click', async function () {
    try {
        const data = await ajaxRequest({
            url: '/TbCuotas/Create',
            type: 'GET',
            errorMessage: 'Error al cargar el formulario de creación'
        });

        $('#dataTableContainer').html(data).fadeIn();
        inicializarSelect2();
        aplicarMascaraPrecio();
        initFormValidation($('#dataTableContainer').find('form'));
    } catch (error) {
        console.error('Error al cargar formulario de creación:', error);
    }
});

// Manejador para crear nuevo inmueble
$('#deleteCuota').on('click', async function () {
    try {
        const data = await ajaxRequest({
            url: '/TbCuotas/Delete',
            type: 'GET',
            errorMessage: 'Error al cargar el formulario para eliminar cuotas'
        });

        $('#dataTableContainer').html(data).fadeIn();
        inicializarSelect2();
        filtrarCuotasPorCxc();
        aplicarMascaraPrecio();
        initFormValidation($('#dataTableContainer').find('form'));
    } catch (error) {
        console.error('Error al cargar formulario para eliminar cuotas:', error);
    }
});

$(document).on('click', '.delete-cuota', function (event) {
    event.preventDefault();
    event.stopPropagation();

    const cuotaId = $(this).data('id');
    console.log('ID de la cuota a eliminar:', cuotaId);

    // Obtener la instancia de DataTable
    const table = $('#tablaCuotasPorCxC').DataTable();
    const allData = table.rows().data().toArray();

    // Encontrar la cuota actual que se quiere eliminar
    const currentCuota = allData.find(c => c.fidCuota === cuotaId);
    if (!currentCuota) return;

    // Ordenar todas las cuotas por fecha (de más antigua a más reciente)
    const sortedCuotas = [...allData].sort((a, b) => new Date(a.fechaCuota) - new Date(b.fechaCuota));

    // Identificar la cuota más antigua (primera en el array ordenado)
    const oldestCuota = sortedCuotas[0];

    // Identificar la cuota más reciente (última en el array ordenado)
    const mostRecentCuota = sortedCuotas[sortedCuotas.length - 1];

    // 1. Validación: No se puede eliminar la cuota más antigua
    if (cuotaId === oldestCuota.fidCuota) {
        Swal.fire(
            'No se puede eliminar',
            'La cuota más antigua no puede ser eliminada.',
            'warning'
        );
        return;
    }

    // 2. Validación: Solo se puede eliminar la cuota más reciente pendiente
    if (cuotaId !== mostRecentCuota.fidCuota) {
        Swal.fire(
            'No se puede eliminar',
            'Solo puede eliminar la cuota más reciente primero.',
            'warning'
        );
        return;
    }

    // 3. Validación adicional: La cuota más reciente debe estar pendiente
    if (mostRecentCuota.estado === 'Pagado') {
        Swal.fire(
            'No se puede eliminar',
            'No puede eliminar cuotas ya pagadas.',
            'warning'
        );
        return;
    }

    // Si pasa todas las validaciones, mostrar confirmación
    Swal.fire({
        title: '¿Estás seguro?',
        text: "¿Deseas eliminar esta cuota pendiente con fecha " + currentCuota.fechaCuota + "?",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar'
    }).then(async (result) => {
        if (result.isConfirmed) {
            try {
                const response = await $.ajax({
                    url: `/TbCuotas/Delete/${cuotaId}`,
                    type: 'DELETE',
                    contentType: 'application/json',
                    headers: {
                        'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                    }
                });

                if (response.success) {
                    // Encontrar y eliminar la fila usando el ID original (cuotaId)
                    table.rows().every(function () {
                        const rowData = this.data();
                        if (rowData.fidCuota == cuotaId) {
                            this.remove().draw();
                            return false; // Salir del bucle
                        }
                    });

                    Swal.fire(
                        'Eliminado!',
                        'La cuota ha sido eliminada correctamente.',
                        'success'
                    );
                } else {
                    Swal.fire(
                        'Error!',
                        response.message || 'No se pudo eliminar la cuota.',
                        'error'
                    );
                }
            } catch (error) {
                console.error('Error al eliminar la cuota:', error);
                Swal.fire(
                    'Error!',
                    'Ocurrió un error al intentar eliminar la cuota.',
                    'error'
                );
            }
        }
    });
});

function filtrarCuotasPorCxc() {
    console.log('Configurando filtrado de cuotas por CxC...');

    $('#busquedaCxC').off('change').on('change', function () {
        const cuentaId = $(this).val();
        console.log('Cambio en selección de CxC - ID seleccionado:', cuentaId);

        if (cuentaId) {
            console.log('Solicitando cuotas para CxC:', cuentaId);

            $.ajax({
                url: '/TbCuotas/ObtenerCuotasPorCxC',
                type: 'GET',
                data: { cuentaId: cuentaId },
                success: function (data) {
                    console.log('Respuesta recibida para cuotas:', data);
                    if (data.success) {
                        console.log('Datos de cuotas válidos recibidos');

                        // Destruir DataTable existente
                        if ($.fn.DataTable.isDataTable('#tablaCuotasPorCxC')) {
                            console.log('Destruyendo DataTable existente...');
                            $('#tablaCuotasPorCxC').DataTable().clear().destroy();
                        }

                        $('#tablaCuotasPorCxC tbody').empty();
                        console.log('Contenido de tabla limpiado');

                        if (data.cuotas?.length > 0) {
                            console.log('Creando nueva DataTable con', data.cuotas.length, 'cuotas');

                            $('#tablaCuotasPorCxC').DataTable({
                                data: data.cuotas.filter(c => c.fstatus === 'N'), // Filtrar cuotas en el frontend
                                columns: [
                                    {
                                        data: 'fidCuota',
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
                                            return `${dia}/${mes}/${anio}`; // Formato dd/mm/yyyy
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
                                    },
                                    {
                                        data: null,
                                        render: function (data, type, row) {
                                            return `<button class="btn btn-sm btn-outline-danger delete-cuota" data-id="${row.fidCuota}" title="Eliminar">
                                                        <i class="fas fa-trash"></i>
                                                    </button>`;
                                        }
                                    }
                                ],
                                responsive: true,
                                drawCallback: function () {
                                    console.log('DataTable dibujada - configurando checkboxes');
                                }
                            });

                            aplicarMascaraPrecio();
                        } else {
                            console.warn('No hay cuotas para mostrar');
                            $('#tablaCuotasPorCxC tbody').append('<tr><td colspan="7" class="text-center">No hay cuotas para esta cuenta.</td></tr>');
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



$(document).on('submit', 'form[action$="/Create"]', async function (e) {
    e.preventDefault();
    const form = $(this);

    if (!form.valid()) {
        showToast('Por favor complete todos los campos requeridos correctamente', 'error');
        return;
    }

    try {
        // Crear objeto con los datos del formulario
        const formData = {
            FidCxc: parseInt($('#busquedaCxC').val()), // Asegurar que es número
            FNumeroCuota: parseInt($('#FNumeroCuota').val()),
            CantidadCuotas: parseInt($('#CantidadCuotas').val()),
            Fvence: $('#Fvence').val(),
            Fmonto: parseFloat($('#Fmonto').val()),
            TasaMora: parseFloat($('#Fmora').val())
        };

        console.log('Datos procesados:', formData);

        const response = await $.ajax({
            url: form.attr('action'),
            type: 'POST',
            data: JSON.stringify(formData),
            contentType: 'application/json', // Especificar que enviamos JSON
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val(),
                'Accept': 'application/json' // Aceptar respuesta JSON
            }
        });

        console.log('Respuesta recibida:', response);

        if (response.success) {
            // Cargar la tabla de cuotas y mostrar mensaje de éxito
            await cargarTablaCuotas(); // Cambiado de cargarTablaCuentasPorCobrar() a cargarTablaCuotas()
            showToast(response.message || 'Cuota(s) creada(s) correctamente.', 'success');
            form[0].reset();
            $('#busquedaCxC').val(null).trigger('change');
            $('#FNumeroCuota').val(response.nextCuotaNumber || '');
        } else {
            showToast(response.message || 'Error al crear la(s) cuota(s)', 'error');
            if (response.errors) {
                console.error('Errores de validación:', response.errors);
            }
        }
    } catch (error) {
        console.error('Error en la solicitud:', error);
        showToast('Ocurrió un error al procesar la solicitud', 'error');
    } finally {
        $('#FNumeroCuota').prop('disabled', true);
    }
});

function aplicarMascaraPrecio() {
    $('.numeric-input').inputmask('remove');

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
}

/////// Funcion para inicializar Match Custom Inmueble ///////
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
        url: '/TbCuotas/BuscarCxc',
        dataType: 'json',
        success: function (data) {
            cxCData = data.results.filter(item => item.tipo === "cuenta por cobrar");

            $select.append(new Option('', '', false, false)); // Opción vacía

            cxCData.forEach(item => {
                const option = new Option(item.text, item.id, false, false);

                $(option).data('numeroCuota', item.numeroCuota); // Almacenar el número de cuota
                // Convertir la fecha al formato YYYY-MM-DD para el input date
                if (item.fechaVencimiento) {
                    const fecha = new Date(item.fechaVencimiento);
                    const fechaFormateada = fecha.toISOString().split('T')[0];
                    $(option).data('fechaVencimiento', fechaFormateada);
                }

                $(option).data('monto', item.monto); // Almacenar el monto
                $(option).data('mora', item.mora); // Almacenar el monto
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

    // Manejar el evento change para cargar los datos en los campos correspondientes
    $select.on('change', function () {
        const selectedOption = $(this).find('option:selected');
        const numeroCuota = selectedOption.data('numeroCuota');
        const fechaVencimiento = selectedOption.data('fechaVencimiento');
        const monto = selectedOption.data('monto');
        const mora = selectedOption.data('mora');

        console.log("Número de cuota obtenida:", numeroCuota);
        $('#FNumeroCuota').val(numeroCuota); // Establecer el Número de Cuota

        console.log("Fecha de vencimiento obtenida:", fechaVencimiento);
        $('#Fvence').val(fechaVencimiento); // Establecer la Fecha de Vencimiento

        console.log("Monto obtenido:", monto);
        $('#Fmonto').val(monto); // Establecer el Monto

        console.log("Mora obtenida:", mora);
        $('#Fmora').val(mora); // Establecer el Monto

        $('#FidCxc').val($(this).val()); // Asegúrate de que el ID de la Cxc también se establezca
    });
}


// Helpers
function inicializarDataTable() {
    // Verificar si la tabla ya está inicializada y destruirla si es necesario
    if ($.fn.DataTable.isDataTable('#cuotaTable')) {
        $('#cuotaTable').DataTable().destroy();
    }

    // Inicializar la tabla
    const table = $('#cuotaTable').DataTable({
        pageLength: 10,
        lengthMenu: [5, 10, 25, 50, 100],
        order: [[0, 'desc']], // Ordenar por Inquilino por defecto
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
                    columns: [0, 1, 2, 3, 4, 5, 6],
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

                    // Encontrar los índices de las columnas necesarias
                    let montoIndex = -1;
                    let saldoIndex = -1;

                    table.columns().every(function () {
                        const headerText = this.header().textContent.trim();
                        if (headerText === 'Monto') {
                            montoIndex = this.index();
                        } else if (headerText === 'Saldo') {
                            saldoIndex = this.index();
                        }
                    });

                    // Calcular el total de la columna Monto
                    let totalMonto = 0;
                    let totalSaldo = 0;
                    let totalRegistros = 0;

                    if (montoIndex !== -1) {
                        table.rows({ search: 'applied' }).data().each(function (row) {
                            const montoStr = row[montoIndex].toString();
                            const montoLimpio = montoStr.replace(/[^\d.-]/g, '');
                            const montoNum = parseFloat(montoLimpio) || 0;
                            totalMonto += montoNum;
                        });
                    }

                    // Calcular el total de la columna Saldo
                    if (saldoIndex !== -1) {
                        table.rows({ search: 'applied' }).data().each(function (row) {
                            const saldoStr = row[saldoIndex].toString();
                            const saldoLimpio = saldoStr.replace(/[^\d.-]/g, '');
                            const saldoNum = parseFloat(saldoLimpio) || 0;
                            totalSaldo += saldoNum;
                        });
                    }

                    // Contar registros (filas visibles después de búsqueda/filtrado)
                    totalRegistros = table.rows({ search: 'applied' }).count();

                    // Formatear los totales al formato RD
                    const totalMontoFormateado = 'RD$ ' + formatoRD(totalMonto);
                    const totalSaldoFormateado = 'RD$ ' + formatoRD(totalSaldo);

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
                        totalRow[3] = { text: totalMontoFormateado, bold: true, alignment: 'right' };

                        // Colocar el saldo total en la columna Saldo (ajusta el índice según la posición)
                        totalRow[4] = { text: totalSaldoFormateado, bold: true, alignment: 'right' };

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
                    columns: [0, 1, 2, 3, 4, 5, 6], // Exportar solo columnas visibles
                }
            }
        ],
        columnDefs: [
            {
                targets: [0, 1],
                className: 'text-center'
            },
            {
                targets: [2], // Columna de Fechas 
                className: 'text-center',
                type: 'date-euro', // Tipo especial para ordenamiento de fechas
                searchable: true
            },
            {
                targets: [3, 4], // Columna de Monto y Saldo
                orderable: false,
                searchable: true
            },
            {
                targets: [5], // Columna de mora
                orderable: false,
                searchable: false
            },
            {
                targets: [6], // Columna de Estado
                orderable: false,
                searchable: true
            }
        ]
    });
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

// Función para inicializar validación del formulario
function initFormValidation(form) {
    form.validate({
        rules: {
            CantidadCuotas: {
                required: true,
            },
            Fmonto: {
                required: true
            }
        },
        messages: {
            CantidadCuotas: {
                required: "La cantidad de cuotas es obligatorio",
                number: "Debe ser un valor numérico mayor que cero",
            },
            Fmonto: {
                required: "El monto es obligatorio"
            }
        },
        errorClass: "is-invalid",
        validClass: "is-valid",
        errorPlacement: function (error, element) {
            error.addClass('invalid-feedback');
            element.after(error);
        },
        highlight: function (element, errorClass, validClass) {
            $(element).addClass(errorClass).removeClass(validClass);
        },
        unhighlight: function (element, errorClass, validClass) {
            $(element).removeClass(errorClass).addClass(validClass);
        }
    });
}

$(document).ready(function () {
    // Configuración global de manejo de errores AJAX
    $(document).ajaxError(function (event, jqxhr, settings, thrownError) {
        showToast('Ocurrió un error inesperado. Por favor intente nuevamente.');
    });

    // Cargar tabla al inicio
    cargarTablaCuotas();
});