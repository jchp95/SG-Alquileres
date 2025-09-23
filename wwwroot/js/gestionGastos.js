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

/////// Función personalizada de coincidencia para Select2 ///////
function matchCustom(params, data) {
    if ($.trim(params.term) === '') return data;
    if (typeof data.text === 'undefined') return null;

    if (data.text.toLowerCase().indexOf(params.term.toLowerCase()) > -1) {
        var modifiedData = $.extend({}, data, true);
        modifiedData.text += ' (matched)';
        return modifiedData;
    }

    return null;
}

// Configuración del toast (misma configuración que cobros.js)
const Toast = Swal.mixin({
    toast: true,
    position: 'top-end',
    showConfirmButton: false,
    timer: 2000,
    timerProgressBar: true,
    didOpen: (toast) => {
        toast.addEventListener('mouseenter', Swal.stopTimer);
        toast.addEventListener('mouseleave', Swal.resumeTimer);
    }
});

// Función para mostrar toasts (la misma que en inmuebles)
function showToast(message, type = 'error', duration = 3000) {
    const backgroundColor = type === 'error' ? 'red' : (type === 'success' ? 'green' : 'orange');
    const toast = $(`<div class="toast" style="background-color: ${backgroundColor}; color: white;" role="alert" aria-live="assertive" aria-atomic="true">
                                        <div class="toast-body">${message}</div>
                                    </div>`);
    $('#toastContainer').append(toast);
    new bootstrap.Toast(toast[0], { delay: duration }).show();
}

// Función para manejar errores de validación
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

// Manejador para cambiar estado
$(document).on('submit', '.cambiarEstadoForm', async function (e) {
    e.preventDefault();
    const form = $(this);
    try {
        await ajaxRequest({
            url: form.attr('action'),
            type: 'POST',
            data: form.serialize(),
            errorMessage: 'Error al cambiar el estado del gasto',
            suppressPermissionToasts: true
        });

        await cargarTablaGasto();
        Toast.fire({
            icon: 'success',
            title: 'Estado del gasto actualizado correctamente'
        });

    } catch (error) {
        // Manejo específico para errores de permisos (403)
        if (error.status === 403 || error.message === 'Permiso denegado') {
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
        }
        // Manejo para otros tipos de errores
        else {
            console.error('Error al cambiar estado:', error);
            showToast('Error al cambiar el estado del gasto', 'error');
        }
    }
});

async function cargarTablaGasto() {
    try {
        const data = await ajaxRequest({
            url: `/TbGastos/CargarGasto`,
            type: 'GET',
            errorMessage: 'Error al cargar la lista de gastos'
        });

        $('#dataTableContainer').html(data).fadeIn();
        inicializarDataTableGasto();
        inicializarSelectUsuario('gastoTable');
        inicializarSelectTipoGasto('gastoTable');

    } catch (error) {
        if (error.status === 403) {
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
        }
        console.error('Error al cargar los gastos:', error);
        showToast('Error al cargar los gastos: ' + (error.responseJSON?.message || error.message), 'error');
    }
}

$('#loadGastos').on('click', function () {
    cargarTablaGasto();
});

$('#createGasto').on('click', async function () {
    try {
        const data = await ajaxRequest({
            url: '/TbGastos/CreateGasto',
            type: 'GET',
            errorMessage: 'Error al cargar el formulario de creación'
        });

        $('#dataTableContainer').html(data).fadeIn();
        await aplicarMascaraMonto();

        // Inicializar validación del formulario
        initFormValidation($('form[action$="/CreateGasto"]'));
    } catch (error) {
        console.error('Error al cargar formulario de creación:', error);
    }
});

// Manejador para editar inmueble
$(document).on('click', '.editGasto', async function () {
    const id = $(this).data('id');
    try {
        const data = await ajaxRequest({
            url: `/TbGastos/EditGasto/${id}`,
            type: 'GET',
            errorMessage: 'Error al cargar el formulario de edición'
        });

        $('#dataTableContainer').html(data).fadeIn();
        aplicarMascaraMonto();
        initFormValidation($('#dataTableContainer').find('form'));

        // Manejar navegación de vuelta
        $('#btnCancelarEdicion').off('click').on('click', async () => {
            await cargarTablaGasto();
        });

    } catch (error) {
        if (error.status === 403) {
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
        } else if (error.status === 404) {
            showToast('Gasto no encontrado', 'error');
        } else {
            console.error('Error al cargar formulario de edición:', error);
            showToast('Error al cargar el formulario', 'error');
        }
    }
});

// Manejador de envío de formulario de edición
$(document).on('submit', 'form[action*="/EditGasto"]', async function (e) {
    e.preventDefault();
    const form = $(this);
    try {
        const response = await ajaxRequest({
            url: form.attr('action'),
            type: 'POST',
            data: form.serialize(),
            form: form,
            errorMessage: 'Error al editar el gasto',
            dataType: 'json' // Asegurarse de que esperamos JSON
        });

        Toast.fire({
            icon: 'success',
            title: 'Gasto actualizado correctamente'
        });

        if (response.success && response.partialViewUrl) {
            // Cargar la vista parcial desde la URL proporcionada
            const partialView = await $.get(response.partialViewUrl);
            $('#dataTableContainer').html(partialView);
            aplicarMascaraMonto();
            initFormValidation(form);
        }
    } catch (error) {
        console.error('Error al editar el gasto:', error);
    }
});

function inicializarDataTableGasto() {
    // Verificar si la tabla ya está inicializada y destruirla si es necesario
    if ($.fn.DataTable.isDataTable('#gastoTable')) {
        $('#gastoTable').DataTable().destroy();
    }

    // Inicializar la tabla
    const table = $('#gastoTable').DataTable({
        pageLength: 10,
        lengthMenu: [5, 10],
        order: [[1, 'desc']],
        responsive: true,
        dom: '<"top"lf>Brt<"bottom"ip>',
        language: {
            "lengthMenu": "Mostrar _MENU_ registros",
            "emptyTable": "No hay datos disponibles en la tabla",
            "info": "Mostrando _START_ a _END_ de _TOTAL_ registros",
            "infoEmpty": "Mostrando 0 a 0 de 0 registros",
            "infoFiltered": "(filtrado de _MAX_ registros totales)",
            "search": "Buscar:",
            "zeroRecords": "No se encontraron registros coincidentes",
            "paginate": {
                "first": "Primero",
                "last": "Último",
                "next": "Siguiente",
                "previous": "Anterior"
            },
        },
        buttons: [
            {
                extend: 'pdfHtml5',
                text: '<i class="fas fa-file-pdf"></i> PDF',
                className: 'btn btn-outline-danger',
                titleAttr: 'Exportar a PDF',
                orientation: 'landscape',
                pageSize: 'LETTER',
                exportOptions: {
                    columns: [0, 1, 2, 3, 4, 5],
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

                    const table = $('#gastoTable').DataTable();

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
                    doc.pageMargins = [40, 40, 40, 80];
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
                    columns: [0, 1, 2, 3, 4, 5], // Exportar solo columnas visibles
                }
            }
        ],
        columnDefs: [
            {
                targets: [0], //usuario
                visible: false
            },
            {
                targets: [1], // id gasto
            },
            {
                targets: [2], // Columna de Tipo gasto 
                searchable: true
            },
            {
                targets: [3], // Columna de monto
            },
            {
                targets: [4], // Columna de fecha
            },
            {
                targets: [5], // Columna de notas
            }
        ]
    });


    // Filtro por fechas (cliente)
    $('#fechaInicioFiltro, #fechaFinFiltro').on('change', function () {
        table.draw();
    });

    // Filtro personalizado de fechas
    $.fn.dataTable.ext.search.push(function (settings, data, dataIndex) {
        const min = $('#fechaInicioFiltro').val();
        const max = $('#fechaFinFiltro').val();
        const fecha = data[4]; // Columna de fecha (ajustado a la posición correcta)

        if (!min && !max) return true;
        if (!fecha) return false;

        const date = moment(fecha, "DD/MM/YYYY");
        const minDate = min ? moment(min) : null;
        const maxDate = max ? moment(max) : null;

        if (minDate && maxDate) {
            return date.isBetween(minDate, maxDate, null, '[]');
        } else if (minDate) {
            return date.isSameOrAfter(minDate);
        } else if (maxDate) {
            return date.isSameOrBefore(maxDate);
        }

        return true;
    });

    $('#btnResetFiltros').on('click', function () {
        // Limpiar inputs de filtro
        $('#busquedaUsuario').val(null).trigger('change');
        $('#NombreUsuario').val('');
        $('#fechaInicioFiltro').val('');
        $('#fechaFinFiltro').val('');
        $('#tipoGasto').val(null).trigger('change');
        $('#GastoTipo').val('');


        // Resetear filtros del DataTable
        const table = $('#gastoTable').DataTable();
        table.search('').columns().search('').draw();

        // Remover filtro de fechas personalizado
        $.fn.dataTable.ext.search = $.fn.dataTable.ext.search.filter(filtro => {
            return filtro.toString() !== function (settings, data, dataIndex) {
                /* filtro de fechas */
            }.toString();
        });

        table.draw();
    });
}

function inicializarSelectUsuario(nombreTabla) {
    const $selectUsuario = $('#busquedaUsuario');
    const valorSeleccionado = $selectUsuario.val();

    if ($selectUsuario.hasClass('select2-hidden-accessible')) {
        $selectUsuario.select2('destroy');
    }

    $selectUsuario.empty().select2({
        placeholder: "Buscar usuario...",
        allowClear: true,
        matcher: matchCustom
    });

    $.ajax({
        url: '/TbGastos/BuscarUsuario',
        dataType: 'json',
        success: function (data) {
            usuarioData = data;
            $selectUsuario.append(new Option('', '', false, false));

            usuarioData.forEach(item => {
                const option = new Option(item.text, item.id, false, false);
                $selectUsuario.append(option);
            });

            if (valorSeleccionado) {
                $selectUsuario.val(valorSeleccionado).trigger('change');
            }

            $selectUsuario.on('change', function () {
                const usuarioSeleccionado = $(this).val();
                $('#NombreUsuario').val(usuarioSeleccionado);
                const table = $(`#${nombreTabla}`).DataTable();
                table.column(0).search(usuarioSeleccionado).draw();
            });
        },
        error: function () {
            alert('Error al cargar los usuarios.');
        }
    });
}

function inicializarSelectTipoGasto(nombreTabla) {
    const $selectTipoGasto = $('#tipoGasto');
    const valorSeleccionado = $selectTipoGasto.val();

    if ($selectTipoGasto.hasClass('select2-hidden-accessible')) {
        $selectTipoGasto.select2('destroy');
    }

    $selectTipoGasto.empty().select2({
        placeholder: "Buscar tipo de gasto...",
        allowClear: true,
        matcher: matchCustom
    });

    $.ajax({
        url: '/TbGastos/BuscarTipoGasto',
        dataType: 'json',
        success: function (data) {
            tipoGastoData = data;
            $selectTipoGasto.append(new Option('', '', false, false));

            tipoGastoData.forEach(item => {
                const option = new Option(item.text, item.id, false, false);
                $selectTipoGasto.append(option);
            });

            if (valorSeleccionado) {
                $selectTipoGasto.val(valorSeleccionado).trigger('change');
            }

            $selectTipoGasto.on('change', function () {
                const tipoGastoSeleccionado = $(this).val();
                $('#GastoTipo').val(tipoGastoSeleccionado);
                const table = $(`#${nombreTabla}`).DataTable();
                table.column(2).search(tipoGastoSeleccionado).draw();
            });
        },
        error: function () {
            alert('Error al cargar los tipos de gastos.');
        }
    });
}

$(document).on('submit', 'form[action$="/CreateGasto"]', async function (e) {
    e.preventDefault();
    const form = $(this);

    if (!form.data("validator")) {
        initFormValidation(form);
    }

    if (!form.valid()) {
        return false;
    }

    try {
        const data = await ajaxRequest({
            url: form.attr('action'),
            type: 'POST',
            data: form.serialize(),
            form: form,
            errorMessage: 'Error al crear el gasto'
        });

        Toast.fire({
            icon: 'success',
            title: 'Gasto creado correctamente'
        });

        // Siempre cargar la tabla después de crear un gasto
        await cargarTablaGasto();

        // Restablecer el formulario
        form[0].reset();

    } catch (error) {
        if (error.status === 403) {
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
        } else {
            console.error('Error al crear el gasto:', error);
            showToast('Error al crear el gasto: ' + (error.responseJSON?.message || error.message), 'error');
        }
    }
});

// Función para aplicar máscara de precio
function aplicarMascaraMonto() {
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

function initFormValidation(form) {
    return form.validate({
        rules: {
            FkidGastoTipo: {
                required: true,
            },
            Fmonto: {
                required: true,
                number: true,
                min: 0.01
            },
            Fdescripcion: {
                required: true
            },
            Ffecha: {
                required: true
            }
        },
        messages: {
            FkidGastoTipo: {
                required: "Seleccione un tipo de gasto"
            },
            Fmonto: {
                required: "El monto es obligatorio",
                number: "Debe ser un valor numérico",
                min: "El monto debe ser mayor a 0"
            },
            Fdescripcion: {
                required: "La descripción es obligatoria"
            },
            Ffecha: {
                required: "La fecha es obligatoria"
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

async function cargarVistaCrearGasto() {
    try {
        const response = await ajaxRequest({
            url: '/TbGastos/CreateGasto',
            type: 'GET',
            errorMessage: 'Error al cargar el formulario de creación',
            suppressPermissionToasts: true
        });

        if (response && response.success === false && response.error) {
            showPermissionAlert(response.error);
            return;
        }

        $('#dataTableContainer').html(response).fadeIn();

        // Inicializar máscaras y validaciones específicas de gastos
        await aplicarMascaraMonto();
        initFormValidation($('#dataTableContainer').find('form'));

    } catch (error) {
        if (error.status === 403 || (error.responseJSON && error.responseJSON.error)) {
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
        } else if (error.message !== 'Permiso denegado') {
            console.error('Error al cargar formulario de creación de gasto:', error);
            showToast('Error al cargar el formulario de creación', 'error');
        }
    }
}

$(document).ready(async function () {
    // Configuración global de manejo de errores AJAX
    $(document).ajaxError(function (event, jqxhr, settings, thrownError) {
        showToast('Ocurrió un error inesperado. Por favor intente nuevamente.');
    });
});