$(document).ready(function () {
    // 1. Solicitud GET para cargar la tabla
    function cargarTablaCuotas() {
        $.ajax({
            url: '/TbCuotas/CargarCuota',
            type: 'GET',
            success: function (data) {
                $('#dataTableContainer').html(data).fadeIn();
                inicializarDataTable();
            },
            error: function () {
                alert('Error al cargar la tabla de cuotas. Inténtalo de nuevo más tarde.');
            }
        });
    }
    // Helpers
    function inicializarDataTable() {
        if ($.fn.DataTable.isDataTable('#cuotaTable')) {
            $('#cuotaTable').DataTable().destroy();
        }
        $('#cuotaTable').DataTable({
            stateSave: false,
        });
    }
    // Eventos
    $('#loadCuota').click(cargarTablaCuotas);
});