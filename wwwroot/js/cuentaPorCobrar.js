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

// Función para manejar errores de validación (la misma que en inmuebles)
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

// Función para inicializar DataTable de cuentas por cobrar
function initCuentasPorCobrarDataTable() {
    if ($.fn.DataTable.isDataTable('#cuentasPorCobrarTable')) {
        $('#cuentasPorCobrarTable').DataTable().destroy();
    }

    // Configuración de localización para fechas
    moment.locale('es', {
        months: 'Enero_Febrero_Marzo_Abril_Mayo_Junio_Julio_Agosto_Septiembre_Octubre_Noviembre_Diciembre'.split('_'),
        monthsShort: 'Ene_Feb_Mar_Abr_May_Jun_Jul_Ago_Sep_Oct_Nov_Dic'.split('_'),
        weekdays: 'Domingo_Lunes_Martes_Miércoles_Jueves_Viernes_Sábado'.split('_'),
        weekdaysShort: 'Dom_Lun_Mar_Mié_Jue_Vie_Sáb'.split('_')
    });

    const table = $('#cuentasPorCobrarTable').DataTable({
        pageLength: 10,
        lengthMenu: [5, 10, 25, 50, 100],
        order: [[0, 'desc']],
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
                    columns: [0, 1, 2, 3, 4, 5, 6, 7, 8, 10, 11],
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

                    const table = $('#cuentasPorCobrarTable').DataTable();

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
                    columns: [0, 1, 2, 3, 4, 5, 6, 7, 8, 10, 11], // Exportar solo columnas visibles
                }
            }
        ],
        columnDefs: [
            {
                targets: 0,  // Columna ID (oculta)
                type: "num",
                visible: false
            },
            {
                targets: [4, 5],  // Columnas Días de Gracia (5) y Tasa de Mora (6)
            },
            {
                targets: [6, 7, 8],
                type: 'date-euro'
            },
            {
                targets: [9], // Columna de estado
                orderable: false,
                searchable: true,
                visible: false
            },
            {
                targets: [10],
                orderable: false,
                searchable: true,
                visible: true
            }
        ]
    });

    // Inicializar datepickers
    setupDateFilters(table);

    // Aplicar otros filtros
    applyCustomFilters(table);

    return table;
}

// Función para configurar los datepickers y filtros de fecha
function setupDateFilters(table) {
    // 1. Inicializar datepickers
    const minDatePicker = new DateTime($('#min'), {
        format: 'DD/MM/YYYY',
        i18n: {
            previous: 'Anterior',
            next: 'Siguiente',
            months: moment.localeData('es').months(),
            weekdays: moment.localeData('es').weekdaysShort()
        },
        buttons: {
            showToday: true,
            showClear: true,
            showClose: true
        }
    });

    const maxDatePicker = new DateTime($('#max'), {
        format: 'DD/MM/YYYY',
        i18n: {
            previous: 'Anterior',
            next: 'Siguiente',
            months: moment.localeData('es').months(),
            weekdays: moment.localeData('es').weekdaysShort()
        },
        buttons: {
            showToday: true,
            showClear: true,
            showClose: true
        },
        useCurrent: false
    });

    // 2. Configurar eventos para mostrar calendario
    $('#min').on('click', function () {
        minDatePicker.toggle();
    });

    $('#max').on('click', function () {
        maxDatePicker.toggle();
    });

    // 3. Sincronización de datepickers (sin disparar filtrado)
    $('#min').on('change.datetimepicker', function (e) {
        if (e.date) {
            maxDatePicker.minDate(e.date);
            $(this).addClass('has-value');
        } else {
            $(this).removeClass('has-value');
        }
        // Removido table.draw() aquí
    });

    $('#max').on('change.datetimepicker', function (e) {
        if (e.date) {
            minDatePicker.maxDate(e.date);
            $(this).addClass('has-value');
        } else {
            $(this).removeClass('has-value');
        }
        // Removido table.draw() aquí
    });

    // 4. Botones para limpiar fechas
    $('#min-clear').on('click', function () {
        minDatePicker.clear();
        $(this).closest('.input-group').find('input').val('').removeClass('has-value');
        // No llamar a table.draw() aquí
    });

    $('#max-clear').on('click', function () {
        maxDatePicker.clear();
        $(this).closest('.input-group').find('input').val('').removeClass('has-value');
        // No llamar a table.draw() aquí
    });

    // 5. Variable para almacenar la función de filtrado
    let dateFilterFunction = null;

    // Configurar el botón "Aplicar Filtros" para el rango de fechas
    // Configurar el botón "Aplicar Filtros" para el rango de fechas
    $('#btnAplicarFiltros').on('click', function () {
        // Remover el filtro anterior si existe
        if (dateFilterFunction) {
            $.fn.dataTable.ext.search.pop();
        }

        // Obtener qué columna de fecha vamos a filtrar
        const columnaFecha = parseInt($('#filtroTipoFecha').val());

        // Validar que la columna sea correcta
        if (isNaN(columnaFecha) || columnaFecha < 0) {
            console.error('Columna de fecha no válida:', columnaFecha);
            return;
        }

        // Crear nuevo filtro
        dateFilterFunction = function (settings, data, dataIndex) {
            const min = $('#min').val();
            const max = $('#max').val();
            const dateStr = data[columnaFecha]; // Usamos la columna seleccionada

            console.log('Filtrando:', {
                min, max, dateStr, columnaFecha
            });

            // Si no hay filtros de fecha, mostrar todas las filas
            if (!min && !max) return true;

            // Si la celda está vacía o es "N/A", no mostrarla cuando hay filtros activos
            if (!dateStr || dateStr.trim() === '' || dateStr.trim() === 'N/A') {
                return false;
            }

            try {
                const fechaTabla = parseDateString(dateStr);
                if (!fechaTabla) return false;

                const minDate = min ? parseDateString(min) : null;
                const maxDate = max ? parseDateString(max) : null;

                // Si no se pudo parsear alguna fecha de filtro, mostrar todas las filas
                if ((min && !minDate) || (max && !maxDate)) {
                    return true;
                }

                // Comparar fechas
                const fechaTablaTime = fechaTabla.getTime();
                const minDateTime = minDate ? minDate.getTime() : null;
                const maxDateTime = maxDate ? maxDate.getTime() : null;

                let cumpleMin = true;
                let cumpleMax = true;

                if (minDateTime) {
                    cumpleMin = fechaTablaTime >= minDateTime;
                }

                if (maxDateTime) {
                    // Incluir el día completo de la fecha máxima
                    const maxDateInclusive = new Date(maxDate);
                    maxDateInclusive.setHours(23, 59, 59, 999);
                    cumpleMax = fechaTablaTime <= maxDateInclusive.getTime();
                }

                return cumpleMin && cumpleMax;
            } catch (e) {
                console.error('Error al parsear fecha:', e);
                return false;
            }
        };

        // Aplicar el filtro
        $.fn.dataTable.ext.search.push(dateFilterFunction);
        table.draw();
    });

    // Función mejorada para parsear fechas
    function parseDateString(dateStr) {
        if (!dateStr) return null;

        // Limpiar el string de cualquier tag HTML o clase
        dateStr = dateStr.toString().replace(/<[^>]*>/g, '').trim();

        // Manejar el caso "N/A"
        if (dateStr === 'N/A' || dateStr === 'NULL') return null;

        // Extraer la fecha en formato DD/MM/YYYY
        const dateMatch = dateStr.match(/(\d{1,2})\/(\d{1,2})\/(\d{2,4})/);
        if (!dateMatch) {
            console.warn('Formato de fecha no reconocido:', dateStr);
            return null;
        }

        const day = dateMatch[1].padStart(2, '0');
        const month = dateMatch[2].padStart(2, '0');
        let year = dateMatch[3];

        // Ajustar año de 2 dígitos
        if (year.length === 2) {
            year = '20' + year;
        }

        // Crear la fecha en formato ISO (YYYY-MM-DD)
        const isoDate = `${year}-${month}-${day}`;
        const dateObj = new Date(isoDate);

        // Validar que la fecha sea válida
        if (isNaN(dateObj.getTime())) {
            console.warn('Fecha inválida:', dateStr);
            return null;
        }

        return dateObj;
    }
}

// Función para aplicar otros filtros personalizados
function applyCustomFilters(table) {
    // Filtro por estado
    $('#filtroEstado').on('change', function () {
        const value = $(this).val();

        // Limpiar búsquedas previas
        table.search('').columns().search('').draw();

        if (value !== '') {
            // Eliminar cualquier filtro previo
            $.fn.dataTable.ext.search = [];

            // Agregar nuevo filtro
            $.fn.dataTable.ext.search.push(
                function (settings, data, dataIndex) {
                    // La columna de Estado es la 9 (index 9)
                    const estadoText = data[9].trim();
                    return (value === 'true' && estadoText === 'Activo') ||
                        (value === 'false' && estadoText === 'Inactivo');
                }
            );

            table.draw();
        } else {
            // Si no hay valor seleccionado, eliminar todos los filtros
            $.fn.dataTable.ext.search = [];
            table.draw();
        }
    });

    // Filtro por inquilino (columna 1 - index 1)
    $('#filtroInquilino').on('keyup', function () {
        table.column(1).search(this.value).draw();
    });

    // Filtro por período (columna 7 - index 7)
    $('#filtroPeriodo').on('change', function () {
        table.column(7).search(this.value).draw();
    });

    // Botón reset
    $('#btnResetFiltros').on('click', function () {
        // Limpiar todos los filtros
        $.fn.dataTable.ext.search = [];

        // Resetear controles
        $('#filtroEstado').val('').trigger('change');
        $('#filtroInquilino').val('').trigger('keyup');
        $('#filtroPeriodo').val('').trigger('change');
        $('#filtroTipoFecha').val('5'); // Resetear a Fecha Inicio
        $('#min').val('').removeClass('has-value');
        $('#max').val('').removeClass('has-value');

        // Redibujar tabla sin filtros
        table.search('').columns().search('').draw();
    });
}

/////// Funcion para inicializar Match Custom CxC ///////
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

function inicializarSelect2() {
    const $selectInquilino = $('#busquedaInquilino');
    const valorSeleccionadoInquilino = $selectInquilino.val();

    // Reiniciar Select2 para inquilino
    if ($selectInquilino.hasClass('select2-hidden-accessible')) {
        $selectInquilino.select2('destroy');
    }

    $selectInquilino.empty().select2({
        placeholder: "Buscar inquilino...",
        allowClear: true,
        matcher: matchCustom // Asegúrate de tener definida esta función
    });

    $.ajax({
        url: '/TbCuentasPorCobrar/BuscarInquilinos',
        dataType: 'json',
        success: function (data) {
            const inquilinoData = data.results;

            $selectInquilino.append(new Option('', '', false, false)); // Opción vacía

            inquilinoData.forEach(item => {
                const option = new Option(item.text, item.id, false, false);
                $selectInquilino.append(option);
            });

            if (valorSeleccionadoInquilino) {
                $selectInquilino.val(valorSeleccionadoInquilino).trigger('change');
            }
        },
        error: function () {
            alert('Error al cargar los inquilinos.');
        }
    });

    $selectInquilino.on('change', function () {
        $('#FkidInquilino').val($(this).val());
    });

    $('#busquedaInmueble').on('change', function () {
        const selectedOption = $(this).find('option:selected');
        const monto = selectedOption.data('monto'); // Obtener el monto del inmueble seleccionado
        $('#Fmonto').val(monto); // Establecer el monto en el campo correspondiente
        $('#FkidInmueble').val($(this).val()); // Asegúrate de que el ID del inmueble también se establezca
    });

    const $selectInmueble = $('#busquedaInmueble');
    const valorSeleccionadoInm = $selectInmueble.val();

    // Reiniciar Select2 para inmueble
    if ($selectInmueble.hasClass('select2-hidden-accessible')) {
        $selectInmueble.select2('destroy');
    }

    $selectInmueble.empty().select2({
        placeholder: "Buscar inmueble...",
        allowClear: true,
        matcher: matchCustom // Asegúrate de tener definida esta función
    });

    $.ajax({
        url: '/TbCuentasPorCobrar/BuscarInmuebles',
        dataType: 'json',
        success: function (data) {
            const inmuebleData = data.results;

            $selectInmueble.append(new Option('', '', false, false)); // Opción vacía

            inmuebleData.forEach(item => {
                const option = new Option(item.text, item.id, false, false);
                $(option).data('monto', item.monto); // Almacenar el monto en el elemento option
                $selectInmueble.append(option);
            });

            if (valorSeleccionadoInm) {
                $selectInmueble.val(valorSeleccionadoInm).trigger('change');
            }
        },
        error: function () {
            alert('Error al cargar los inmuebles.');
        }
    });

    $selectInmueble.on('change', function () {
        $('#FkidInmueble').val($(this).val());
    });
}

// Función para cargar la tabla de cuentas por cobrar (adaptada de inmuebles)
async function cargarTablaCuentasPorCobrar() {
    try {
        const data = await ajaxRequest({
            url: '/TbCuentasPorCobrar/CargarCuentasPorCobrar',
            type: 'GET',
            errorMessage: 'Error al cargar la lista de cuentas por cobrar'
        });

        $('#dataTableContainer').html(data).fadeIn();
        initCuentasPorCobrarDataTable();
    } catch (error) {
        console.error('Error al cargar inquilinos:', error);

        // Manejo específico para error de permisos (403)
        if (error.status === 403 || error.message === 'Permiso denegado') {
            const errorMsg = error.responseJSON?.error || 'No tiene permisos para ver la lista de CxC';
            showPermissionAlert(errorMsg);
        }
        // Manejo de otros tipos de errores
        else {
            showToast('Error al cargar la lista de CxC', 'error');
        }
    }
}

// Manejador de eventos para el botón "Cargar Propietarios"
$('#loadCuentasPorCobrar').on('click', function () {
    cargarTablaCuentasPorCobrar();
});

// Manejador para crear nueva cuenta por cobrar
$('#createCuentaPorCobrar').on('click', async function () {
    try {
        const data = await ajaxRequest({
            url: '/TbCuentasPorCobrar/Create',
            type: 'GET',
            errorMessage: 'Error al cargar el formulario de creación'
        });

        $('#dataTableContainer').html(data).fadeIn();
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

$(document).on('submit', 'form[action$="/Create"]', async function (e) {
    e.preventDefault();
    const form = $(this);

    // Inicializar validación
    initFormValidation(form);

    // Detener si el formulario es inválido
    if (!form.valid()) {
        return;
    }

    // Crear FormData tradicional
    const formData = new FormData(form[0]);

    // Agregar console.log para depuración
    for (let [key, value] of formData.entries()) {
        console.log(`${key}: ${value}`);
    }

    // Establecer valores por defecto si están vacíos
    if (!formData.get('FdiasGracia')) formData.set('FdiasGracia', '0');
    if (!formData.get('FtasaMora')) formData.set('FtasaMora', '0');
    if (!formData.get('Fnota')) formData.set('Fnota', 'No se proporcionaron notas adicionales');

    try {
        const response = await $.ajax({
            url: form.attr('action'),
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val(),
                'Accept': 'application/json'
            }
        });

        if (response.success) {
            Toast.fire({
                icon: 'success',
                title: 'Cuenta por cobrar creada correctamente'
            });
            // 🚀 Recargar formulario limpio
            await cargarVistaCrearCuentaPorCobrar();

        } else {
            const errorMsg = response.message ||
                (response.errors ? response.errors.join(', ') : 'Error desconocido');
            showToast('Error al crear la cuenta: ' + errorMsg, 'error');
        }
    } catch (error) {
        console.error('Error al crear la CxC:', error);
        let errorMsg = 'Error desconocido';

        if (error.responseJSON) {
            errorMsg = error.responseJSON.message ||
                (error.responseJSON.errors ? Object.values(error.responseJSON.errors).flat().join(', ') : JSON.stringify(error.responseJSON));
        } else if (error.responseText) {
            try {
                const parsedError = JSON.parse(error.responseText);
                errorMsg = parsedError.message || Object.values(parsedError.errors).flat().join(', ');
            } catch {
                errorMsg = error.responseText;
            }
        } else if (error.statusText) {
            errorMsg = error.statusText;
        }

        showToast('Error al crear la CxC: ' + errorMsg, 'error');
    }
});

// Manejador para editar cuenta por cobrar
$(document).on('click', '.editCxC', async function () {
    const id = $(this).data('id');
    try {
        const data = await ajaxRequest({
            url: `/TbCuentasPorCobrar/Edit/${id}`,
            type: 'GET',
            errorMessage: 'Error al cargar el formulario de edición'
        });

        $('#dataTableContainer').html(data).fadeIn();
        //inicializarSelect2();
        aplicarMascaraPrecio();
        initFormValidation($('#dataTableContainer').find('form'));

        // Manejar navegación de vuelta
        $('#btnCancelarEdicion').off('click').on('click', async () => {
            await cargarTablaCxC();
        });
    } catch (error) {
        if (error.status === 403) {
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
        } else if (error.status === 404) {
            showToast('Cuenta por cobrar no encontrado', 'error');
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
            errorMessage: 'Error al editar la cuenta por cobrar'
        });

        cargarTablaCuentasPorCobrar();
        initCuentasPorCobrarDataTable();
        Toast.fire({
            icon: 'success',
            title: 'Cuenta por cobrar actualizada correctamente'
        });

    } catch (error) {
        if (error.status === 403) {
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
        } else {
            console.error('Error al editar la cuenta por cobrar:', error);
            showToast('Error al editar la cuenta por cobrar: ' + (error.responseJSON?.message || error.message), 'error');
        }
    }
});

// Manejador para cambiar estado (adaptado de inmuebles)
$(document).on('submit', '.cambiarEstadoForm', async function (e) {
    e.preventDefault();
    const form = $(this);
    try {
        await ajaxRequest({
            url: form.attr('action'),
            type: 'POST',
            data: form.serialize(),
            errorMessage: 'Error al cambiar el estado de la cuenta por cobrar',
            suppressPermissionToasts: true
        });

        await cargarTablaCuentasPorCobrar();
        Toast.fire({
            icon: 'success',
            title: 'Estado de la cuenta por cobrar actualizado correctamente'
        });

    } catch (error) {
        // Manejo específico para errores de permisos (403)
        if (error.status === 403 || error.message === 'Permiso denegado') {
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
        }
        // Manejo para otros tipos de errores
        else {
            console.error('Error al cambiar estado:', error);
            showToast('Error al cambiar el estado de la cuenta por cobrar', 'error');
        }
    }
});

// Manejador para el formulario de anulación
$(document).on('submit', '.cancelarCxCForm', function (e) {
    e.preventDefault();
    const form = $(this);
    const cuentaId = form.find('input[name="cuentaId"]').val();

    if (!cuentaId) {
        console.error('No se pudo obtener el ID de la CxC');
        showToast('Error al obtener la CxC a cancelar', 'error');
        return;
    }

    // Mostrar modal de confirmación y guardar el ID
    $('#confirmCancelarModal').data('cuenta-id', cuentaId).modal('show');

    // Cargar datos del usuario actual
    $.get('/TbCuentasPorCobrar/GetUsuarioActual')
        .done(function (response) {
            if (response.success) {
                const userName = response.usuario.UserName || response.usuario.userName;
                $('#userName').val(userName || 'Usuario no identificado');
            }
        })
        .fail(function (xhr) {
            // Manejo específico para error 403 (Forbidden)
            if (xhr.status === 403) {
                const mensaje = xhr.responseJSON?.error || 'No tiene permisos para consultar el usuario actual.';
                showPermissionAlert(mensaje);
                return;
            }

            $('#userName').val('Error al cargar usuario');
            showToast('Error al cargar datos del usuario', 'error');
            console.error('Error al consultar usuario actual:', xhr);
        });
});

// Manejador para el botón de confirmación (usa delegación de eventos)
$(document).on('click', '#confirmCancelarBtn', async function () {
    const btn = $(this);
    const cuentaId = $('#confirmCancelarModal').data('cuenta-id');

    if (!cuentaId) {
        showToast('No se pudo identificar la CxC a cancelar', 'error');
        return;
    }

    btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Anulando...');

    const motivo = $('#motivoCancelacion').val();
    const password = $('#usuarioPassword').val();

    if (!motivo || !password) {
        showToast('Debe completar todos los campos', 'error');
        btn.prop('disabled', false).html('<i class="fas fa-ban me-1"></i> Anular CxC');
        return;
    }

    try {
        // Validar contraseña
        const validacion = await $.ajax({
            url: '/TbCuentasPorCobrar/ValidarContrasena',
            type: 'POST',
            data: JSON.stringify({ password: password }),
            contentType: 'application/json',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            }
        });

        if (!validacion.success) {
            showToast(validacion.message || 'Contraseña incorrecta', 'error');
            return;
        }

        // Cancelar la CxC
        const response = await $.ajax({
            url: `/TbCuentasPorCobrar/CancelarCxC/${cuentaId}`,
            type: 'POST',
            data: JSON.stringify({
                motivoCancelacion: motivo,
                password: password
            }),
            contentType: 'application/json',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val(),
                'X-Requested-With': 'XMLHttpRequest'
            }
        }).fail(function (jqXHR, textStatus, errorThrown) {
            console.error("AJAX error:", textStatus, errorThrown);
            console.error("Full response:", jqXHR.responseText);
            return Promise.reject(jqXHR);
        });

        if (response.success) {
            $('#confirmCancelarModal').modal('hide');
            Toast.fire({
                icon: 'success',
                title: response.message
            });
            await cargarTablaCuentasPorCobrar();
        } else {
            showToast(response.message || 'Error al cancelar la CxC', 'error');
        }

    } catch (error) {
        console.error('Error al cancelar CxC:', error);

        // Manejo específico de error 403
        if (error.status === 403) {
            showPermissionAlert(error.responseJSON?.message || 'No tiene permisos para esta acción');
        } else {
            const errorMsg = error.responseJSON?.message || 'Error al cancelar la CxC';
            showToast(errorMsg, 'error');
        }

    } finally {
        btn.prop('disabled', false).html('<i class="fas fa-ban me-1"></i> Cancelar CxC');
        $('#motivoCancelacion').val('');
        $('#usuarioPassword').val('');
    }
});

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

// Función para inicializar validación del formulario (adaptada de inmuebles)
function initFormValidation(form) {
    form.validate({
        rules: {
            FkidInquilino: {
                required: true
            },
            busquedaInmueble: {
                required: true
            },
            Fmonto: {
                required: true,
                number: true,
                min: 1
            },
            FfechaInicio: {
                required: true,
                date: true
            },
            FdiasGracia: {
                number: true,
                min: 0
            },
            FkidPeriodoPago: {
                required: true
            }
        },
        messages: {
            FkidInquilino: {
                required: "Debe seleccionar un inquilino"
            },
            busquedaInmueble: {
                required: "Debe seleccionar un inmueble"
            },
            Fmonto: {
                required: "El monto es obligatorio",
                number: "Debe ser un valor numérico",
                min: "El monto debe ser mayor a 0"
            },
            FfechaInicio: {
                required: "La fecha de inicio es obligatoria",
                date: "Debe ser una fecha válida"
            },
            FkidPeriodoPago: {
                required: "Debe seleccionar un período de pago"
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

async function cargarVistaCrearCuentaPorCobrar() {
    try {
        const response = await ajaxRequest({
            url: '/TbCuentasPorCobrar/Create',
            type: 'GET',
            errorMessage: 'Error al cargar el formulario de creación',
            suppressPermissionToasts: true
        });

        if (response && response.success === false && response.error) {
            showPermissionAlert(response.error);
            return;
        }

        $('#dataTableContainer').html(response).fadeIn();

        // Inicializar select2 (Inquilino + Inmueble)
        inicializarSelect2();

        // Aplicar máscara a monto
        aplicarMascaraPrecio();

        // Validación del formulario
        initFormValidation($('#dataTableContainer').find('form'));
    } catch (error) {
        if (error.status === 403 || (error.responseJSON && error.responseJSON.error)) {
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
        } else if (error.message !== 'Permiso denegado') {
            console.error('Error al cargar formulario de creación:', error);
            showToast('Error al cargar el formulario', 'error');
        }
    }
}

// Click en la opción "Lista inquilinos" del sidebar
$(document).on('click', '#menuListaCxC', function (e) {
    e.preventDefault();
    cargarTablaCuentasPorCobrar();
});

$(document).ready(async function () {
    // Configuración global de manejo de errores AJAX
    $(document).ajaxError(function (event, jqxhr, settings, thrownError) {
        showToast('Ocurrió un error inesperado. Por favor intente nuevamente.');
    });
  
});