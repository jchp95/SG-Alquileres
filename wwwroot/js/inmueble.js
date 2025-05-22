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
                            const userMessage = error.status === 403 ?
                                'Por favor contacte al administrador' :
                                (options.errorMessage || 'Error de comunicación con el servidor');
                            showToast(userMessage);
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

// Función para inicializar DataTable de inmuebles
function initInmueblesDataTable() {
    if ($.fn.DataTable.isDataTable('#tablaInmuebles')) {
        $('#tablaInmuebles').DataTable().destroy();
    }

    const table = $('#tablaInmuebles').DataTable({
        pageLength: 10,
        lengthMenu: [5, 10, 25, 50, 100],
        order: [[0, 'asc']],
        autoWidth: false,
        responsive: true,
        dom: '<"top"lf>rt<"bottom"ip><"clear">',
        columnDefs: [
            {
                targets: [5], // Columna de estado
                orderable: false,
                searchable: true
            },
            {
                targets: [6], // Columna de acciones
                orderable: false,
                searchable: false
            }
        ]
    });

    applyCustomFilters(table);
    return table;
}

// Función para aplicar filtros personalizados
function applyCustomFilters(table) {
    // Filtro por estado
    $('#filtroEstado').on('change', function () {
        const value = $(this).val();
        table.search('').columns().search('').draw();

        if (value !== '') {
            $.fn.dataTable.ext.search.push(
                function (settings, data, dataIndex) {
                    const row = table.row(dataIndex).node();
                    const estadoText = $(row).find('td:eq(5)').text().trim();
                    return (value === 'true' && estadoText === 'Activo') ||
                        (value === 'false' && estadoText === 'Inactivo');
                }
            );
            table.draw();
            $.fn.dataTable.ext.search.pop();
        } else {
            table.draw();
        }
    });

    // Filtro por propietario
    $('#filtroPropietario').on('keyup', function () {
        table.column(0).search(this.value).draw();
    });

    // Filtro por ubicación
    $('#filtroUbicacion').on('keyup', function () {
        table.column(3).search(this.value).draw();
    });

    // Filtro por precio máximo
    $('#filtroPrecio').on('keyup', function () {
        const maxPrice = parseFloat(this.value) || 0;
        if (maxPrice > 0) {
            $.fn.dataTable.ext.search.push(
                function (settings, data, dataIndex) {
                    const price = parseFloat(data[4].replace(/[^0-9.]/g, '')) || 0;
                    return price <= maxPrice;
                }
            );
            table.draw();
            $.fn.dataTable.ext.search.pop();
        } else {
            table.draw();
        }
    });

    // Botón reset
    $('#btnResetInmuebles').on('click', function () {
        $('#filtroEstado').val('').trigger('change');
        $('#filtroPropietario').val('').trigger('keyup');
        $('#filtroUbicacion').val('').trigger('keyup');
        $('#filtroPrecio').val('').trigger('keyup');
    });
}

// Función para aplicar máscara de precio
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

// Función para inicializar Select2 para propietarios
function inicializarSelect2() {
    const $select = $('#busquedaPropietario');
    const valorSeleccionado = $select.val();

    // Limpiar opciones previas y destruir Select2 si ya existe
    if ($select.hasClass('select2-hidden-accessible')) {
        $select.select2('destroy');
    }

    // Inicializar Select2 (vacío temporalmente)
    $select.empty().select2({
        placeholder: "Buscar propietario...",
        allowClear: true,
        matcher: matchCustom
    });

    $.ajax({
        url: '/TbInmuebles/BuscarPropietario',
        dataType: 'json',
        success: function (data) {
            propietarioData = data.results.filter(item => item.tipo === "propietario")
            $select.append(new Option('', '', false, false)); // Opción vacía

            propietarioData.forEach(item => {
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
        $('#FkidPropietario').val($(this).val());
    });
}

// Función para cargar la tabla de inmuebles
async function cargarTablaInmuebles() {
    try {
        const data = await ajaxRequest({
            url: '/TbInmuebles/CargarInmuebles',
            type: 'GET',
            errorMessage: 'Error al cargar la lista de inmuebles'
        });

        $('#dataTableContainer').html(data).fadeIn();
        initInmueblesDataTable();
    } catch (error) {
        console.error('Error al cargar inmuebles:', error);
    }
}


$('#loadInmuebles').on('click', function () {
    cargarTablaInmuebles();
});

// Manejador para crear nuevo inmueble
$('#createInmueble').on('click', async function () {
    try {
        const data = await ajaxRequest({
            url: '/TbInmuebles/Create',
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

// Manejador para editar inmueble
$(document).on('click', '.editInmueble', async function () {
    const id = $(this).data('id');
    try {
        const data = await ajaxRequest({
            url: `/TbInmuebles/Edit/${id}`,
            type: 'GET',
            errorMessage: 'Error al cargar el formulario de edición'
        });

        $('#dataTableContainer').html(data).fadeIn();
        inicializarSelect2();
        aplicarMascaraPrecio();
        initFormValidation($('#dataTableContainer').find('form'));

        // Manejar navegación de vuelta
        $('#btnCancelarEdicion').off('click').on('click', async () => {
            await cargarTablaInmuebles();
        });
    } catch (error) {
        if (error.status === 403) {
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
        } else if (error.status === 404) {
            showToast('Inmueble no encontrado', 'error');
        } else {
            console.error('Error al cargar formulario de edición:', error);
            showToast('Error al cargar el formulario', 'error');
        }
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
            errorMessage: 'Error al crear el inmueble'
        });

        if (data.includes('_InmueblesPartial')) {
            await cargarTablaInmuebles();
            showToast('Inmueble creado correctamente', 'success');
        } else {
            $('#dataTableContainer').html(data);
            inicializarSelect2();
            aplicarMascaraPrecio();
            initFormValidation(form);
        }
    } catch (error) {
        if (error.status === 403) {
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
        } else {
            console.error('Error al crear el inmueble:', error);
            showToast('Error al crear el inmueble: ' + (error.responseJSON?.message || error.message), 'error');
        }
    }
});

// Manejador de envío de formulario de edición
$(document).on('submit', 'form[action*="/Edit"]', async function (e) {
    e.preventDefault();
    const form = $(this);
    try {
        const data = await ajaxRequest({
            url: form.attr('action'),
            type: 'POST',
            data: form.serialize(),
            form: form,
            errorMessage: 'Error al editar el inmueble'
        });

        if (data.includes('_InmueblesPartial')) {
            await cargarTablaInmuebles();
            showToast('Inmueble actualizado correctamente', 'success');
        } else {
            $('#dataTableContainer').html(data);
            inicializarSelect2();
            aplicarMascaraPrecio();
            initFormValidation(form);
        }
    } catch (error) {
        console.error('Error al editar inmueble:', error);
    }
});

// Manejador para cambiar estado
$(document).on('submit', '.cambiarEstadoForm', async function (e) {
    e.preventDefault();
    const form = $(this);
    try {
        await ajaxRequest({
            url: form.attr('action'),
            type: 'POST',
            data: form.serialize(),
            errorMessage: 'Error al cambiar el estado del inmueble',
            suppressPermissionToasts: true
        });

        await cargarTablaInmuebles();
        showToast('Estado del inmueble actualizado correctamente', 'success');
    } catch (error) {
        if (error.status === 403) {
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
        } else (error.status && error.status !== 403); {
            console.error('Error al cambiar estado:', error);
            showToast('Error al cambiar el estado del inmueble', 'error');
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

// Función para inicializar validación del formulario
function initFormValidation(form) {
    form.validate({
        rules: {
            Fdescripcion: {
                required: true,
                minlength: 10
            },
            Fdireccion: {
                required: true
            },
            Fprecio: {
                required: true,
                number: true,
                min: 1
            },
            FkidPropietario: {
                required: true
            }
        },
        messages: {
            Fdescripcion: {
                required: "La descripción es obligatoria",
                minlength: "La descripción debe tener al menos 10 caracteres"
            },
            Fdireccion: {
                required: "La dirección es obligatoria"
            },
            Fprecio: {
                required: "El precio es obligatorio",
                number: "Debe ser un valor numérico",
                min: "El precio debe ser mayor a 0"
            },
            FkidPropietario: {
                required: "Debe seleccionar un propietario"
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
    cargarTablaInmuebles();
});