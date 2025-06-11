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
                    columns: [0, 1, 2, 3, 4, 5, 6], // Selecciona las columnas a exportar
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
                    columns: [0, 1, 2, 3, 4, 5, 6], // Exportar solo columnas visibles
                }
            }
        ],
        columnDefs: [
            {
                targets: 0,
                visible: false
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
        table.column(1).search(this.value).draw();
    });

    // Filtro por ubicación
    $('#filtroDescripcion').on('keyup', function () {
        table.column(2).search(this.value).draw();
    });

    // Filtro por precio máximo
    $('#filtroPrecio').on('keyup', function () {
        const maxPrice = parseFloat(this.value) || 0;
        if (maxPrice > 0) {
            $.fn.dataTable.ext.search.push(
                function (settings, data, dataIndex) {
                    const price = parseFloat(data[5].replace(/[^0-9.]/g, '')) || 0;
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
        $('#filtroDescripcion').val('').trigger('keyup');
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
        console.error('Error al cargar inquilinos:', error);

        // Manejo específico para error de permisos (403)
        if (error.status === 403 || error.message === 'Permiso denegado') {
            const errorMsg = error.responseJSON?.error || 'No tiene permisos para ver la lista de inmuebles';
            showPermissionAlert(errorMsg);
        }
        // Manejo de otros tipos de errores
        else {
            showToast('Error al cargar la lista de inmuebles', 'error');
        }
    }
}


// Manejador para crear nuevo inmueble
$('#createInmueble').on('click', async function () {
    try {
        const data = await ajaxRequest({
            url: '/TbInmuebles/Create',
            type: 'GET',
            errorMessage: 'Error al cargar el formulario de creación'
        });

        $('#dataTableContainer').html(data).fadeIn();

        initMap();

        // Cuando se abra el modal, leer input y actualizar mapa
        $('#mapModal').on('show.bs.modal', function (event) {
            var inputVal = $('#Fubicacion').val();
            var coords = parseCoordinates(inputVal);
            updateMapLocation(coords);
            // Hacer que el mapa se actualice visualmente al abrir modal (necesario para bootstrap modal)
            setTimeout(function () {
                map.invalidateSize();
            }, 200);
        });

        inicializarSelect2();
        aplicarMascaraPrecio();
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

        showToast('Inmueble creado correctamente', 'success');
        if (data.includes('_InmueblesPartial')) {
            await cargarTablaInmuebles();
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
        showToast('Inmueble actualizado correctamente', 'success');
        if (data.includes('_InmueblesPartial')) {
            await cargarTablaInmuebles();
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
        // Manejo específico para errores de permisos (403)
        if (error.status === 403 || error.message === 'Permiso denegado') {
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
        }
        // Manejo para otros tipos de errores
        else {
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
            Fubicacion: {
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
            Fubicacion: {
                required: "La ubicacion es obligatoria"
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

var map;
var marker;

// Inicializar el mapa una sola vez
function initMap() {
    map = L.map('map').setView([0, 0], 2);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '© OpenStreetMap'
    }).addTo(map);
}

// Función para parsear coordenadas del input y validar
function parseCoordinates(input) {
    if (!input) return null;
    var parts = input.split(',');
    if (parts.length !== 2) return null;
    var lat = parseFloat(parts[0].trim());
    var lng = parseFloat(parts[1].trim());
    if (isNaN(lat) || isNaN(lng)) return null;
    if (lat < -90 || lat > 90 || lng < -180 || lng > 180) return null;
    return [lat, lng];
}

// Función para actualizar el mapa y marcador con las coordenadas dadas
function updateMapLocation(coords) {
    if (!coords) {
        // Si coords no válido, centrar en default (0,0) y quitar marcador
        map.setView([0, 0], 2);
        if (marker) {
            map.removeLayer(marker);
            marker = null;
        }
        document.getElementById('selectedLocation').textContent = "Ninguna";
        return;
    }
    var lat = coords[0];
    var lng = coords[1];
    map.setView([lat, lng], 15);
    if (marker) {
        marker.setLatLng([lat, lng]);
    } else {
        marker = L.marker([lat, lng]).addTo(map);
    }
    document.getElementById('selectedLocation').textContent = lat + ", " + lng;
}


$(document).ready(function () {
    // Inicializar el mapa al cargar la página


    // Configuración global de manejo de errores AJAX
    $(document).ajaxError(function (event, jqxhr, settings, thrownError) {
        showToast('Ocurrió un error inesperado. Por favor intente nuevamente.');
    });

    // Cargar tabla al inicio
    cargarTablaInmuebles();
});
