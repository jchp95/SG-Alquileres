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
                error: function (xhr) {
                    // Manejar redirección 401 directamente aquí
                    if (xhr.status === 401) {
                        const redirectUrl = xhr.responseJSON?.redirectUrl || '/Identity/Account/Login';
                        window.location.replace(redirectUrl);
                        return false; // Evitar que el error llegue al catch
                    }
                    // Permitir que otros errores pasen al catch
                    return true;
                }
            });

            return response;
        } catch (error) {
            // Este bloque no se ejecutará para errores 401
            attempts++;

            // Manejo específico de códigos de estado
            switch (error.status) {
                case 403:
                    const errorMessage = error.responseJSON?.error || 'No tiene permisos para realizar esta acción';
                    showPermissionAlert(errorMessage);
                    if (options.suppressPermissionToasts) {
                        throw new Error('Permiso denegado');
                    }
                    showToast('Por favor contacte al administrador', 'error');
                    throw error;
                case 404:
                    showToast('Recurso no encontrado', 'error');
                    throw error;
                default:
                    if (error.responseJSON && error.responseJSON.errors) {
                        if (options.form) {
                            handleValidationErrors($(options.form), error.responseJSON.errors);
                        } else {
                            showToast(Object.values(error.responseJSON.errors).flat().join(' '));
                        }
                    } else {
                        const userMessage = options.errorMessage || 'Error de comunicación con el servidor';
                        showToast(userMessage);
                    }
                    throw error;
            }
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


var LenguajeEs = {
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
        fechaFin: fechaFin || '',
        filtroInquilino: $('#filtroInquilino').val() || '',
        filtroInmueble: $('#filtroInmueble').val() || ''
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

        inicializarSelectUsuario('reporteCxCTable');

        // Inicializar el DataTable con los datos cargados
        const table = $('#reporteCxCTable').DataTable({
            destroy: true,
            responsive: true,
            dom: '<"top"lf>Brt<"bottom"ip>',
            language: LenguajeEs,
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
                        columns: ':visible',
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

                        const table = $('#reporteCxCTable').DataTable();

                        // Encontrar los índices de las columnas necesarias
                        let montoIndex = -1;
                        let cuentaIndex = -1;

                        table.columns().every(function () {
                            const headerText = this.header().textContent.trim();
                            if (headerText === 'Monto') {
                                montoIndex = this.index();
                            } else if (headerText === '# Cuenta') {
                                cuentaIndex = this.index();
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
                        doc.pageMargins = [40, 40, 40, 80];;
                        doc.defaultStyle.fontSize = 8;
                        doc.styles.tableHeader.fontSize = 9
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

                            // Colocar "TOTAL:" en la columna Ubicación (índice 3)
                            totalRow[4] = { text: 'TOTAL:', bold: true, alignment: 'right' };

                            // Colocar el monto total en la columna Monto (índice 4)
                            totalRow[5] = { text: totalFormateado, bold: true, alignment: 'right' };

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
                }
            ],
            lengthMenu: [10, 25, 50, 100],
            pageLength: 10,
            columnDefs: [
                {
                    targets: [0],
                    visible: false,
                    searchable: true
                },
                {
                    targets: [5, 6],
                    visible: false,
                    searchable: true
                }
            ],
        });

        // Filtros rápidos
        $('#filtroInquilino, #filtroInmueble').on('keyup', function () {
            const columnIndex = $(this).attr('id') === 'filtroInquilino' ? 2 : 3; // 2 para Inquilino, 3 para Inmueble
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
            const fecha = data[9]; // Columna de fecha (ajustado a la posición correcta)

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
            $('#filtroInquilino').val('');
            $('#filtroInmueble').val('');

            // Resetear filtros del DataTable
            const table = $('#reporteCxCTable').DataTable();
            table.search('').columns().search('').draw();

            // Remover filtro de fechas personalizado
            $.fn.dataTable.ext.search = $.fn.dataTable.ext.search.filter(filtro => {
                return filtro.toString() !== function (settings, data, dataIndex) {
                    /* filtro de fechas */
                }.toString();
            });

            table.draw();
        });

        // Función para calcular y actualizar los totales del reporte de CxC
        function calcularTotalesCxC() {
            const table = $('#reporteCxCTable').DataTable();
            
            // Obtener solo las filas visibles (después de filtros)
            const filasVisibles = table.rows({ search: 'applied' }).data();
            
            let totalRegistros = 0;
            let totalMonto = 0;
            
            // Contar registros
            totalRegistros = filasVisibles.length;
            
            // Iterar sobre las filas visibles para sumar los montos
            filasVisibles.each(function (row) {
                // Estructura de columnas según el HTML del reporte CxC:
                // 0: Usuario (oculto), 1: # Cuenta, 2: Inquilino, 3: Inmueble, 4: Dirección, 
                // 5: Ubicación, 6: Fecha Actual, 7: Hora Actual, 8: Monto, 9: Fecha Inicio
                
                // Limpiar y convertir monto (columna 8)
                const montoStr = row[8] ? row[8].toString() : '0';
                const monto = parseFloat(montoStr.replace(/[^\d.-]/g, '')) || 0;
                totalMonto += monto;
            });
            
            // Formatear números con formato 1,000.00
            function formatearMonto(numero) {
                return 'RD$ ' + numero.toLocaleString('en-US', {
                    minimumFractionDigits: 2,
                    maximumFractionDigits: 2
                });
            }
            
            // Actualizar los elementos HTML
            $('#totalRegistrosCxC').text(totalRegistros.toLocaleString('en-US'));
            $('#totalCxC').text(formatearMonto(totalMonto));
            
            console.log('[TOTALES CXC]', {
                registros: totalRegistros,
                monto: totalMonto
            });
        }
        
        // Calcular totales iniciales
        calcularTotalesCxC();
        
        // Recalcular totales cuando la tabla se redibuja (filtros, búsquedas, etc.)
        table.on('draw', function() {
            calcularTotalesCxC();
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

        inicializarSelectUsuario('reporteCobrosTable');

        // Inicializar el DataTable con los datos cargados
        const table = $('#reporteCobrosTable').DataTable({
            destroy: true,
            responsive: true,
            dom: '<"top"lf>Brt<"bottom"ip>',
            language: LenguajeEs,
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
                        columns: ':visible',
                        modifier: {
                            page: 'all'
                        }
                    },
                    customize: function (doc) {
                        // Función para formatear números al estilo RD (comas para miles, punto para decimales)
                        function formatoRD(numero) {
                            // Si el número es 0, mostrar "0.00" en lugar de "NaN"
                            if (isNaN(numero) || numero === 0) return '0.00';
                            return numero.toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,');
                        }

                        // Obtener fecha y hora actual
                        const now = new Date();
                        const fechaHora = now.toLocaleDateString('es-MX', {
                            year: 'numeric',
                            month: 'long',
                            day: 'numeric',
                            hour: '2-digit',
                            minute: '2-digit'
                        });

                        const table = $('#reporteCobrosTable').DataTable();

                        // Encontrar el índice de la columna Monto
                        let montoIndex = -1;
                        table.columns().every(function () {
                            const headerText = this.header().textContent.trim();
                            if (headerText === 'Monto') {
                                montoIndex = this.index();
                                return false; // Salir del bucle una vez encontrado
                            }
                        });

                        // Calcular el total de la columna Monto
                        let totalMonto = 0;
                        let totalRegistros = 0;

                        if (montoIndex !== -1) {
                            table.rows({ search: 'applied' }).data().each(function (row) {
                                const montoStr = row[montoIndex].toString();
                                // Eliminar RD$, comas y cualquier otro carácter no numérico excepto punto y negativo
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
                            text: `Total de cobros: ${totalRegistros.toLocaleString('es-RD')}`,
                            alignment: 'right',
                            margin: [0, 0, 40, 10],
                            fontSize: 9,
                            bold: true
                        });

                        // Añadir fila de total al cuerpo de la tabla
                        if (doc.content[2].table.body.length > 0) {
                            const columnsCount = doc.content[2].table.body[0].length;
                            const totalRow = new Array(columnsCount).fill('');

                            // Asumiendo la estructura: [Usuario, #Cobro, Inquilino, Monto, Concepto, Fecha]
                            // La columna Monto sería la 4ta columna (índice 3)
                            const montoColPosition = 2;

                            // Colocar "TOTAL:" en la columna anterior al Monto (Inquilino)
                            totalRow[montoColPosition - 1] = {
                                text: 'TOTAL:',
                                bold: true,
                                alignment: 'right',
                                margin: [0, 5, 0, 5]
                            };

                            // Colocar el monto total en la columna Monto
                            totalRow[montoColPosition] = {
                                text: totalFormateado,
                                bold: true,
                                alignment: 'right',
                                margin: [0, 5, 0, 5]
                            };

                            doc.content[2].table.body.push(totalRow);
                        }

                        // Centrar la tabla
                        doc.content[2].alignment = 'center';

                        // Ajustar el ancho de la tabla automáticamente
                        doc.content[2].table.widths = Array(doc.content[2].table.body[0].length).fill('*');

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
                }
            ],
            columnDefs: [
                {
                    targets: [0], // La columna de usuario (index 0)
                    visible: false,
                    searchable: true
                },
                {
                    targets: [3, 4],
                    visible: false, // La ocultamos
                    searchable: true
                }
            ],
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

        // Función para calcular y actualizar los totales del reporte de cobros
        function calcularTotalesCobros() {
            const table = $('#reporteCobrosTable').DataTable();
            
            // Obtener solo las filas visibles (después de filtros)
            const filasVisibles = table.rows({ search: 'applied' }).data();
            
            let totalRegistros = 0;
            let totalMonto = 0;
            
            // Contar registros
            totalRegistros = filasVisibles.length;
            
            // Iterar sobre las filas visibles para sumar los montos
            filasVisibles.each(function (row) {
                // Estructura de columnas según el HTML:
                // 0: Usuario (oculto), 1: #Cobro, 2: Inquilino, 3: IdInquilino (oculto), 
                // 4: IdInmueble (oculto), 5: Monto, 6: Concepto, 7: Fecha
                
                // Limpiar y convertir monto (columna 5)
                const montoStr = row[5] ? row[5].toString() : '0';
                const monto = parseFloat(montoStr.replace(/[^\d.-]/g, '')) || 0;
                totalMonto += monto;
            });
            
            // Formatear números con formato 1,000.00
            function formatearMonto(numero) {
                return 'RD$ ' + numero.toLocaleString('en-US', {
                    minimumFractionDigits: 2,
                    maximumFractionDigits: 2
                });
            }
            
            // Actualizar los elementos HTML
            $('#totalRegistrosCobros').text(totalRegistros.toLocaleString('en-US'));
            $('#totalCobros').text(formatearMonto(totalMonto));
            
            console.log('[TOTALES COBROS]', {
                registros: totalRegistros,
                monto: totalMonto
            });
        }
        
        // Calcular totales iniciales
        calcularTotalesCobros();
        
        // Recalcular totales cuando la tabla se redibuja (filtros, búsquedas, etc.)
        table.on('draw', function() {
            calcularTotalesCobros();
        });

    } catch (error) {
        if (error.status === 403) { // Evitar mostrar toast si ya se mostró el alert
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
        } else if (error.status === 404) {
            showToast('Reporte no encontrado', 'error');
        }
    }
};

/////// Función para cargar el reporte de atrasos ///////
async function cargarReporteAtrasos() {
    const spinner = document.getElementById("loadingSpinner");
    const container = document.getElementById("dataTableContainer");

    spinner.style.display = "block";
    container.style.display = "none";

    const filtroNombreInquilino = $('#busquedaInquilino').val();
    const filtroInmueble = $('#busquedaInmueble').val();
    const cuotasMin = $('#cuotasMin').val();
    const cuotasMax = $('#cuotasMax').val();

    console.log('[FILTROS] Inquilino:', filtroNombreInquilino, '| Inmueble:', filtroInmueble,
        '| Cuotas Min:', cuotasMin, '| Cuotas Max:', cuotasMax);

    const queryParams = new URLSearchParams({
        filtroNombreInquilino: filtroNombreInquilino || '',
        filtroInmueble: filtroInmueble || '',
        cuotasMin: cuotasMin || '',
        cuotasMax: cuotasMax || ''
    });


    try {
        const html = await ajaxRequest({
            url: `/Reportes/ReporteAtrasos?${queryParams}`,
            type: 'GET',
            errorMessage: 'Error al cargar el reporte de atrasos',
            suppressPermissionToasts: false
        });

        container.innerHTML = html;
        container.style.display = "block";

        inicializarSelectInquilino('reporteAtrasosTable');
        inicializarSelectInmueble('reporteAtrasosTable');

        // Inicializar el DataTable con los datos cargados
        const table = $('#reporteAtrasosTable').DataTable({
            destroy: true,
            responsive: true,
            dom: '<"top"lf>Brt<"bottom"ip>',
            language: LenguajeEs,
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
                        columns: ':visible',
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

                        const table = $('#reporteAtrasosTable').DataTable();

                        // Encontrar los índices de las columnas necesarias
                        let montoIndex = -1;
                        let totalIndex = -1;

                        table.columns().every(function () {
                            const headerText = this.header().textContent.trim();
                            if (headerText === 'Monto Total Atraso') {
                                montoIndex = this.index();
                            } else if (headerText === 'Total') {
                                totalIndex = this.index();
                            }
                        });

                        // Calcular el total de la columna Monto Total Atraso
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

                            // Colocar "TOTAL:" en la columna Ubicación (índice 3)
                            totalRow[3] = { text: 'TOTAL:', bold: true, alignment: 'right' };

                            // Colocar el monto total en la columna Monto (índice 4)
                            totalRow[4] = { text: totalFormateado, bold: true, alignment: 'right' };

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
                            return (i === doc.content[2].table.body.length - 2) ? 1 : 0.5;
                        };
                        objLayout['vLineWidth'] = function (i) { return 0.5; };
                        objLayout['hLineColor'] = function (i) { return '#aaa'; };
                        objLayout['vLineColor'] = function (i) { return '#aaa'; };
                        objLayout['paddingLeft'] = function (i) { return 4; };
                        objLayout['paddingRight'] = function (i) { return 4; };
                        doc.content[2].layout = objLayout;
                    }
                }
            ],
            lengthMenu: [10, 25, 50, 100],
            pageLength: 10,
            columnDefs: [
                {
                    targets: [0],
                    visible: false,
                    searchable: true
                },
                {
                    targets: [1, 2, 3, 4, 5],
                    visible: true,
                    searchable: true
                }
            ],
        });

        // Función para calcular y actualizar los totales del reporte de atrasos
        function calcularTotalesAtrasos() {
            const table = $('#reporteAtrasosTable').DataTable();
            
            // Obtener solo las filas visibles (después de filtros)
            const filasVisibles = table.rows({ search: 'applied' }).data();
            
            let totalRegistros = 0;
            let totalAtrasos = 0;
            let totalMora = 0;
            let granTotal = 0;
            
            // Contar registros
            totalRegistros = filasVisibles.length;
            
            // Iterar sobre las filas visibles para sumar los montos
            filasVisibles.each(function (row) {
                // Estructura de columnas según el HTML:
                // 0: Usuario (oculto), 1: Inquilino, 2: Inmueble, 3: Dirección, 
                // 4: Cant. Cuotas Atrasadas, 5: Monto Total Atraso, 6: Mora, 7: Total
                
                // Limpiar y convertir monto total atraso (columna 5)
                const montoAtrasStr = row[5] ? row[5].toString() : '0';
                const montoAtrasos = parseFloat(montoAtrasStr.replace(/[^\d.-]/g, '')) || 0;
                totalAtrasos += montoAtrasos;
                
                // Limpiar y convertir mora (columna 6)
                const moraStr = row[6] ? row[6].toString() : '0';
                const mora = parseFloat(moraStr.replace(/[^\d.-]/g, '')) || 0;
                totalMora += mora;
                
                // Limpiar y convertir total (columna 7)
                const totalStr = row[7] ? row[7].toString() : '0';
                const total = parseFloat(totalStr.replace(/[^\d.-]/g, '')) || 0;
                granTotal += total;
            });
            
            // Formatear números con formato 1,000.00
            function formatearMonto(numero) {
                return 'RD$ ' + numero.toLocaleString('en-US', {
                    minimumFractionDigits: 2,
                    maximumFractionDigits: 2
                });
            }
            
            // Actualizar los elementos HTML
            $('#totalRegistros').text(totalRegistros.toLocaleString('en-US'));
            $('#totalAtrasos').text(formatearMonto(totalAtrasos));
            $('#totalMora').text(formatearMonto(totalMora));
            $('#granTotal').text(formatearMonto(granTotal));
            
            console.log('[TOTALES ATRASOS]', {
                registros: totalRegistros,
                atrasos: totalAtrasos,
                mora: totalMora,
                granTotal: granTotal
            });
        }
        
        // Calcular totales iniciales
        calcularTotalesAtrasos();
        
        // Recalcular totales cuando la tabla se redibuja (filtros, búsquedas, etc.)
        table.on('draw', function() {
            calcularTotalesAtrasos();
        });

        // Depuración: Mostrar estructura de columnas y datos
        console.groupCollapsed('[DEPURACIÓN] Estructura de la tabla');
        console.log("Cabeceras de columnas:");
        table.columns().every(function (index) {
            console.log(`Columna ${index}:`, this.header().textContent.trim());
        });

        console.log("\nPrimeras 5 filas de datos:");
        table.rows().data().each(function (row, index) {
            if (index < 5) console.log(`Fila ${index}:`, row);
        });
        console.groupEnd();

        // Modificar el filtro personalizado
        // Modificar el filtro personalizado
        $.fn.dataTable.ext.search.push(
            function (settings, data, dataIndex) {
                const nombreInquilino = $('#busquedaInquilino').select2('data')[0]?.text || '';
                const filtroInmueble = $('#busquedaInmueble').val() || '';
                const cuotasMin = parseInt($('#cuotasMin').val()) || 0;
                const cuotasMax = parseInt($('#cuotasMax').val()) || Infinity;

                // Columnas según la estructura de la tabla:
                const nombreInquilinoFila = data[1]; // Columna 1: Nombre completo
                const nombreInmueble = data[2];
                const cuotasAtrasadas = parseInt(data[4]) || 0; // Columna 4: Cant. Cuotas Atrasadas

                const matchInquilino = nombreInquilino === '' ||
                    nombreInquilinoFila === nombreInquilino;
                const matchInmueble = filtroInmueble === '' ||
                    nombreInmueble === filtroInmueble;
                const matchCuotas = cuotasAtrasadas >= cuotasMin &&
                    cuotasAtrasadas <= cuotasMax;

                return matchInquilino && matchInmueble && matchCuotas;
            }
        );

        $('#cuotasMin, #cuotasMax').on('change', function () {
            const min = parseInt($('#cuotasMin').val()) || 0;
            const max = parseInt($('#cuotasMax').val()) || Infinity;

            if (min > max) {
                showToast('El valor mínimo no puede ser mayor que el máximo', 'warning');
                $(this).val('');
            } else {
                $('#reporteAtrasosTable').DataTable().draw();
            }
        });

        // Eventos para los selectores con depuración
        $('#busquedaInquilino, #busquedaInmueble').on('change', function () {
            const tipo = $(this).attr('id') === 'busquedaInquilino' ? 'INQUILINO' : 'INMUEBLE';
            const valor = $(this).val();
            console.log(`[CAMBIO SELECTOR] ${tipo}:`, valor);

            // Verificar qué está recibiendo realmente el filtro
            const table = $('#reporteAtrasosTable').DataTable();
            console.log('Datos visibles antes de filtrar:', table.rows({ search: 'applied' }).count());

            table.draw();

            console.log('Datos visibles después de filtrar:', table.rows({ search: 'applied' }).count());
        });

        $('#btnResetFiltros').on('click', function () {
            console.log('[RESET] Limpiando filtros...');

            // Limpiar selects
            $('#busquedaInquilino').val(null).trigger('change');
            $('#busquedaInmueble').val(null).trigger('change');
            $('#cuotasMin').val('');
            $('#cuotasMax').val('');

            // Limpiar campos ocultos
            $('#FidInquilino').val('');
            $('#FidInmueble').val('');

            // Resetear DataTable
            const table = $('#reporteAtrasosTable').DataTable();
            table.search('').columns().search('').draw();

            console.log('[RESET] Filtros limpiados. Datos visibles:', table.rows({ search: 'applied' }).count());

            showToast('Filtros limpiados correctamente', 'success');
        });

    } catch (error) {
        console.error('Error al cargar reporte:', error);
        if (error.status === 403) {
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
        } else if (error.status === 404) {
            showToast('Reporte no encontrado', 'error');
        }
    }
}


/////// Función para cargar el reporte de estado de cuenta ///////
async function cargarVistaReporteEstadoCuenta() {
    const spinner = document.getElementById("loadingSpinner");
    const container = document.getElementById("dataTableContainer");

    spinner.style.display = "block";
    container.style.display = "none";

    try {
        const html = await ajaxRequest({
            url: `/Reportes/VistaReporteEstadoCuenta`,
            type: 'GET',
            errorMessage: 'Error al cargar el reporte de estado de cuenta',
            suppressPermissionToasts: false
        });

        container.innerHTML = html;
        container.style.display = "block";

        inicializarSelectInquilino();
        inicializarEventosReset(); // Mover la inicialización del reset aquí

    } catch (error) {
        if (error.status === 403) {
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
        } else if (error.status === 404) {
            showToast('Reporte no encontrado', 'error');
        } else {
            console.error('Error:', error);
            showToast('Error al cargar el reporte', 'error');
        }
    } finally {
        spinner.style.display = "none";
    }
}

// Función para inicializar los eventos de reset
function inicializarEventosReset() {
    $('#btnResetFiltros').off('click').on('click', function () {
        // Limpiar select2 de inquilino
        $('#busquedaInquilino').val(null).trigger('change');
        // Limpiar campo oculto
        $('#FidInquilino').val('');
        // Limpiar campos de fecha
        $('#fechaInicioFiltro').val('');
        $('#fechaFinFiltro').val('');

        showToast('Filtros limpiados correctamente', 'success');
    });
}


$(document).on('click', '#btnEC', function () {
    const idInquilino = $('#FidInquilino').val(); // Usar el campo oculto
    const fechaInicio = $('#fechaInicioFiltro').val();
    const fechaFin = $('#fechaFinFiltro').val();

    console.log('Datos capturados en el click del botón:');
    console.log('ID Inquilino (del campo oculto):', idInquilino, 'Tipo:', typeof idInquilino);
    console.log('Texto mostrado en select2:', $('#busquedaInquilino').select2('data')[0]?.text);

    if (!idInquilino) {
        showToast('Debe seleccionar un inquilino', 'warning');
        return;
    }

    if (!fechaInicio || !fechaFin) {
        showToast('Debe seleccionar ambas fechas', 'warning');
        return;
    }

    if (new Date(fechaInicio) > new Date(fechaFin)) {
        showToast('La fecha inicial no puede ser mayor que la fecha final', 'warning');
        return;
    }

    cargarReporteEstadoCuenta(idInquilino, fechaInicio, fechaFin);
});

async function cargarReporteEstadoCuenta(idInquilino, fechaInicio, fechaFin) {
    const spinner = document.getElementById("loadingSpinner");
    const resultsContainer = document.getElementById("reporteEstadoCuentaResults");

    console.log('[DEBUG] Parámetros recibidos en cargarReporteEstadoCuenta:');
    console.log('idInquilino:', idInquilino, 'Tipo:', typeof idInquilino);
    console.log('fechaInicio:', fechaInicio, 'Tipo:', typeof fechaInicio);
    console.log('fechaFin:', fechaFin, 'Tipo:', typeof fechaFin);

    spinner.style.display = "block";
    resultsContainer.innerHTML = ''; // Limpiar resultados anteriores

    try {
        // Convertir y validar parámetros
        const idInquilinoNum = idInquilino ? parseInt(idInquilino) : null;
        const fechaInicioDate = fechaInicio ? new Date(fechaInicio) : null;
        const fechaFinDate = fechaFin ? new Date(fechaFin) : null;

        console.log('[DEBUG] Parámetros convertidos:');
        console.log('idInquilinoNum:', idInquilinoNum, 'Tipo:', typeof idInquilinoNum);
        console.log('fechaInicioDate:', fechaInicioDate, 'Tipo:', typeof fechaInicioDate);
        console.log('fechaFinDate:', fechaFinDate, 'Tipo:', typeof fechaFinDate);

        // Construir los parámetros de consulta
        const params = new URLSearchParams();
        if (idInquilinoNum) params.append('FidInquilino', idInquilinoNum);
        if (fechaInicioDate) params.append('fechaInicio', fechaInicioDate.toISOString());
        if (fechaFinDate) params.append('fechaFin', fechaFinDate.toISOString());

        console.log('[DEBUG] Parámetros que se enviarán:', params.toString());

        const response = await $.ajax({
            url: `/Reportes/ReporteEstadoCuenta?${params.toString()}`,
            type: 'GET',
            dataType: 'json',
            error: function (xhr, status, error) {
                console.error('[DEBUG] Error en la solicitud AJAX:', {
                    status: xhr.status,
                    statusText: xhr.statusText,
                    responseText: xhr.responseText,
                    error: error
                });
                throw { status: xhr.status, responseJSON: xhr.responseJSON };
            }
        });

        console.log('[DEBUG] Respuesta del servidor:', response);

        if (response.success) {
            console.log('[DEBUG] Datos recibidos para renderizar:', response.data);
            renderizarReporteEstadoCuenta(response.data);
        } else {
            showToast(response.message || 'Error al cargar el reporte', 'error');
        }

    } catch (error) {
        console.error('[DEBUG] Error en cargarReporteEstadoCuenta:', error);

        if (error.status === 403) {
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
        } else if (error.status === 404) {
            showToast('Reporte no encontrado', 'error');
        } else if (error.status === 400) {
            showToast('Datos de solicitud inválidos: ' + (error.responseJSON?.message || ''), 'error');
        } else {
            showToast('Error al cargar el reporte', 'error');
        }
    } finally {
        spinner.style.display = "none";
        console.log('[DEBUG] Carga del reporte finalizada');
    }
}

function renderizarReporteEstadoCuenta(data) {
    const container = document.getElementById("reporteEstadoCuentaResults");

    // Función auxiliar para formatear números como moneda
    const formatearMoneda = (valor) => {
        return parseFloat(valor || 0).toLocaleString('en-US', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    };

    // Función auxiliar para formatear fechas como dd/mm/yyyy
    const formatearFecha = (fecha) => {
        if (!fecha) return '';
        const date = new Date(fecha);
        const day = date.getDate().toString().padStart(2, '0');
        const month = (date.getMonth() + 1).toString().padStart(2, '0');
        const year = date.getFullYear();
        return `${day}/${month}/${year}`;
    };

    // Funciones auxiliares para cálculos
    const calcularTotalPendientes = (pendientes) => {
        return formatearMoneda(pendientes.reduce((total, cuota) => total + (cuota.fmonto || 0) + (cuota.fmora || 0), 0));
    };

    const calcularTotalPagado = (pagos) => {
        return formatearMoneda(pagos.reduce((total, pago) => total + (pago.fmonto || 0), 0));
    };

    // Funciones para renderizar tablas
    const renderizarTablaPendientes = (pendientes) => {
        if (!pendientes || pendientes.length === 0) return '<div class="alert alert-info">No hay cuotas pendientes</div>';

        return `
            <div class="table-responsive">
                <table class="table table-hover table-sm">
                    <thead class="table-light">
                        <tr>
                            <th># Cuota</th>
                            <th>Vencimiento</th>
                            <th>Monto</th>
                            <th>Mora</th>
                            <th>Días Atraso</th>
                            <th>Total</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${pendientes.map(cuota => `
                            <tr>
                                <td>${cuota.fNumeroCuota || ''}</td>
                                <td>${formatearFecha(cuota.fvence)}</td>
                                <td>$${formatearMoneda(cuota.fmonto)}</td>
                                <td>$${formatearMoneda(cuota.fmora)}</td>
                                <td>${cuota.fdiasAtraso || 0}</td>
                                <td>$${formatearMoneda((cuota.fmonto || 0) + (cuota.fmora || 0))}</td>
                            </tr>
                        `).join('')}
                    </tbody>
                    <tfoot class="table-light">
                        <tr>
                            <th colspan="2">Total</th>
                            <th>$${formatearMoneda(pendientes.reduce((sum, c) => sum + (c.fmonto || 0), 0))}</th>
                            <th>$${formatearMoneda(pendientes.reduce((sum, c) => sum + (c.fmora || 0), 0))}</th>
                            <th></th>
                            <th>$${calcularTotalPendientes(pendientes)}</th>
                        </tr>
                    </tfoot>
                </table>
            </div>
        `;
    };

    const renderizarTablaPagos = (pagos) => {
        if (!pagos || pagos.length === 0) return '<div class="alert alert-info">No hay pagos registrados</div>';

        return `
            <div class="table-responsive">
                <table class="table table-hover table-sm">
                    <thead class="table-light">
                        <tr>
                            <th>Fecha</th>
                            <th>Concepto</th>
                            <th>Monto</th>
                            <th>Descuento</th>
                            <th>Cargos</th>
                            <th>Total</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${pagos.map(pago => `
                            <tr>
                                <td>${formatearFecha(pago.ffecha)}</td>
                                <td>${pago.fconcepto || ''}</td>
                                <td>$${formatearMoneda(pago.fmonto)}</td>
                                <td>$${formatearMoneda(pago.fdescuento)}</td>
                                <td>$${formatearMoneda(pago.fcargos)}</td>
                                <td>$${formatearMoneda((pago.fmonto || 0) - (pago.fdescuento || 0) + (pago.fcargos || 0))}</td>
                            </tr>
                        `).join('')}
                    </tbody>
                    <tfoot class="table-light">
                        <tr>
                            <th colspan="2">Total</th>
                            <th>$${formatearMoneda(pagos.reduce((sum, p) => sum + (p.fmonto || 0), 0))}</th>
                            <th>$${formatearMoneda(pagos.reduce((sum, p) => sum + (p.fdescuento || 0), 0))}</th>
                            <th>$${formatearMoneda(pagos.reduce((sum, p) => sum + (p.fcargos || 0), 0))}</th>
                            <th>$${formatearMoneda(pagos.reduce((sum, p) => sum + (p.fmonto || 0) - (p.fdescuento || 0) + (p.fcargos || 0), 0))}</th>
                        </tr>
                    </tfoot>
                </table>
            </div>
        `;
    };

    const renderizarTablaCuotas = (cuotas) => {
        if (!cuotas || cuotas.length === 0) return '<div class="alert alert-info">No hay cuotas registradas</div>';

        return `
            <div class="table-responsive">
                <table class="table table-hover table-sm">
                    <thead class="table-light">
                        <tr>
                            <th># Cuota</th>
                            <th>Vencimiento</th>
                            <th>Monto</th>
                            <th>Saldo</th>
                            <th>Mora</th>
                            <th>Estado</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${cuotas.map(cuota => `
                            <tr>
                                <td>${cuota.fNumeroCuota || ''}</td>
                                <td>${formatearFecha(cuota.fvence)}</td>
                                <td>$${formatearMoneda(cuota.fmonto)}</td>
                                <td>$${formatearMoneda(cuota.fsaldo)}</td>
                                <td>$${formatearMoneda(cuota.fmora)}</td>
                                <td>
                                    <span class="badge ${(cuota.fstatus) === 'N' ? 'bg-warning' : (cuota.fstatus) === 'S' ? 'bg-success' : 'bg-danger'}">
                                        ${(cuota.fstatus) === 'N' ? 'Pendiente' : (cuota.fstatus) === 'S' ? 'Pagado' : 'Vencido'}
                                    </span>
                                </td>
                            </tr>
                        `).join('')}
                    </tbody>
                </table>
            </div>
        `;
    };

    // Plantilla HTML para el reporte
    const html = `
        <div class="card border-0 shadow-sm mt-4">
            <div class="card-body p-4">
                <!-- Encabezado del reporte -->
                <div class="d-flex justify-content-between align-items-center mb-4">
                    <h4 class="mb-0">
                        <i class="fas fa-file-invoice-dollar me-2 text-danger"></i>
                        Estado de Cuenta - ${data.nombreInquilino || 'Inquilino'}
                    </h4>
                    <button class="btn btn-sm btn-outline-danger" onclick="exportarReporte()">
                        <i class="fas fa-file-export me-1"></i> Exportar
                    </button>
                </div>
                
                <!-- Información del inquilino -->
                <div class="row mb-4">
                    <div class="col-md-6">
                        <div class="card bg-light">
                            <div class="card-body REC">
                                <h5 class="card-title">Información del Inquilino</h5>
                                <p class="mb-1"><strong>Nombre:</strong> ${data.nombreInquilino || 'No especificado'}</p>
                                <p class="mb-1"><strong>Inmueble:</strong> ${data.descripcionInmueble || 'No especificado'}</p>
                                <p class="mb-0"><strong>Dirección:</strong> ${data.direccionInmueble || 'No especificado'}</p>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-6">
                        <div class="card bg-light">
                            <div class="card-body resumen">
                                <h5 class="card-title">Resumen</h5>
                                <p class="mb-1"><strong>Total Pendiente:</strong> $${calcularTotalPendientes(data.pendientes || [])}</p>
                                <p class="mb-1"><strong>Total Pagado:</strong> $${calcularTotalPagado(data.pagos || [])}</p>
                                <p class="mb-0"><strong>Fecha del Reporte:</strong> ${data.fechaActual ? (data.fechaActual) : ''} ${data.horaActual || ''}</p>
                            </div>
                        </div>
                    </div>
                </div>
                
                <!-- Pestañas para diferentes secciones -->
                <ul class="nav nav-tabs mb-4" id="reporteTabs" role="tablist">
                    <li class="nav-item" role="presentation">
                        <button class="nav-link active" id="pendientes-tab" data-bs-toggle="tab" data-bs-target="#pendientes" type="button" role="tab">
                            Pendientes (${data.pendientes ? data.pendientes.length : data.Pendientes ? data.Pendientes.length : 0})
                        </button>
                    </li>
                    <li class="nav-item" role="presentation">
                        <button class="nav-link" id="pagos-tab" data-bs-toggle="tab" data-bs-target="#pagos" type="button" role="tab">
                            Pagos (${data.pagos ? data.pagos.length : data.Pagos ? data.Pagos.length : 0})
                        </button>
                    </li>
                    <li class="nav-item" role="presentation">
                        <button class="nav-link" id="cuotas-tab" data-bs-toggle="tab" data-bs-target="#cuotas" type="button" role="tab">
                            Todas las Cuotas (${data.cuotas ? data.cuotas.length : data.Cuotas ? data.Cuotas.length : 0})
                        </button>
                    </li>
                </ul>
                
                <!-- Contenido de las pestañas -->
                <div class="tab-content" id="reporteTabsContent">
                    <!-- Pestaña Pendientes -->
                    <div class="tab-pane fade show active" id="pendientes" role="tabpanel">
                        ${renderizarTablaPendientes(data.pendientes || [])}
                    </div>
                    
                    <!-- Pestaña Pagos -->
                    <div class="tab-pane fade" id="pagos" role="tabpanel">
                        ${renderizarTablaPagos(data.pagos || [])}
                    </div>
                    
                    <!-- Pestaña Todas las Cuotas -->
                    <div class="tab-pane fade" id="cuotas" role="tabpanel">
                        ${renderizarTablaCuotas(data.cuotas || [])}
                    </div>
                </div>
            </div>
        </div>
    `;

    container.innerHTML = html;

    // Inicializar tooltips
    if (typeof $ !== 'undefined' && $.fn.tooltip) {
        $('[data-bs-toggle="tooltip"]').tooltip();
    }

    // Inicializar DataTables si es necesario
    if (typeof $ !== 'undefined' && $.fn.DataTable) {
        $('.table').DataTable({
            responsive: true,
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
            dom: '<"top"f>rt<"bottom"lip><"clear">',
            pageLength: 10
        });
    }
}

// Funcion para exportar el reporte a PDF con jsPDF y html2Canva 
function exportarReporte() {
    // Importar las librerías dinámicamente si no están cargadas
    const loadScripts = () => {
        return new Promise((resolve) => {
            if (window.jspdf && window.html2canvas) {
                resolve();
                return;
            }

            const scripts = [
                'https://cdnjs.cloudflare.com/ajax/libs/jspdf/2.5.1/jspdf.umd.min.js',
                'https://cdnjs.cloudflare.com/ajax/libs/html2canvas/1.4.1/html2canvas.min.js'
            ];

            let loaded = 0;

            scripts.forEach(src => {
                const script = document.createElement('script');
                script.src = src;
                script.onload = () => {
                    loaded++;
                    if (loaded === scripts.length) resolve();
                };
                document.head.appendChild(script);
            });
        });
    };

    loadScripts().then(() => {
        const { jsPDF } = window.jspdf;
        const element = document.getElementById("reporteEstadoCuentaResults");

        // Ocultar el botón de exportar para que no aparezca en el PDF
        const exportButton = element.querySelector('button[onclick="exportarReporte()"]');
        const originalDisplay = exportButton.style.display;
        exportButton.style.display = 'none';

        html2canvas(element, {
            scale: 2, // Mayor calidad
            logging: false,
            useCORS: true,
            allowTaint: true,
            scrollY: -window.scrollY
        }).then(canvas => {
            // Restaurar el botón
            exportButton.style.display = originalDisplay;

            const imgData = canvas.toDataURL('image/png');
            const pdf = new jsPDF('l', 'mm', 'letter'); // 'l' para landscape, 'letter' para tamaño carta
            const imgWidth = 279.4; // Ancho de letter en mm (11 pulgadas)
            const pageHeight = 215.9; // Altura de letter en mm (8.5 pulgadas)
            const imgHeight = canvas.height * imgWidth / canvas.width;
            let heightLeft = imgHeight;
            let position = 0;

            pdf.addImage(imgData, 'PNG', 0, position, imgWidth, imgHeight);
            heightLeft -= pageHeight;

            // Añadir páginas adicionales si el contenido es muy largo
            while (heightLeft >= 0) {
                position = heightLeft - imgHeight;
                pdf.addPage();
                pdf.addImage(imgData, 'PNG', 0, position, imgWidth, imgHeight);
                heightLeft -= pageHeight;
            }

            // Guardar el PDF
            pdf.save(`Estado_de_Cuenta_${new Date().toISOString().slice(0, 10)}.pdf`);
        });
    }).catch(error => {
        console.error('Error al cargar las librerías:', error);
        alert('Error al generar el PDF. Por favor intente nuevamente.');
    });
}


function inicializarSelectInquilino() {
    const $selectInquilino = $('#busquedaInquilino');

    if ($selectInquilino.hasClass('select2-hidden-accessible')) {
        $selectInquilino.select2('destroy');
    }

    $selectInquilino.empty().select2({
        placeholder: "Buscar inquilino...",
        allowClear: true,
        matcher: matchCustom,
        ajax: {
            url: '/Reportes/BuscarInquilino',
            dataType: 'json',
            processResults: function (data) {
                return {
                    results: data.map(item => ({
                        id: item.id, // Usar el texto como Id
                        text: item.text
                    }))
                };
            }
        }
    }).on('change', function () {
        $('#FidInquilino').val($(this).val());
    });
}

function inicializarSelectInmueble() {
    const $selectInmueble = $('#busquedaInmueble');
    const valorSeleccionado = $selectInmueble.val();

    if ($selectInmueble.hasClass('select2-hidden-accessible')) {
        $selectInmueble.select2('destroy');
    }

    $selectInmueble.empty().select2({
        placeholder: "Buscar inmueble...",
        allowClear: true,
        matcher: matchCustom
    });

    $.ajax({
        url: '/Reportes/BuscarInmueble',
        dataType: 'json',
        success: function (data) {
            $selectInmueble.append(new Option('', '', false, false));

            data.forEach(item => {
                const option = new Option(item.text, item.id, false, false);
                $selectInmueble.append(option);
            });

            if (valorSeleccionado) {
                $selectInmueble.val(valorSeleccionado).trigger('change');
            }

            // Actualizar campo oculto cuando cambia el select2
            $selectInmueble.on('change', function () {
                $('#FidInmueble').val($(this).val());
            });
        },
        error: function () {
            showToast('Error al cargar los inmuebles', 'error');
        }
    });
}


/////// Función para cargar el reporte de estado de gasto ///////
async function cargarReporteEstadoGasto(fechaInicio, fechaFin) {
    const spinner = document.getElementById("loadingSpinner");
    const container = document.getElementById("dataTableContainer");

    spinner.style.display = "block";
    container.style.display = "none";

    try {
        // Construir los parámetros de consulta
        const params = new URLSearchParams();
        if (fechaInicio) params.append('fechaInicio', fechaInicio);
        if (fechaFin) params.append('fechaFin', fechaFin);

        const html = await ajaxRequest({
            url: `/Reportes/ReporteCuentaGasto?${params.toString()}`,
            type: 'GET',
            errorMessage: 'Error al cargar el reporte de estado de gasto',
            suppressPermissionToasts: false
        });

        container.innerHTML = html;
        container.style.display = "block";

        // Inicializar el DataTable
        const table = $('#reporteEstadoGastoTable').DataTable({
            destroy: true,
            responsive: true,
            dom: '<"top"lf>Brt<"bottom"ip>',
            language: LenguajeEs,
            buttons: [
                {
                    extend: 'excelHtml5',
                    text: '<i class="fas fa-file-excel"></i> Excel',
                    className: 'btn btn-outline-success btn-sm'
                },
                {
                    extend: 'pdfHtml5',
                    text: '<i class="fas fa-file-pdf"></i> PDF',
                    className: 'btn btn-outline-danger btn-sm',
                    orientation: 'vertical',
                    pageSize: 'LETTER',
                    exportOptions: {
                        columns: ':visible',
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

                        const table = $('#reporteEstadoCuentaTable').DataTable();

                        // Contar registros (filas visibles después de búsqueda/filtrado) - CORRECCIÓN AQUÍ
                        const totalRegistros = 1;

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
                            return (i === doc.content[2].table.body.length - 2) ? 1 : 0.5;
                        };
                        objLayout['vLineWidth'] = function (i) { return 0.5; };
                        objLayout['hLineColor'] = function (i) { return '#aaa'; };
                        objLayout['vLineColor'] = function (i) { return '#aaa'; };
                        objLayout['paddingLeft'] = function (i) { return 4; };
                        objLayout['paddingRight'] = function (i) { return 4; };
                        doc.content[2].layout = objLayout;
                    }
                }
            ]
        });

        // Solo restaurar valores si no estamos haciendo un reset
        if (fechaInicio !== null && fechaFin !== null) {
            if (window.tempFechaInicio) {
                $('#fechaInicioFiltro').val(window.tempFechaInicio);
            }
            if (window.tempFechaFin) {
                $('#fechaFinFiltro').val(window.tempFechaFin);
            }
        } else {
            // Limpiar las variables temporales cuando se hace reset
            window.tempFechaInicio = '';
            window.tempFechaFin = '';
        }

        // Botón reset - mejorado
        $('#btnResetFiltros').off('click').on('click', function () {
            // Limpiar campos
            $('#fechaInicioFiltro').val('');
            $('#fechaFinFiltro').val('');

            // Limpiar variables temporales
            window.tempFechaInicio = '';
            window.tempFechaFin = '';

            // Recargar sin filtros
            cargarReporteEstadoGasto(null, null);
        });

    } catch (error) {
        if (error.status === 403) {
            showPermissionAlert(error.responseJSON?.error || 'No tiene permisos para esta acción');
        } else if (error.status === 404) {
            showToast('Reporte no encontrado', 'error');
        }
    }
}

// Boton de generar
$(document).on('click', '#btnGenerarReporte', function () {
    const fechaInicio = $('#fechaInicioFiltro').val();
    const fechaFin = $('#fechaFinFiltro').val();

    if (!fechaInicio || !fechaFin) {
        showToast('Debe seleccionar ambas fechas', 'warning');
        return;
    }

    if (new Date(fechaInicio) > new Date(fechaFin)) {
        showToast('La fecha inicial no puede ser mayor que la fecha final', 'warning');
        return;
    }

    // Guardar los valores en variables temporales
    window.tempFechaInicio = fechaInicio;
    window.tempFechaFin = fechaFin;

    cargarReporteEstadoGasto(fechaInicio, fechaFin);
});

/////// Función para inicializar solo el selector de usuarios ///////
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
        url: '/Reportes/BuscarUsuario',
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



