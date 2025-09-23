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

let tipoGastoEditId = null;

// Modifica la función que carga el formulario de creación
$('#createTipoGasto').on('click', async function () {
    try {
        const data = await ajaxRequest({
            url: '/TbGastos/CargarGastoTipo', // ✅ el método correcto del controlador
            type: 'GET',
            errorMessage: 'Error al cargar el formulario de creación de tipo de gasto'
        });

        $('#dataTableContainer').html(data).fadeIn();
        inicializarDataTable();
        initFormValidation($('#gastoTipoForm'));
    } catch (error) {
        console.error('Error al cargar formulario de creación:', error);
    }
});


// Modificar el manejador del evento submit
$(document).on('submit', '#gastoTipoForm', async function (e) {
    e.preventDefault();
    const form = $(this);

    // Validación del lado del cliente
    if (!form.valid()) {
        form.validate().focusInvalid();
        return false;
    }

    try {
        const formData = new FormData(form[0]);
        const url = tipoGastoEditId ? `/TbGastos/Update/${tipoGastoEditId}` : form.attr('action');
        const method = tipoGastoEditId ? 'POST' : 'POST';

        const response = await ajaxRequest({
            url: url,
            type: method,
            data: formData,
            processData: false,  // Importante para FormData
            contentType: false,  // Importante para FormData
            form: form,
            errorMessage: tipoGastoEditId
                ? 'Error al actualizar el tipo de gasto'
                : 'Error al guardar el tipo de gasto'
        });

        // Si la respuesta es JSON (contiene success)
        if (response && typeof response === 'object' && 'success' in response) {
            if (!response.success) {
                if (response.errors) {
                    handleValidationErrors(form, response.errors);
                } else if (response.message) {
                    showToast(response.message, 'error');
                }
                return;
            }
        }

        // Si es HTML (la partial view)
        $('#dataTableContainer').html(response).fadeIn();
        inicializarDataTable();
        form[0].reset();
        tipoGastoEditId = null;
        showToast(
            tipoGastoEditId
                ? 'Tipo de gasto actualizado exitosamente'
                : 'Tipo de gasto guardado exitosamente',
            'success'
        );
    } catch (error) {
        console.error('Error al guardar:', error);
        if (error.status === 403) {
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
        } else {
            showToast('Error al guardar el tipo de gasto: ' + (error.responseJSON?.message || error.message), 'error');
        }
    }
});

// Modificar la función de edición para inicializar validación
function setupEditButtons() {
    $('.btn-editar-gasto-tipo').off('click').on('click', async function () {
        const id = $(this).data('id');
        try {
            const response = await ajaxRequest({
                url: `/TbGastos/Edit/${id}`,
                type: 'GET',
                errorMessage: 'Error al cargar el tipo de gasto para editar'
            });

            if (response.success) {
                tipoGastoEditId = id;
                $('input[name="Fdescripcion"]').val(response.data.fdescripcion);
                $('#gastoTipoForm').attr('action', `/TbGastos/Update/${id}`);
                $('input[name="Fdescripcion"]').focus();

                // Inicializar validación para el formulario de edición
                initFormValidation($('#gastoTipoForm'));

                showToast('Tipo de gasto cargado para edición', 'info');
            } else {
                showToast(response.message || 'Error al cargar para edición', 'error');
            }
        } catch (error) {
            console.error('Error al editar:', error);
        }
    });
}


async function cargarTablaGastoTipos() {
    try {
        const data = await ajaxRequest({
            url: '/TbGastos/CargarGastoTipo',
            type: 'GET',
            errorMessage: 'Error al cargar la lista de tipos de gastos'
        });

        $('#dataTableContainer').html(data).fadeIn();
        inicializarDataTable();
        setupEditButtons(); // Configurar los botones de editar después de cargar la tabla
    } catch (error) {
        if (error.status === 403) {
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
        }
        console.error('Error al cargar los tipos de gastos:', error);
        showToast('Error al cargar los tipos de gastos: ' + (error.responseJSON?.message || error.message), 'error');
    }
}

function inicializarDataTable() {
    // Verificar si la tabla ya está inicializada y destruirla si es necesario
    if ($.fn.DataTable.isDataTable('#gastoTipoTable')) {
        $('#gastoTipoTable').DataTable().destroy();
    }

    // Inicializar la tabla
    const table = $('#gastoTipoTable').DataTable({
        pageLength: 10,
        lengthMenu: [5, 10],
        order: [[0, 'desc']], // Ordenar por defecto
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
                orientation: 'vertical',
                pageSize: 'LETTER',
                exportOptions: {
                    columns: [0, 1],
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
                    columns: [0, 1], // Exportar solo columnas visibles
                }
            }
        ],
        columnDefs: [
            {
                targets: [0], // id tipo gasto
            },
            {
                targets: [1], // Columna de descripcion 
                searchable: true
            },
            {
                targets: [2], // Columna de acciones
            }
        ]
    });

    setupEditButtons();
}

function initFormValidation(form) {
    // Limpiar validación previa si existe
    if (form.data('validator')) {
        form.validate().destroy();
    }

    form.validate({
        rules: {
            Fdescripcion: {
                required: true,
                minlength: 2
            }
        },
        messages: {
            Fdescripcion: {
                required: "La descripción es obligatoria",
                minlength: "La descripción debe tener al menos 2 caracteres"
            }
        },
        errorElement: 'div',
        errorClass: 'invalid-feedback',
        highlight: function (element) {
            $(element).addClass('is-invalid');
        },
        unhighlight: function (element) {
            $(element).removeClass('is-invalid');
        },
        errorPlacement: function (error, element) {
            error.insertAfter(element);
        }
    });
}

// ✅ Función para cargar la vista de Crear Tipo de Gasto
async function cargarVistaCrearTipoGasto() {
    try {
        const response = await ajaxRequest({
            url: '/TbGastos/CargarGastoTipo', // ✅ EXISTE en tu controlador
            type: 'GET',
            errorMessage: 'Error al cargar el formulario de creación de tipo de gasto',
            suppressPermissionToasts: true
        });

        if (response && response.success === false && response.error) {
            showPermissionAlert(response.error);
            return;
        }

        $('#dataTableContainer').html(response).fadeIn();

        // Inicializar la tabla de tipos de gasto y validación del form
        inicializarDataTable();
        initFormValidation($('#gastoTipoForm'));

        tipoGastoEditId = null;
    } catch (error) {
        if (error.status === 403 || (error.responseJSON && error.responseJSON.error)) {
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
        } else if (error.message !== 'Permiso denegado') {
            console.error('Error al cargar vista de creación de tipo de gasto:', error);
            showToast('Error al cargar el formulario de tipo de gasto', 'error');
        }
    }
}


$(document).ready(function () {
    // Configuración global de manejo de errores AJAX
    $(document).ajaxError(function (event, jqxhr, settings, thrownError) {
        showToast('Ocurrió un error inesperado. Por favor intente nuevamente.');
    });

    // Cargar tabla al inicio
});