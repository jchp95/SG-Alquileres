// Función genérica para llamadas AJAX
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
                        throw error;
                    case 404:
                        showError('Recurso no encontrado');
                        return;
                    default:
                        if (error.responseJSON && error.responseJSON.errors) {
                            if (options.form) {
                                handleValidationErrors($(options.form), error.responseJSON.errors);
                            } else {
                                showError(Object.values(error.responseJSON.errors).flat().join(' '));
                            }
                        } else {
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

function initPropietariosDataTable() {
    if ($.fn.DataTable.isDataTable('#tablaPropietarios')) {
        $('#tablaPropietarios').DataTable().destroy();
    }

    const table = $('#tablaPropietarios').DataTable({
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
                orderable: false,
                searchable: true
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

    // Filtro por estado
    $('#filtroEstado').on('change', function () {
        const value = $(this).val();

        // Limpiamos cualquier filtro previo
        table.search('').columns().search('').draw();

        if (value !== '') {
            // Usamos una función de filtrado personalizada
            $.fn.dataTable.ext.search.push(
                function (settings, data, dataIndex) {
                    const row = table.row(dataIndex).node();
                    const estadoText = $(row).find('td:eq(6)').text().trim();

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

    $('#btnResetPropietarios').on('click', function () {
        $('#filtroEstado').val('').trigger('change');
        $('#filtroNombre').val('').trigger('keyup');
        $('#filtroCedula').val('').trigger('keyup');
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

// Función para cargar la tabla de propietarios
async function cargarTablaPropietarios() {
    try {
        const data = await ajaxRequest({
            url: '/TbPropietarios/CargarPropietarios',
            type: 'GET',
            errorMessage: 'Error al cargar la lista de propietarios'
        });

        // Destruye el DataTable existente si hay uno
        if ($.fn.DataTable.isDataTable('#tablaPropietarios')) {
            $('#tablaPropietarios').DataTable().destroy();
            $('#tablaPropietarios').empty(); // Limpiar contenido previo
        }

        $('#dataTableContainer').html(data).fadeIn();
        initPropietariosDataTable(); // Re-inicializar el DataTable

    } catch (error) {
        console.error('Error al cargar propietarios:', error);
    }
}

// Manejador de eventos para el botón "Cargar Propietarios"
$('#loadPropietarios').on('click', function () {
    cargarTablaPropietarios();
});

// Manejador para crear nuevo propietario
$('#createPropietario').on('click', async function () {
    try {
        const data = await ajaxRequest({
            url: '/TbPropietarios/Create',
            type: 'GET',
            errorMessage: 'Error al cargar el formulario de creación'
        });

        $('#dataTableContainer').html(data).fadeIn();
        applyPhoneMasks();
        initFormValidation($('#dataTableContainer').find('form'));
    } catch (error) {
        console.error('Error al cargar formulario de creación:', error);
    }
});

// Manejador para editar propietrio
$(document).on('click', '.editPropietario', async function () {
    const id = $(this).data('id');
    try {
        const data = await ajaxRequest({
            url: `/TbPropietarios/Edit/${id}`,
            type: 'GET',
            errorMessage: 'Error al cargar el formulario de edición'
        });

        $('#dataTableContainer').html(data).fadeIn();

        // Aplicar máscaras después de renderizar
        setTimeout(() => {
            applyPhoneMasks();
            initFormValidation($('#dataTableContainer').find('form'));

            // Manejar navegación de vuelta
            $('#btnCancelarEdicion').off('click').on('click', async () => {
                await cargarTablaPropietarios();
            });
        }, 100);

    } catch (error) {
        if (error.status === 403) {
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
        } else if (error.status === 404) {
            showToast('Propietario no encontrado', 'error');
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
            errorMessage: 'Error al editar los datos del propietario'
        });

        if (data.includes('_PropietariosPartial')) {
            await cargarTablaPropietarios();
            showToast('Propietario editado correctamente.', 'success');
        } else if (data.includes('_EditPropietarioPartial')) {
            $('#dataTableContainer').html(data);
            applyPhoneMasks();
            initFormValidation($('#dataTableContainer').find('form'));
        } else {
            await cargarTablaPropietarios();
            showToast('Propietario editado correctamente.', 'success');
        }
    } catch (error) {
        console.error('Error al editar propietario:', error);
    }
});

// Manejador de eventos para el botón "Anular/Activar"
$(document).on('submit', '.cambiarEstadoForm', async function (e) {
    e.preventDefault();
    const form = $(this);
    
    try {
        // Realizar la petición AJAX
        const response = await ajaxRequest({
            url: form.attr('action'),
            type: 'POST',
            data: form.serialize(),
            errorMessage: 'Error al cambiar el estado del propietario',
            suppressPermissionToasts: true 
        });

        // Si llegamos aquí, la operación fue exitosa
        await cargarTablaPropietarios();
        showToast('Estado del propietario actualizado correctamente.', 'success');
        
    } catch (error) {
        if (error.status === 403) {
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
        } else (error.status && error.status !== 403); {
            console.error('Error al cambiar estado:', error);
            showToast('Error al cambiar el estado del propietario', 'error');
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
            errorMessage: 'Error al crear el propietario',
            suppressPermissionToasts: false 
        });

       // Mostrar toast en cualquier caso de éxito
       showToast('Propietario creado correctamente.', 'success');
       if (data.includes('_PropietariosPartial')) {
           await cargarTablaPropietarios();
           showToast('Propietario creado correctamente.', 'success');
       } else {
           $('#dataTableContainer').html(data);
           applyPhoneMasks();
           initFormValidation(form);
       }
   } catch (error) {
       if (error.status === 403) {
           showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
       } else {
           console.error('Error al crear propietario:', error);
           showToast('Error al crear propietario: ' + (error.responseJSON?.message || error.message), 'error');
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
            Ftelefono: {
                digits: true
            },
            Fcelular: {
                digits: true
            }
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
    cargarTablaPropietarios();
});