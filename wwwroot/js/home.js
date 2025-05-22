function loadDashboard() {
    $.ajax({
        url: '/Home/LoadDashboard',
        type: 'GET',
        success: function (response) {
            $('#dashboard-container').html(response);
        },
        error: function (xhr, status, error) {
            console.error('Error al cargar el dashboard:', error);
            $('#dashboard-container').html('<div class="alert alert-danger">Error al cargar el dashboard. Intente recargar la página.</div>');
        }
    });
}

function loadPartialView(url) {
    $.ajax({
        url: url,
        type: 'GET',
        success: function (response) {
            $('#dashboard-container').html(response);
        },
        error: function (xhr, status, error) {
            console.error('Error al cargar la vista parcial:', error);
            $('#dashboard-container').html('<div class="alert alert-danger">Error al cargar la vista. Intente nuevamente.</div>');
        }
    });
}

// Eventos para los botones de acción rápida
$(document).on('click', '.action-btn', function () {
    const action = $(this).find('span').text().trim();
    console.log('Acción seleccionada:', action);

    switch (action) {
        case 'Crear Inquilino':
            loadPartialView('/Home/LoadCreateInquilinoPartial');
            break;
        case 'Crear Propietario':
            loadPartialView('/Home/LoadCreatePropietarioPartial');
            break;
        case 'Crear Inmueble':
            loadPartialView('/Home/LoadCreateInmueblePartial');
            break;
        case 'Crear CxC':
            loadPartialView('/Home/LoadCreateCxCPartial');
            break;
        case 'Crear Cobro':
            loadPartialView('/Home/LoadCreateCobroPartial');
            break;
        default:
            console.log('Acción no reconocida:', action);
    }
});

$(document).ready(function () {
    loadDashboard();
    setInterval(loadDashboard, 50000); // Opcional: Recargar cada cierto tiempo
});