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

// Función para mostrar toasts
function showToast(message, type = 'error', duration = 3000) {
    const backgroundColor = type === 'error' ? 'red' : (type === 'success' ? 'green' : 'orange');
    const toast = $(`<div class="toast" style="background-color: ${backgroundColor}; color: white;" role="alert" aria-live="assertive" aria-atomic="true">
                                                                                                                                                                                                                                                                <div class="toast-body">${message}</div>
                                                                                                                                                                                                                                                            </div>`);
    $('#toastContainer').append(toast);
    new bootstrap.Toast(toast[0], { delay: duration }).show();
}

// Función para manejar errores de validación del servidor
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

function initInquilinosDataTable() {
    if ($.fn.DataTable.isDataTable('#tablaInquilinos')) {
        $('#tablaInquilinos').DataTable().destroy();
    }

    const table = $('#tablaInquilinos').DataTable({
        pageLength: 10,
        lengthMenu: [5, 10, 25, 50, 100],
        order: [[0, 'desc']],
        autoWidth: false,
        responsive: true,
        dom: '<"top"lf>Brt<"bottom"ip>',
        buttons: [
            {
                extend: 'pdfHtml5',
                text: '<i class="fas fa-file-pdf"></i> PDF',
                titleAttr: 'Exportar a PDF',
                className: 'btn btn-outline-danger',
                orientation: 'landscape',
                pageSize: 'LETTER',
                exportOptions: {
                    columns: [0, 1, 2, 3, 4, 5, 6, 7], // Selecciona las columnas a exportar
                    modifier: {
                        page: 'all'
                    }
                },
                customize: function (doc) {
                    // Obtener fecha y hora actual
                    const now = new Date();
                    const fechaHora = now.toLocaleDateString('es-RD', {
                        year: 'numeric',
                        month: 'long',
                        day: 'numeric',
                        hour: '2-digit',
                        minute: '2-digit'
                    });

                    // Contar registros (filas visibles después de búsqueda/filtrado)
                    totalRegistros = table.rows({ search: 'applied' }).count();

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
                    columns: [0, 1, 2, 3, 4, 5, 6, 7], // Exportar solo columnas visibles
                }
            }
        ],
        columnDefs: [
            {
                targets: 0,
                visible: false
            },
            {
                targets: [4], // Columna de dirección
                render: function (data, type, row) {
                    if (type === 'display') {
                        return data;
                    }
                    return data;
                }
            },
            {
                targets: [7], // Columna de estado
                orderable: false,
                searchable: true
            },
            {
                targets: [8], // Columna de acciones
                orderable: false,
                searchable: false
            }
        ]
    });

    // Aplicar filtros personalizados
    applyCustomFilters(table);
    applyCedulaMask();
    applyPhoneMasks();
    return table;
}


function applyCustomFilters(table) {
    console.log('Inicializando filtros personalizados...');

    // Filtro por estado - Versión definitiva
    $('#filtroEstado').on('change', function () {
        const value = $(this).val();
        // console.log('Filtro estado cambiado a:', value);

        // Limpiamos cualquier filtro previo
        table.search('').columns().search('').draw();

        if (value !== '') {
            // console.log('Aplicando filtro para estado:', value);

            // Usamos una función de filtrado personalizada
            $.fn.dataTable.ext.search.push(
                function (settings, data, dataIndex) {
                    const row = table.row(dataIndex).node();
                    const estadoText = $(row).find('td:eq(6)').text().trim();
                    // console.log('Texto estado encontrado:', estadoText);

                    if (value === 'true') {
                        return estadoText === 'Activo';
                    } else {
                        return estadoText === 'Inactivo';
                    }
                }
            );

            table.draw();
            // Limpiamos el filtro después de aplicarlo
            $.fn.dataTable.ext.search.pop();
        } else {
            // console.log('Mostrando todos los registros (sin filtro)');
            table.draw();
        }
    });

    // Resto de los filtros
    $('#filtroNombre').on('keyup', function () {
        table.column(1).search(this.value).draw();
    });

    $('#filtroCedula').on('keyup', function () {
        table.column(3).search(this.value).draw();
    });

    $('#btnResetInquilinos').on('click', function () {
        $('#filtroEstado').val('').trigger('change');
        $('#filtroNombre').val('').trigger('keyup');
        $('#filtroCedula').val('').trigger('keyup');
    });

    //console.log('Filtros personalizados inicializados correctamente');
}

$(document).ready(function () {
    $('#cedulaInput').inputmask({
        mask: '999-9999999-9',
        placeholder: '___-_______-_',
        clearIncomplete: true
    });
});

function applyCedulaMask() {
    $('#cedulaInput').inputmask({
        mask: [
            '9{3}-9{7}-9{1}', // Formato de cédula: 950-9193088-9
            'A-9{6}'          // Formato de pasaporte: P-493687
        ],
        placeholder: '',
        clearIncomplete: true,
        definitions: {
            'A': { validator: '[A-Za-z]', cardinality: 1 }, // Define 'A' como una letra
            '9': { validator: '[0-9]', cardinality: 1 }      // Define '9' como un número
        }
    });
}

// Función para aplicar máscaras a los teléfonos
function applyPhoneMasks() {
    $('.telefono-input').inputmask({
        mask: '(999) 999-9999',
        placeholder: '',
        showMaskOnHover: false,
        clearIncomplete: true,
        autoUnmask: true,
        showMaskOnFocus: false,
        onBeforeMask: function (value, opts) {
            return value.replace(/[^0-9]/g, '');
        }
    });
}

// Función para cargar la tabla de inquilinos con manejo de errores
async function cargarTablaInquilinos() {
    try {
        const data = await ajaxRequest({
            url: '/TbInquilinoes/CargarInquilinos',
            type: 'GET',
            errorMessage: 'Error al cargar la lista de inquilinos',
            suppressPermissionToasts: true // Suprimimos toasts para manejar el 403 manualmente
        });

        $('#dataTableContainer').html(data).fadeIn();
        initInquilinosDataTable(); // Re-inicializar el DataTable

    } catch (error) {
        console.error('Error al cargar inquilinos:', error);

        // Manejo específico para error de permisos (403)
        if (error.status === 403 || error.message === 'Permiso denegado') {
            const errorMsg = error.responseJSON?.error || 'No tiene permisos para ver la lista de inquilinos';
            showPermissionAlert(errorMsg);
        }
        // Manejo de otros tipos de errores
        else {
            showToast('Error al cargar la lista de inquilinos', 'error');
        }
    }
}

// Manejador de eventos para el botón "Cargar Inquilinos"
$('#loadInquilinos').on('click', function () {
    cargarTablaInquilinos();
});

// Manejador para crear nuevo inquilino
$('#createInquilino').on('click', async function () {
    try {
        const response = await ajaxRequest({
            url: '/TbInquilinoes/Create',
            type: 'GET',
            errorMessage: 'Error al cargar el formulario de creación',
            suppressPermissionToasts: true // Suprimir toasts para permisos
        });

        // Verificar si es una respuesta de error de permisos
        if (response && response.success === false && response.error) {
            showPermissionAlert(response.error);
            return;
        }

        $('#dataTableContainer').html(response).fadeIn();
        applyCedulaMask();
        applyPhoneMasks();
        initFormValidation($('#dataTableContainer').find('form'));
    } catch (error) {
        if (error.status === 403 || (error.responseJSON && error.responseJSON.error)) {
            // Solo mostrar la alerta, no mostrar toast adicional
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
        } else if (error.message !== 'Permiso denegado') { // Evitar mostrar toast para errores de permiso
            console.error('Error al cargar formulario de creación:', error);
            showToast('Error al cargar el formulario', 'error');
        }
    }
});

// Manejador para editar inquilino
$(document).on('click', '.editInquilino', async function () {
    const id = $(this).data('id');
    try {
        const data = await ajaxRequest({
            url: `/TbInquilinoes/Edit/${id}`,
            type: 'GET',
            errorMessage: 'Error al cargar el formulario de edición',
            suppressPermissionToasts: true
        });

        $('#dataTableContainer').html(data).fadeIn();

        // Aplicar máscaras después de renderizar
        setTimeout(() => {
            applyCedulaMask();
            applyPhoneMasks();
            initFormValidation($('#dataTableContainer').find('form'));

            // Manejar navegación de vuelta
            $('#btnCancelarEdicion').off('click').on('click', async () => {
                await cargarTablaInquilinos();
            });
        }, 100);

    } catch (error) {
        if (error.status === 403) {
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
        } else if (error.status === 404) {
            showToast('Inquilino no encontrado', 'error');
        } else {
            console.error('Error al cargar formulario de edición:', error);
            showToast('Error al cargar el formulario', 'error');
        }
    }
});

// Manejar envío de formulario de edición
$(document).on('submit', 'form[action*="/Edit"]', async function (e) {
    e.preventDefault();
    const form = $(this);
    try {
        const data = await ajaxRequest({
            url: form.attr('action'),
            type: 'POST',
            data: form.serialize(),
            form: form,
            errorMessage: 'Error al editar los datos del inquilino'
        });
        showToast('Inquilino editado correctamente.', 'success');
        if (data.includes('_InquilinosPartial')) {
            await cargarTablaInquilinos();
        } else if (data.success === false && data.errors) {
            // Manejo de errores de validación
            for (const key in data.errors) {
                if (data.errors.hasOwnProperty(key)) {
                    const errorMessage = data.errors[key];
                    const input = form.find(`[name="${key}"]`);

                    // Limpiar mensajes de error anteriores
                    input.removeClass('is-invalid'); // Eliminar clase de error
                    input.next('.invalid-feedback').remove(); // Eliminar mensaje de error anterior

                    input.addClass('is-invalid'); // Agregar clase de error
                    input.after(`<div class="invalid-feedback">${Array.isArray(errorMessage) ? errorMessage.join(' ') : errorMessage}</div>`); // Mostrar mensaje de error
                }
            }
        } else {
            $('#dataTableContainer').html(data);
            applyCedulaMask();
            applyPhoneMasks();
            initFormValidation($('#dataTableContainer').find('form'));
        }
    } catch (error) {
        console.error('Error al editar inquilino:', error);
    }
});


$(document).on('submit', '.cambiarEstadoForm', async function (e) {
    e.preventDefault();
    const form = $(this);

    try {
        // Realizar la petición AJAX
        const response = await ajaxRequest({
            url: form.attr('action'),
            type: 'POST',
            data: form.serialize(),
            errorMessage: 'Error al cambiar el estado del inquilino',
            suppressPermissionToasts: true // Suprimir toasts de permisos
        });

        // Si llegamos aquí, la operación fue exitosa
        await cargarTablaInquilinos();
        showToast('Estado del inquilino actualizado correctamente.', 'success');

    } catch (error) {
        // Manejo específico para errores de permisos (403)
        if (error.status === 403 || error.message === 'Permiso denegado') {
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
        }
        // Manejo para otros tipos de errores
        else {
            console.error('Error al cambiar estado:', error);
            showToast('Error al cambiar el estado del inquilino', 'error');
        }
    }
});

$(document).on('submit', 'form[action$="/Create"]', async function (e) {
    e.preventDefault();
    const form = $(this);
    try {
        const data = await ajaxRequest({
            url: form.attr('action'),
            type: 'POST',
            data: form.serialize(),
            form: form,
            errorMessage: 'Error al crear el inquilino',
            suppressPermissionToasts: false
        });

        // Mostrar toast en cualquier caso de éxito
        showToast('Inquilino creado correctamente.', 'success');
        if (data.includes('_InquilinosPartial')) {
            await cargarTablaInquilinos();
        } else {
            $('#dataTableContainer').html(data);
            applyCedulaMask();
            applyPhoneMasks();
            initFormValidation(form);
        }
    } catch (error) {
        if (error.status === 403) {
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
        } else if (error.status === 400) { // Manejo de errores de validación
            const errors = error.responseJSON.errors;
            for (const key in errors) {
                if (errors.hasOwnProperty(key)) {
                    const errorMessage = errors[key];
                    const input = form.find(`[name="${key}"]`);

                    // Limpiar mensajes de error anteriores
                    input.removeClass('is-invalid'); // Eliminar clase de error
                    input.next('.invalid-feedback').remove(); // Eliminar mensaje de error anterior

                    input.addClass('is-invalid'); // Agregar clase de error
                    input.after(`<div class="invalid-feedback">${Array.isArray(errorMessage) ? errorMessage.join(' ') : errorMessage}</div>`); // Mostrar mensaje de error
                }
            }
        } else {
            console.error('Error al crear inquilino:', error);
            showToast('Error al crear inquilino: ' + (error.responseJSON?.message || error.message), 'error');
        }
    }
});




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


// Función para inicializar validación del lado cliente
function initFormValidation(form) {
    form.validate({
        rules: {
            Fcedula: {
                required: true,
                digits: false
            },
        },
        messages: {
            Fcedula: {
                required: "La cédula es obligatoria",
            }
        },
        errorClass: "is-invalid",
        validClass: "is-valid",
        errorPlacement: function (error, element) {
            error.addClass('invalid-feedback');
            element.after(error);
        }
    });
}



$(document).ready(function () {
    // Configuración global de manejo de errores AJAX
    $(document).ajaxError(function (event, jqxhr, settings, thrownError) {
        showToast('Ocurrió un error inesperado. Por favor intente nuevamente.');
    });

    // Cargar tabla al inicio
    cargarTablaInquilinos();
});