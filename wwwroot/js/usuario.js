// --- Botones seleccionar todos (general y por categoría) ---
// Botón general seleccionar/deseleccionar todos los permisos
$(document).on('click', '.master-permission-btn-usuario', function () {
    const totalCheckboxes = $('.permission-switch-usuario').length;
    const checkedCheckboxes = $('.permission-switch-usuario:checked').length;
    const isAllChecked = checkedCheckboxes === totalCheckboxes;
    $('.permission-switch-usuario').prop('checked', !isAllChecked).trigger('change');
    // Actualizar texto
    $(this).html(`<i class="fas fa-check-double"></i> ${!isAllChecked ? 'Deseleccionar' : 'Seleccionar'} todos los permisos`);
    // Actualizar los botones de cada categoría
    $('.select-all-btn-usuario').each(function () {
        const container = $(this).closest('.tab-pane').find('.permission-items-container-usuario');
        const checkboxes = container.find('.permission-switch-usuario');
        const allChecked = checkboxes.length === checkboxes.filter(':checked').length;
        $(this).html(`<i class="fas fa-${allChecked ? 'times' : 'check'}-circle"></i> ${allChecked ? 'Deseleccionar' : 'Seleccionar'} todos`);
    });
});

// Botón seleccionar/deseleccionar todos por categoría
$(document).on('click', '.select-all-btn-usuario', function () {
    const container = $(this).closest('.tab-pane').find('.permission-items-container-usuario');
    const checkboxes = container.find('.permission-switch-usuario');
    const allChecked = checkboxes.length === checkboxes.filter(':checked').length;
    checkboxes.prop('checked', !allChecked).trigger('change');
    // Actualizar texto
    $(this).html(`<i class="fas fa-${!allChecked ? 'times' : 'check'}-circle"></i> ${!allChecked ? 'Deseleccionar' : 'Seleccionar'} todos`);
    // Actualizar el botón general
    const totalCheckboxes = $('.permission-switch-usuario').length;
    const checkedCheckboxes = $('.permission-switch-usuario:checked').length;
    const isAllChecked = checkedCheckboxes === totalCheckboxes;
    $('.master-permission-btn-usuario').html(`<i class="fas fa-check-double"></i> ${isAllChecked ? 'Deseleccionar' : 'Seleccionar'} todos los permisos`);
});

// Actualizar texto de botones al cambiar cualquier checkbox
$(document).on('change', '.permission-switch-usuario', function () {
    // Por categoría
    $('.tab-pane').each(function () {
        const container = $(this).find('.permission-items-container-usuario');
        const checkboxes = container.find('.permission-switch-usuario');
        const allChecked = checkboxes.length > 0 && checkboxes.length === checkboxes.filter(':checked').length;
        $(this).find('.select-all-btn-usuario').html(`<i class="fas fa-${allChecked ? 'times' : 'check'}-circle"></i> ${allChecked ? 'Deseleccionar' : 'Seleccionar'} todos`);
    });
    // General
    const totalCheckboxes = $('.permission-switch-usuario').length;
    const checkedCheckboxes = $('.permission-switch-usuario:checked').length;
    const isAllChecked = checkedCheckboxes === totalCheckboxes;
    $('.master-permission-btn-usuario').html(`<i class="fas fa-check-double"></i> ${isAllChecked ? 'Deseleccionar' : 'Seleccionar'} todos los permisos`);
});
// Diccionario de descripciones breves para los permisos
const PERMISSION_DESCRIPTIONS = {
    // Inquilinos
    "Permissions.Inquilinos.Ver": "Permite visualizar la lista y detalles de los inquilinos registrados.",
    "Permissions.Inquilinos.Crear": "Permite agregar nuevos inquilinos al sistema.",
    "Permissions.Inquilinos.Editar": "Permite modificar la información de los inquilinos.",
    "Permissions.Inquilinos.Anular": "Permite anular inquilinos, pero no eliminarlos definitivamente.",
    // Propietarios
    "Permissions.Propietarios.Ver": "Permite ver la lista y detalles de propietarios.",
    "Permissions.Propietarios.Crear": "Permite registrar nuevos propietarios.",
    "Permissions.Propietarios.Editar": "Permite editar la información de propietarios.",
    "Permissions.Propietarios.Anular": "Permite anular propietarios, pero no eliminarlos definitivamente.",
    // Inmuebles
    "Permissions.Inmuebles.Ver": "Permite ver la lista y detalles de inmuebles.",
    "Permissions.Inmuebles.Crear": "Permite agregar nuevos inmuebles.",
    "Permissions.Inmuebles.Editar": "Permite editar la información de los inmuebles.",
    "Permissions.Inmuebles.Anular": "Permite anular inmuebles, pero no eliminarlos definitivamente.",
    // CxC
    "Permissions.CxC.Ver": "Permite ver cuentas por cobrar y sus detalles.",
    "Permissions.CxC.Crear": "Permite crear nuevas cuentas por cobrar.",
    "Permissions.CxC.Editar": "Permite editar cuentas por cobrar existentes.",
    "Permissions.CxC.Anular": "Permite anular cuentas por cobrar, pero no eliminarlas definitivamente.",
    "Permissions.CxC.Cancelar": "Permite cancelar cuentas por cobrar.",
    // Cuotas
    "Permissions.Cuotas.Ver": "Permite ver cuotas generadas.",
    "Permissions.Cuotas.Crear": "Permite crear nuevas cuotas.",
    "Permissions.Cuotas.Eliminar": "Permite eliminar cuotas existentes.",
    // Cobros
    "Permissions.Cobros.Ver": "Permite ver la lista de cobros realizados.",
    "Permissions.Cobros.VerEstadoCobro": "Permite ver el estado de los cobros.",
    "Permissions.Cobros.Crear": "Permite registrar nuevos cobros.",
    "Permissions.Cobros.VerDetalles": "Permite ver detalles específicos de un cobro.",
    "Permissions.Cobros.Anular": "Permite anular cobros realizados.",
    // Gastos
    "Permissions.Gastos.Ver": "Permite ver la lista y detalles de gastos.",
    "Permissions.Gastos.Crear": "Permite registrar nuevos gastos.",
    "Permissions.Gastos.Editar": "Permite editar información de gastos.",
    "Permissions.Gastos.Anular": "Permite anular gastos, pero no eliminarlos definitivamente.",
    // Reportes
    "Permissions.Reportes.Ver": "Permite acceder y visualizar reportes del sistema.",
    // Home
    "Permissions.Home.Ver": "Permite acceder al dashboard principal.",
    // Usuarios
    "Permissions.Usuario.Ver": "Permite ver la lista y detalles de usuarios.",
    "Permissions.Usuario.Crear": "Permite crear nuevos usuarios en el sistema.",
    "Permissions.Usuario.Editar": "Permite editar la información de los usuarios.",
    "Permissions.Usuario.Anular": "Permite anular usuarios, pero no eliminarlos definitivamente."
};
// Diccionario de nombres amigables para los permisos
const PERMISSION_DISPLAY_NAMES = {
    // Inquilinos
    "Permissions.Inquilinos.Ver": "Ver Inquilinos",
    "Permissions.Inquilinos.Crear": "Crear Inquilinos",
    "Permissions.Inquilinos.Editar": "Editar Inquilinos",
    "Permissions.Inquilinos.Anular": "Anular Inquilinos",
    // Propietarios
    "Permissions.Propietarios.Ver": "Ver Propietarios",
    "Permissions.Propietarios.Crear": "Crear Propietarios",
    "Permissions.Propietarios.Editar": "Editar Propietarios",
    "Permissions.Propietarios.Anular": "Anular Propietarios",
    // Inmuebles
    "Permissions.Inmuebles.Ver": "Ver Inmuebles",
    "Permissions.Inmuebles.Crear": "Crear Inmuebles",
    "Permissions.Inmuebles.Editar": "Editar Inmuebles",
    "Permissions.Inmuebles.Anular": "Anular Inmuebles",
    // CxC
    "Permissions.CxC.Ver": "Ver Cuentas por Cobrar",
    "Permissions.CxC.Crear": "Crear Cuentas por Cobrar",
    "Permissions.CxC.Editar": "Editar Cuentas por Cobrar",
    "Permissions.CxC.Anular": "Anular Cuentas por Cobrar",
    "Permissions.CxC.Cancelar": "Cancelar Cuentas por Cobrar",
    // Cuotas
    "Permissions.Cuotas.Ver": "Ver Cuotas",
    "Permissions.Cuotas.Crear": "Crear Cuotas",
    "Permissions.Cuotas.Eliminar": "Eliminar Cuotas",
    // Cobros
    "Permissions.Cobros.Ver": "Ver Cobros",
    "Permissions.Cobros.VerEstadoCobro": "Ver Estado de Cobro",
    "Permissions.Cobros.Crear": "Crear Cobros",
    "Permissions.Cobros.VerDetalles": "Ver Detalles de Cobro",
    "Permissions.Cobros.Anular": "Anular Cobros",
    // Gastos
    "Permissions.Gastos.Ver": "Ver Gastos",
    "Permissions.Gastos.Crear": "Crear Gastos",
    "Permissions.Gastos.Editar": "Editar Gastos",
    "Permissions.Gastos.Anular": "Anular Gastos",
    // Reportes
    "Permissions.Reportes.Ver": "Ver Reportes",
    // Home
    "Permissions.Home.Ver": "Ver Dashboard",
    // Usuarios
    "Permissions.Usuario.Ver": "Ver Usuarios",
    "Permissions.Usuario.Crear": "Crear Usuarios",
    "Permissions.Usuario.Editar": "Editar Usuarios",
    "Permissions.Usuario.Anular": "Anular Usuarios"
};
// Cargar y renderizar todos los permisos en la card de crear usuario (estilo administrarPermisos.js)
async function loadAllPermissionsUsuario() {
    // Limpia contenedores
    $('.permission-items-container-usuario').empty();
    try {
        const data = await $.get('/Permisos/GetAllPermissions');
        // data.permissionCategories: { [categoria]: [ { value, displayName } ] }
        $.each(data.permissionCategories, function (category, permissions) {
            const container = $(`#content-usuario-${category} .permission-items-container-usuario`);
            const actionsContainer = $(`#content-usuario-${category} .category-actions-usuario`);
            if (container.length === 0) return;
            // Botón seleccionar todos por categoría
            actionsContainer.html(`
                <button type="button" class="btn btn-sm btn-outline-primary select-all-btn-usuario mb-2" data-category="${category}">
                    <i class="fas fa-check-circle"></i> Seleccionar todos
                </button>
            `);
            permissions.forEach(permission => {
                const switchId = `perm-usuario-${permission.value.replace(/\./g, '-')}`;
                const displayName = PERMISSION_DISPLAY_NAMES[permission.value] || permission.displayName;
                const description = PERMISSION_DESCRIPTIONS[permission.value] || '';
                container.append(`
                    <div class="permission-item">
                        <div class="form-check form-switch">
                            <input type="checkbox" class="form-check-input permission-switch-usuario" 
                                   id="${switchId}" 
                                   data-permission="${permission.value}">
                            <label class="form-check-label" for="${switchId}">
                                <i class="fas ${getPermissionIconUsuario(permission.value)}"></i>
                                ${displayName}
                            </label>
                        </div>
                        <div class="permission-description">${description}</div>
                    </div>
                `);
            });
        });
    } catch (err) {
        showToast('Error al cargar permisos', 'error');
    }
}

async function ajaxRequest(config) {
    console.log("🔍 Iniciando petición AJAX:", {
        url: config.url,
        type: config.type,
        data: config.data,
        headers: config.headers
    });

    const defaults = {
        showLoading: true,
        retries: 3,
        retryDelay: 1000,
        suppressPermissionToasts: true,
        includeAntiForgeryToken: true,
        dataType: 'json' // Asegurar que esperamos JSON
    };

    const options = { ...defaults, ...config };
    let attempts = 0;

    // Obtener el token antiforgery
    let antiForgeryToken = '';
    if (options.includeAntiForgeryToken && (options.type === 'POST' || options.type === 'PUT' || options.type === 'DELETE')) {
        antiForgeryToken = $('input[name="__RequestVerificationToken"]').val();
        console.log("🔐 Token antiforgery:", antiForgeryToken ? "✅ Presente" : "❌ No encontrado");
    }

    while (attempts < options.retries) {
        try {
            if (options.showLoading) {
                $('#loadingSpinner').show();
            }

            const ajaxOptions = {
                ...options,
                error: null
            };

            if (antiForgeryToken) {
                ajaxOptions.headers = {
                    ...ajaxOptions.headers,
                    'X-RequestVerificationToken': antiForgeryToken
                };
            }

            console.log("📤 Enviando petición con opciones:", ajaxOptions);
            const response = await $.ajax(ajaxOptions);
            console.log("✅ Respuesta exitosa:", response);
            return response;

        } catch (error) {
            attempts++;
            console.error(`❌ Error en intento ${attempts}:`, error);

            // Si es un error de parseo JSON pero el status es 200, probablemente recibimos HTML en lugar de JSON
            if (error.status === 200 && error.responseText && error.responseText.includes('<!DOCTYPE')) {
                console.error("⚠️  El servidor devolvió HTML en lugar de JSON");
                throw new Error("Formato de respuesta inesperado. Contacte al administrador.");
            }

            if (attempts === options.retries || (error.status && [400, 401, 403, 404].includes(error.status))) {
                switch (error.status) {
                    case 400:
                        console.log("🛑 Error 400 - Bad Request:", error.responseJSON);
                        if (error.responseJSON?.errors) {
                            console.log("📋 Errores de validación:", error.responseJSON.errors);
                            if (options.form) {
                                handleValidationErrors($(options.form), error.responseJSON.errors);
                            } else {
                                showToast(Object.values(error.responseJSON.errors).flat().join(' '), 'error');
                            }
                        } else {
                            showToast('Solicitud incorrecta. Verifique los datos.', 'error');
                        }
                        throw error;
                    case 403:
                        const errorMessage = error.responseJSON?.error || 'No tiene permisos para esta acción';
                        if (options.suppressPermissionToasts) {
                            throw { ...error, message: 'Permiso denegado' };
                        }
                        showPermissionAlert(errorMessage);
                        showToast('Por favor contacte al administrador', 'error');
                        throw error;
                    default:
                        showToast(error.responseJSON?.message || 'Error en la solicitud', 'error');
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
    // Limpiar errores anteriores
    form.find('.is-invalid').removeClass('is-invalid');
    form.find('.is-valid').removeClass('is-valid');
    form.find('.invalid-feedback').remove();
    form.find('.valid-feedback').remove();

    // Mostrar nuevos errores
    for (const [key, messages] of Object.entries(errors)) {
        const input = form.find(`[name="${key}"]`);
        if (input.length) {
            input.addClass('is-invalid');

            // Si es un array de mensajes, unirlos
            const errorMessage = Array.isArray(messages) ? messages.join(' ') : messages;
            input.after(`<div class="invalid-feedback">${errorMessage}</div>`);
        }
    }

    // Marcar campos válidos
    form.find(':input').each(function () {
        const input = $(this);
        const fieldName = input.attr('name');
        if (fieldName && !errors[fieldName] && input.val().trim() !== '') {
            input.addClass('is-valid');
        }
    });
}

// Manejador de eventos para el botón "Cargar Inquilinos"
$('#loadUsuarios').on('click', function () {
    cargarTablaUsuarios();
});


async function cargarTablaUsuarios() {
    try {
        const data = await ajaxRequest({
            url: '/TbUsuarios/CargarUsuarios',
            type: 'GET',
            dataType: 'html', // Esperar HTML, no JSON
            errorMessage: 'Error al cargar la lista de usuarios',
            suppressPermissionToasts: true
        });

        $('#dataTableContainer').html(data).fadeIn();
        initUsuariosDataTable(); // Re-inicializar el DataTable

    } catch (error) {
        console.error('Error al cargar los usuarios:', error);

        // Manejo específico para error de permisos (403)
        if (error.status === 403 || error.message === 'Permiso denegado') { /* ... */ }
        else { /* ... */ }
    }
}

function initUsuariosDataTable() {
    if ($.fn.DataTable.isDataTable('#tablaUsuarios')) {
        $('#tablaUsuarios').DataTable().destroy();
    }

    const table = $('#tablaUsuarios').DataTable({
        pageLength: 10,
        lengthMenu: [5, 10, 25, 50, 100],
        order: [[0, 'desc']],
        autoWidth: false,
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
                titleAttr: 'Exportar a PDF',
                className: 'btn btn-outline-danger',
                orientation: 'landscape',
                pageSize: 'LETTER',
                exportOptions: {
                    columns: [0, 1, 2, 3, 4, 5], // Selecciona las columnas a exportar
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
                    columns: [0, 1, 2, 3, 4, 5], // Exportar solo columnas visibles
                }
            }
        ],
        columnDefs: [
            {
                targets: 0,
                visible: false
            },
            {
                targets: [1], // Columna de nombre
                visible: true,
                searchable: true
            },
            {
                targets: [2], // Columna de usuario
                visible: true,
                searchable: true
            },
            {
                targets: [3], // Columna de email
                visible: true

            },
            {
                targets: [4], // Columna de estado
                orderable: false,
                searchable: true
            },
            {
                targets: [5], // Columna de acciones
                orderable: false,
                searchable: false
            }
        ]
    });

    // Aplicar filtros personalizados
    applyCustomFilters(table);
    return table;
}


function applyCustomFilters(table) {
    console.log('Inicializando filtros personalizados...');

    // Filtro por estado - igual que el de inquilinos
    $('#filtroEstado').on('change', function () {
        const value = $(this).val();

        // Limpiar filtros previos
        table.search('').columns().search('').draw();

        if (value !== '') {
            $.fn.dataTable.ext.search.push(function (settings, data, dataIndex) {
                const row = table.row(dataIndex).node();
                const estadoText = $(row).find('td:eq(3)').text().trim(); // Columna 4 = Estado
                return value === 'true' ? estadoText === 'Activo' : estadoText === 'Inactivo';
            });

            table.draw();

            // Remover inmediatamente después del draw para que no afecte otros filtros
            $.fn.dataTable.ext.search.pop();
        } else {
            table.draw();
        }
    });

    $('#tablaUsuarios tbody tr:first td').each(function (index) {
        console.log(index + ': ' + $(this).text().trim());
    });

    // Filtro por nombre (columna 1)
    $('#filtroNombre').on('keyup', function () {
        table.column(1).search(this.value).draw();
    });

    // Filtro por usuario (columna 2)
    $('#filtroUsuario').on('keyup', function () {
        table.column(2).search(this.value).draw();
    });

    // Botón reset universal
    $('#btnResetFiltros').on('click', function () {
        // Limpiar inputs y selects
        $('#filtroEstado').val('');
        $('#filtroNombre').val('');
        $('#filtroUsuario').val('');

        // Limpiar filtro de estado personalizado de DataTables
        $.fn.dataTable.ext.search = $.fn.dataTable.ext.search.filter(fn => false);

        // Limpiar filtros de columnas y global
        table.search('').columns().search('').draw();
    });
}


$(document).on('submit', '.cambiarEstadoForm', async function (e) {
    e.preventDefault();
    const form = $(this);

    try {
        // Realizar la petición AJAX esperando HTML
        const response = await ajaxRequest({
            url: form.attr('action'),
            type: 'POST',
            data: form.serialize(),
            dataType: 'html',
            errorMessage: 'Error al cambiar el estado del usuario',
            suppressPermissionToasts: true // Suprimir toasts de permisos
        });

        // Reemplazar la tabla directamente con la respuesta HTML
        $('#dataTableContainer').html(response).fadeIn();
        initUsuariosDataTable();
        showToast('Estado del usuario actualizado correctamente.', 'success');

    } catch (error) {
        // Manejo específico para errores de permisos (403)
        if (error.status === 403 || error.message === 'Permiso denegado') {
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
        }
        // Manejo para otros tipos de errores
        else {
            console.error('Error al cambiar estado:', error);
            showToast('Error al cambiar el estado del usuario', 'error');
        }
    }
});

// Manejador para crear nuevo usuario
$('#createUsuario').on('click', async function () {
    try {
        const response = await ajaxRequest({
            url: '/TbUsuarios/Create',
            type: 'GET',
            dataType: 'html', // Esperar HTML, no JSON
            errorMessage: 'Error al cargar el formulario de creación',
            suppressPermissionToasts: true // Suprimir toasts para permisos
        });

        // Verificar si es una respuesta de error de permisos
        if (response && response.success === false && response.error) {
            showPermissionAlert(response.error);
            return;
        }

        $('#dataTableContainer').html(response).fadeIn();
        // Cargar los permisos en la card al mostrar el formulario
        loadAllPermissionsUsuario();
        initFormValidation($('#dataTableContainer').find('form'));
    } catch (error) {
        if (error.status === 403 || (error.responseJSON && error.responseJSON.error)) {
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
        } else if (error.message !== 'Permiso denegado') {
            console.error('Error al cargar formulario de creación:', error);
            showToast('Error al cargar el formulario', 'error');
        }
    }
});
// Al cargar la página, si existe la card de permisos de usuario, cargar los permisos (para acceso directo a la vista)
$(document).ready(function () {
    if ($('#permissionsCardUsuario').length > 0 && $('.permission-items-container-usuario').length > 0) {
        loadAllPermissionsUsuario();
    }
});

$(document).on('submit', 'form[action$="/Create"]', async function (e) {
    e.preventDefault();
    const form = $(this);

    console.log("📝 Datos del formulario:", form.serialize());

    // Validar que al menos un permiso esté seleccionado antes de crear usuario
    var checkedPerms = $('.permission-switch-usuario:checked');
    if (checkedPerms.length === 0) {
        showToast('Debe seleccionar al menos un permiso para el usuario.', 'error');
        e.preventDefault();
        return false;
    }

    // Eliminar inputs previos de permisos para evitar duplicados
    form.find('input[name="SelectedPermissions[]"]').remove();
    // Agregar los permisos seleccionados como inputs hidden al form
    checkedPerms.each(function () {
        var perm = $(this).data('permission');
        form.append('<input type="hidden" name="SelectedPermissions[]" value="' + perm + '" />');
    });

    try {
        const data = await ajaxRequest({
            url: form.attr('action'),
            type: 'POST',
            data: form.serialize(),
            form: form,
            errorMessage: 'Error al crear el usuario',
            suppressPermissionToasts: false
        });

        console.log("🎉 Usuario creado exitosamente:", data);

        if (data && typeof data === 'object' && data.success) {
            showToast(data.message || 'Usuario creado correctamente.', 'success');
            // Limpiar los campos del formulario
            form[0].reset();
            form.find('.is-valid, .is-invalid').removeClass('is-valid is-invalid');
            form.find('.invalid-feedback, .valid-feedback').remove();

            // Si se devolvió el userId, mostrar la interfaz de permisos personalizada
            if (data.userId) {
                // Mostrar el nombre del usuario en el título (si está disponible)
                const nombre = form.find('[name="Fnombre"]').val() || '';
                $('#selectedUserNameUsuario').text(nombre);
                // Cargar los permisos para el nuevo usuario (debes implementar loadPermissionsUsuario)
                if (typeof loadPermissionsUsuario === 'function') {
                    loadPermissionsUsuario(data.userId);
                }
                // Mostrar la card de permisos personalizada
                $('#permissionsCardUsuario').fadeIn();
            }
        } else if (data && typeof data === 'object') {
            // Manejar errores de validación
            if (data.errors) {
                handleValidationErrors(form, data.errors);
                showToast('Por favor, corrija los errores del formulario.', 'error');
            } else if (data.message) {
                showToast(data.message, 'error');
            }
        } else {
            showToast('Respuesta inesperada del servidor.', 'error');
        }
    } catch (error) {
        console.error("💥 Error completo:", error);
        console.log("📋 Response JSON:", error.responseJSON);
        console.log("📜 Response Text:", error.responseText);

        if (error.status === 400 && error.responseJSON && error.responseJSON.errors) {
            // Errores de validación ya manejados en el bloque try
        } else if (error.status === 403) {
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para crear usuarios');
        } else {
            showToast('Error inesperado: ' + (error.responseJSON?.message || error.message), 'error');
        }
    }
});



$(document).on('submit', 'form[action*="/Edit"]', async function (e) {
    e.preventDefault();
    const form = $(this);
    try {
        const data = await ajaxRequest({
            url: form.attr('action'),
            type: 'POST',
            data: form.serialize(),
            form: form,
            dataType: 'html',
            errorMessage: 'Error al editar los datos del usuario'
        });
        showToast('Usuario editado correctamente.', 'success');
        // Siempre intentar reemplazar el contenedor y reinicializar el DataTable
        $('#dataTableContainer').html(data).fadeIn();
        initUsuariosDataTable();

        // Si la respuesta es JSON (errores de validación) en vez de HTML, intentar parsear y mostrar errores
        if (typeof data === 'string' && (data.trim().startsWith('{') || data.trim().startsWith('['))) {
            let json;
            try {
                json = JSON.parse(data);
            } catch (e) {
                json = null;
            }
            if (json && json.success === false && json.errors) {
                for (const key in json.errors) {
                    if (json.errors.hasOwnProperty(key)) {
                        const errorMessage = json.errors[key];
                        const input = form.find(`[name="${key}"]`);
                        input.removeClass('is-invalid');
                        input.next('.invalid-feedback').remove();
                        input.addClass('is-invalid');
                        input.after(`<div class="invalid-feedback">${Array.isArray(errorMessage) ? errorMessage.join(' ') : errorMessage}</div>`);
                    }
                }
            }
        } else {
            // Si la respuesta es un formulario (por ejemplo, con errores de validación), volver a inicializar la validación
            initFormValidation($('#dataTableContainer').find('form'));
        }
    } catch (error) {
        console.error('Error al editar usuario:', error);
    }
});

// Función para inicializar validación del lado cliente
function initFormValidation(form) {
    console.log("🔐 Token antiforgery en formulario:", $('input[name="__RequestVerificationToken"]').length > 0 ? "✅ Presente" : "❌ No encontrado");
    // Variables para controlar el debounce
    let usuarioTimeout, emailTimeout;

    form.validate({
        onkeyup: function (element) {
            const elementName = $(element).attr('name');
            const value = $(element).val();

            // Validación inmediata para campos normales
            if (elementName !== 'Fusuario' && elementName !== 'Femail') {
                $(element).valid();
                return;
            }

            // Validación con debounce para campos remotos
            if (value.length < 3) return; // No validar si tiene menos de 3 caracteres

            if (elementName === 'Fusuario') {
                clearTimeout(usuarioTimeout);
                usuarioTimeout = setTimeout(() => {
                    $(element).valid();
                }, 500);
            } else if (elementName === 'Femail') {
                clearTimeout(emailTimeout);
                emailTimeout = setTimeout(() => {
                    $(element).valid();
                }, 500);
            }
        },
        onfocusout: function (element) {
            $(element).valid();
        },
        rules: {
            Fnombre: { required: true },
            Fusuario: {
                required: true,
                remote: {
                    url: '/TbUsuarios/VerificarUsuario',
                    type: 'GET',
                    data: {
                        usuario: function () {
                            return form.find('[name="Fusuario"]').val();
                        },
                        id: function () {
                            return form.find('[name="FidUsuario"]').val();
                        }
                    }
                }
            },
            Femail: {
                required: true,
                email: true,
                remote: {
                    url: '/TbUsuarios/VerificarEmail',
                    type: 'GET',
                    data: {
                        email: function () {
                            return form.find('[name="Femail"]').val();
                        },
                        id: function () {
                            return form.find('[name="FidUsuario"]').val();
                        }
                    }
                }
            },
            Fpassword: {
                required: function () {
                    // Solo requerido si es nuevo usuario (no tiene ID)
                    return !form.find('[name="FidUsuario"]').val();
                }
            },
            FRepeatPassword: {
                required: function () {
                    return !form.find('[name="FidUsuario"]').val();
                },
                equalTo: '#Fpassword'
            }
        },
        messages: {
            Fnombre: { required: "El nombre es obligatorio" },
            Fusuario: {
                required: "El usuario es obligatorio",
                remote: "Este nombre de usuario ya está registrado"
            },
            Femail: {
                required: "El correo es obligatorio",
                email: "Ingrese un correo electrónico válido",
                remote: "Este correo electrónico ya está registrado"
            },
            Fpassword: { required: "La contraseña es obligatoria" },
            FRepeatPassword: {
                required: "La confirmación de contraseña es obligatoria",
                equalTo: "Las contraseñas no coinciden"
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
});

// Función para cargar y renderizar los permisos en la card personalizada tras crear usuario
async function loadPermissionsUsuario(userId) {
    // Limpia contenedores
    $('.permission-items-container-usuario').empty();
    // Obtiene los permisos del usuario
    try {
        const data = await $.get(`/AdministrarPermisos/GetUserPermissions?userId=${userId}`);
        // data.permissionCategories: { [categoria]: [ { value, displayName, isSelected } ] }
        $.each(data.permissionCategories, function (category, permissions) {
            // Normaliza el id de la pestaña
            const normalizedCategory = category.replace(/\s+/g, '');
            const container = $(`#content-usuario-${category} .permission-items-container-usuario`);
            if (container.length === 0) return;
            permissions.forEach(permission => {
                const switchId = `perm-usuario-${permission.value.replace(/\./g, '-')}`;
                const isChecked = permission.isSelected ? 'checked' : '';
                container.append(`
                    <div class="permission-item">
                        <div class="form-check form-switch">
                            <input type="checkbox" class="form-check-input permission-switch-usuario" 
                                   id="${switchId}" 
                                   data-permission="${permission.value}"
                                   ${isChecked}>
                            <label class="form-check-label" for="${switchId}">
                                <i class="fas ${getPermissionIconUsuario(permission.value)}"></i>
                                ${permission.displayName}
                            </label>
                        </div>
                    </div>
                `);
            });
        });
    } catch (err) {
        showToast('Error al cargar permisos del usuario', 'error');
    }
}

// Función auxiliar para iconos
function getPermissionIconUsuario(permission) {
    const action = permission.split('.').pop();
    switch (action) {
        case 'Ver': return 'fa-eye';
        case 'Crear': return 'fa-plus-circle';
        case 'Editar': return 'fa-edit';
        case 'Anular': return 'fa-trash-alt';
        default: return 'fa-key';
    }
}