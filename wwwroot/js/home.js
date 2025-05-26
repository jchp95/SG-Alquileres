$(document).ready(function () {
    let currentPage = 1;
    const pageSize = 10; // Probamos con 3 actividades por página
    let isLoading = false;
    let hasMore = true;

    // Cuando se abre el offcanvas, cargar las primeras actividades
    $('#activitiesOffcanvas').on('show.bs.offcanvas', function () {
        currentPage = 1;
        hasMore = true; // Resetear el flag al abrir
        $('#loadMoreActivities').show(); // Mostrar el botón inicialmente
        loadActivities(currentPage);
    });

    // Manejar clic en "Ver más"
    $('#loadMoreActivities').click(function () {
        if (!isLoading && hasMore) {
            currentPage++;
            loadActivities(currentPage);
        }
    });

    function loadActivities(page) {
        isLoading = true;
        $('#loadMoreActivities').prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-2"></i>Cargando...');

        $.get('/Home/GetActivities', { page: page, pageSize: pageSize }, function (data) {
            if (page === 1) {
                $('#allActivitiesList').html(data);
            } else {
                $('#allActivitiesList').append(data);
            }

            // Verificar si hay más datos (cambiamos la lógica)
            const itemsCount = $(data).filter('.activity-item').length;
            if (itemsCount === 0 || itemsCount < pageSize) {
                hasMore = false;
                $('#loadMoreActivities').hide();
            } else {
                hasMore = true;
                $('#loadMoreActivities').show();
            }

            isLoading = false;
            $('#loadMoreActivities').prop('disabled', false).html('<i class="fas fa-plus-circle me-2"></i>Ver más');
        }).fail(function () {
            isLoading = false;
            $('#loadMoreActivities').prop('disabled', false).html('<i class="fas fa-plus-circle me-2"></i>Ver más');
            alert('Error al cargar actividades');
        });
    }
});