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
        order: [[0, 'asc']],
        autoWidth: false,
        responsive: true,
        dom: '<"top"lf>rt<"bottom"ip><"clear">',
        columnDefs: [
            {
                targets: [3], // Columna de dirección
                render: function (data, type, row) {
                    if (type === 'display') {
                        return data;
                    }
                    return data;
                }
            },
            {
                targets: [6], // Columna de estado
                orderable: false, // Asegúrate de que no sea ordenable si no es necesario
                searchable: true // Si deseas que sea filtrable
            },
            {
                targets: [7], // Columna de acciones
                orderable: false,
                searchable: false
            }
        ]
    });

    // Aplicar filtros personalizados
    applyCustomFilters(table);
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
        table.column(0).search(this.value).draw();
    });

    $('#filtroCedula').on('keyup', function () {
        table.column(2).search(this.value).draw();
    });

    $('#btnResetInquilinos').on('click', function () {
        $('#filtroEstado').val('').trigger('change');
        $('#filtroNombre').val('').trigger('keyup');
        $('#filtroCedula').val('').trigger('keyup');
    });

    //console.log('Filtros personalizados inicializados correctamente');
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

// Función para cargar la tabla de inquilinos
async function cargarTablaInquilinos() {
    try {
        const data = await ajaxRequest({
            url: '/TbInquilinoes/CargarInquilinos',
            type: 'GET',
            errorMessage: 'Error al cargar la lista de inquilinos'
        });

        $('#dataTableContainer').html(data).fadeIn();
        initInquilinosDataTable(); // Re-inicializar el DataTable
    } catch (error) {
        console.error('Error al cargar inquilinos:', error);
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
        applyPhoneMasks();
        initFormValidation($('#dataTableContainer').find('form'));
    } catch (error) {
        if (error.status === 403 || (error.responseJSON && error.responseJSON.error)) {
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
        } else {
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

        if (data.includes('_InquilinosPartial')) {
            await cargarTablaInquilinos();
            showToast('Inquilino editado correctamente.', 'success');
        } else if (data.includes('_EditinquilinoPartial')) {
            $('#dataTableContainer').html(data);
            applyPhoneMasks();
            initFormValidation($('#dataTableContainer').find('form'));
        } else {
            await cargarTablaInquilinos();
            showToast('Inquilino editado correctamente.', 'success');
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
        if (error.status === 403) {
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
        // Ignorar errores 403 (ya se mostró showPermissionAlert)
        } else (error.status && error.status !== 403); {
            console.error('Error al cambiar estado:', error);
            showToast('Error al cambiar el estado del inquilino', 'error');
        }
        // Para errores 403 no hacemos nada adicional
    }
});

// Manejador de envío de formulario de creación
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
            showToast('Inquilino creado correctamente.', 'success');
        } else {
            $('#dataTableContainer').html(data);
            applyPhoneMasks();
            initFormValidation(form);
        }
    } catch (error) {
        if (error.status === 403) {
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
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
                digits: true
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