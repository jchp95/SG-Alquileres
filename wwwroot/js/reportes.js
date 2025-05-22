// Función genérica para llamadas AJAX
async function ajaxRequest(config) {
    const defaults = {
        showLoading: true,
        retries: 3,
        retryDelay: 1000,
        suppressPermissionToasts: true // Nueva opción para suprimir toasts de permisos
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

            // Si es el último intento o error no es recuperable
            if (attempts === options.retries ||
                (error.status && [400, 401, 403, 404].includes(error.status))) {

                // Manejo específico de códigos de estado
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
                        throw error; // Asegurarse de que el error se propague
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
                            // Modificado para mostrar mensaje genérico en lugar del error técnico
                            const userMessage = error.status === 403 ?
                                'Por favor contacte al administrador' :
                                (options.errorMessage || 'Error de comunicación con el servidor');
                            showToast(userMessage);
                        }
                        throw error;
                }
            }

            // Esperar antes de reintentar
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

// Inicializar tooltips de Bootstrap
function inicializarTooltips() {
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.forEach(function (tooltipTriggerEl) {
        new bootstrap.Tooltip(tooltipTriggerEl);
    });
}


/////// Función para cargar el reporte CxC ///////
async function cargarReporteCxC() {
    const spinner = document.getElementById("loadingSpinner");
    const container = document.getElementById("dataTableContainer");

    spinner.style.display = "block";
    container.style.display = "none";

    const filtroNombreUsuario = $('#NombreUsuario').val();
    const fechaInicio = $('#fechaInicioFiltro').val();
    const fechaFin = $('#fechaFinFiltro').val();

    const queryParams = new URLSearchParams({
        filtroNombreUsuario: filtroNombreUsuario || '',
        fechaInicio: fechaInicio || '',
        fechaFin: fechaFin || ''
    });

    try {
        const html = await ajaxRequest({
            url: `/Reportes/ReporteCxC?${queryParams}`,
            type: 'GET',
            errorMessage: 'Error al cargar el reporte CxC',
            suppressPermissionToasts: false // Permitir toasts de permisos
        });

        container.innerHTML = html;
        container.style.display = "block";

        inicializarSelect2('reporteCxCTable');
        inicializarTooltips();

        // Inicializar el DataTable con los datos cargados
        const table = $('#reporteCxCTable').DataTable({
            destroy: true,
            responsive: true,
            dom: 'Bfrtip',
            buttons: [
                {
                    extend: 'copyHtml5',
                    text: '<i class="fas fa-copy"></i> Copiar',
                    className: 'btn btn-outline-primary btn-sm'
                },
                {
                    extend: 'excelHtml5',
                    text: '<i class="fas fa-file-excel"></i> Excel',
                    className: 'btn btn-outline-success btn-sm'
                },
                {
                    extend: 'csvHtml5',
                    text: '<i class="fas fa-file-csv"></i> CSV',
                    className: 'btn btn-outline-info btn-sm'
                },
                {
                    extend: 'pdfHtml5',
                    text: '<i class="fas fa-file-pdf"></i> PDF',
                    className: 'btn btn-outline-danger btn-sm',
                    orientation: 'landscape',
                    pageSize: 'A4',
                    exportOptions: {
                        columns: ':visible'
                    }
                },
                {
                    extend: 'print',
                    text: '<i class="fas fa-print"></i> Imprimir',
                    className: 'btn btn-outline-dark btn-sm'
                }
            ],
            columnDefs: [
                {
                    targets: [0], // La columna de usuario (index 0)
                    visible: false, // La ocultamos
                    searchable: true
                }
            ]
        });

        // Filtros rápidos
        $('#filtroInquilino, #filtroInmueble').on('keyup', function () {
            const columnIndex = $(this).attr('id') === 'filtroInquilino' ? 2 : 3;
            table.column(columnIndex).search(this.value).draw();
        });

        // Filtro por fechas (cliente)
        $('#fechaInicioFiltro, #fechaFinFiltro').on('change', function () {
            table.draw();
        });

        // Filtro personalizado de fechas
        $.fn.dataTable.ext.search.push(function (settings, data, dataIndex) {
            const min = $('#fechaInicioFiltro').val();
            const max = $('#fechaFinFiltro').val();
            const fecha = data[8]; // Columna de fecha

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

        // Botón reset (actualizado para limpiar todo correctamente)
        $('#btnResetFiltros').on('click', function () {
            // Limpiar inputs de filtro
            $('#busquedaUsuario').val(null).trigger('change');
            $('#NombreUsuario').val('');
            $('#fechaInicioFiltro').val('');
            $('#fechaFinFiltro').val('');
            $('#filtroInquilino').val('');
            $('#filtroInmueble').val('');

            // Limpiar clases visuales si se usaban
            $('#filtroInquilino, #filtroInmueble').removeClass('has-value');

            // Resetear filtros del DataTable
            const table = $('#reporteCxCTable').DataTable();
            table.search('').columns().search('').draw();

            // Forzar nuevo filtrado sin el filtro de fechas
            $.fn.dataTable.ext.search = $.fn.dataTable.ext.search.filter(function (filtro) {
                // Retiramos específicamente la función de fechas personalizada
                return filtro.name !== 'filtroFechasCxC';
            });

            table.draw();
        });

    } catch (error) {
        if (error.status === 403) { // Evitar mostrar toast si ya se mostró el alert
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
        } else if (error.status === 404) {
            showToast('Reporte no encontrado', 'error');
        }
    }
};

/////// Función para cargar el reporte ///////
async function cargarReporteCobros() {
    const spinner = document.getElementById("loadingSpinner");
    const container = document.getElementById("dataTableContainer");

    spinner.style.display = "block";
    container.style.display = "none";

    const filtroNombreUsuario = $('#NombreUsuario').val();
    const fechaInicio = $('#fechaInicioFiltro').val();
    const fechaFin = $('#fechaFinFiltro').val();

    const queryParams = new URLSearchParams({
        filtroNombreUsuario: filtroNombreUsuario || '',
        fechaInicio: fechaInicio || '',
        fechaFin: fechaFin || ''
    });

    try {
        const html = await ajaxRequest({
            url: `/Reportes/ReporteCobros?${queryParams}`,
            type: 'GET',
            errorMessage: 'Error al cargar el reporte CxC',
            suppressPermissionToasts: false // Permitir toasts de permisos
        });

        container.innerHTML = html;
        container.style.display = "block";

        inicializarSelect2('reporteCxCTable');
        inicializarTooltips();

        // Inicializar el DataTable con los datos cargados
        const table = $('#reporteCobrosTable').DataTable({
            destroy: true,
            responsive: true,
            dom: 'Bfrtip',
            buttons: [
                {
                    extend: 'copyHtml5',
                    text: '<i class="fas fa-copy"></i> Copiar',
                    className: 'btn btn-outline-primary btn-sm'
                },
                {
                    extend: 'excelHtml5',
                    text: '<i class="fas fa-file-excel"></i> Excel',
                    className: 'btn btn-outline-success btn-sm'
                },
                {
                    extend: 'csvHtml5',
                    text: '<i class="fas fa-file-csv"></i> CSV',
                    className: 'btn btn-outline-info btn-sm'
                },
                {
                    extend: 'pdfHtml5',
                    text: '<i class="fas fa-file-pdf"></i> PDF',
                    className: 'btn btn-outline-danger btn-sm',
                    orientation: 'landscape',
                    pageSize: 'A4',
                    exportOptions: {
                        columns: ':visible'
                    }
                },
                {
                    extend: 'print',
                    text: '<i class="fas fa-print"></i> Imprimir',
                    className: 'btn btn-outline-dark btn-sm'
                }
            ],
            columnDefs: [
                {
                    targets: [0], // La columna de usuario (index 0)
                    visible: true, // La ocultamos
                    searchable: true
                }
            ]
        });

        // Filtro rápido por Inquilino (columna 2)
        $('#filtroInquilino').on('keyup', function () {
            table.column(2).search(this.value).draw();
        });

        // Filtro por fechas (cliente)
        $('#fechaInicioFiltro, #fechaFinFiltro').on('change', function () {
            table.draw();
        });

        // Filtro personalizado de fechas
        $.fn.dataTable.ext.search.push(function (settings, data, dataIndex) {
            const min = $('#fechaInicioFiltro').val();
            const max = $('#fechaFinFiltro').val();
            const fecha = data[7]; // Columna de fecha

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

        // Botón reset (actualizado para limpiar todo correctamente)
        $('#btnResetFiltros').on('click', function () {
            // Limpiar inputs de filtro
            $('#busquedaUsuario').val(null).trigger('change');
            $('#NombreUsuario').val('');
            $('#fechaInicioFiltro').val('');
            $('#fechaFinFiltro').val('');
            $('#filtroInquilino').val('');

            // Limpiar clases visuales si se usaban
            $('#filtroInquilino').removeClass('has-value');

            // Resetear filtros del DataTable
            const table = $('#reporteCobrosTable').DataTable();
            table.search('').columns().search('').draw();

            // Forzar nuevo filtrado sin el filtro de fechas
            $.fn.dataTable.ext.search = $.fn.dataTable.ext.search.filter(function (filtro) {
                // Retiramos específicamente la función de fechas personalizada
                return filtro.name !== 'filtroFechasCobros';
            });

            table.draw();
        });

    } catch (error) {
        if (error.status === 403) { // Evitar mostrar toast si ya se mostró el alert
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
        } else if (error.status === 404) {
            showToast('Reporte no encontrado', 'error');
        }
    }
};

/////// Inicializar Select2 y conectar el evento onchange ///////
function inicializarSelect2(nombreTabla) {
    const $select = $('#busquedaUsuario');
    const valorSeleccionado = $select.val();

    if ($select.hasClass('select2-hidden-accessible')) {
        $select.select2('destroy');
    }

    $select.empty().select2({
        placeholder: "Buscar usuario...",
        allowClear: true,
        matcher: matchCustom
    });

    $.ajax({
        url: '/Reportes/BuscarUsuario',
        dataType: 'json',
        success: function (data) {
            usuarioData = data;
            $select.append(new Option('', '', false, false)); // Opción vacía

            usuarioData.forEach(item => {
                const option = new Option(item.text, item.id, false, false);
                $select.append(option);
            });

            if (valorSeleccionado) {
                $select.val(valorSeleccionado).trigger('change');
            }

            $select.select2();
        },
        error: function () {
            alert('Error al cargar los usuarios.');
        }
    });

    $select.on('change', function () {
        const usuarioSeleccionado = $(this).val();
        $('#NombreUsuario').val(usuarioSeleccionado);

        const table = $(`#${nombreTabla}`).DataTable();
        table.column(0).search(usuarioSeleccionado).draw();
    });
}

// Función para mostrar toasts
function showToast(message, type = 'error', duration = 3000) {
    const backgroundColor = type === 'error' ? 'red' : (type === 'success' ? 'green' : 'orange');
    const toast = $(`<div class="toast" style="background-color: ${backgroundColor}; color: white;" role="alert" aria-live="assertive" aria-atomic="true">
                                                                                                                                                                                                                                                                <div class="toast-body">${message}</div>
                                                                                                                                                                                                                                                            </div>`);
    $('#toastContainer').append(toast);
    new bootstrap.Toast(toast[0], { delay: duration }).show();
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



