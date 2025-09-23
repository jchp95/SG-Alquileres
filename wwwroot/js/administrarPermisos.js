$(document).ready(function () {
    let currentUserId = null;
    let permissionData = {};

    // Manejar clic en botón de gestionar permisos
    $(document).on('click', '.manage-permissions', function () {
        const userId = $(this).data('user-id');
        const userName = $(this).data('user-name');
        currentUserId = userId;

        // Mostrar el nombre del usuario en el título
        $('#selectedUserName').text(userName);

        // Cargar los permisos
        loadPermissions(userId);

        // Mostrar la card de permisos
        $('#permissionsCard').fadeIn();

        // Insertar el botón maestro al lado del nombre del usuario
        $('#selectedUserName').after(`
            <button type="button" class="btn btn-sm btn-outline-primary ms-2 master-permission-btn">
                <i class="fas fa-check-double"></i> Seleccionar todos los permisos
            </button>
        `);
    });

    // Cerrar la card de permisos
    $('#closePermissionsCard, #cancelPermissions').on('click', function () {
        $('#permissionsCard').fadeOut();
        $('.master-permission-btn').remove(); // Eliminar el botón maestro al cerrar
    });

    // Función para cargar los permisos via AJAX
    function loadPermissions(userId) {
        $.get(`/AdministrarPermisos/GetUserPermissions?userId=${userId}`, function (data) {
            permissionData = data;

            // Depuración: verificar datos recibidos
            console.log("Datos de permisos recibidos:", data);

            // Llenar cada pestaña con sus permisos correspondientes
            $.each(data.permissionCategories, function (category, permissions) {
                const normalizedCategory = category.replace(/\s+/g, '');
                const container = $(`#content-${normalizedCategory} .permission-items-container`);

                if (container.length === 0) {
                    console.error(`Contenedor no encontrado para categoría: ${category}`);
                    return;
                }

                container.empty();

                permissions.forEach(permission => {
                    const switchId = `perm-${permission.value.replace(/\./g, '-')}`;
                    const isChecked = permission.isSelected ? 'checked' : '';

                    container.append(`
                        <div class="permission-item">
                            <div class="form-check form-switch">
                                <input type="checkbox" class="form-check-input permission-switch" 
                                       id="${switchId}" 
                                       data-permission="${permission.value}"
                                       ${isChecked}>
                                <label class="form-check-label" for="${switchId}">
                                    <i class="fas ${getPermissionIcon(permission.value)}"></i>
                                    ${permission.displayName}
                                </label>
                            </div>
                        </div>
                    `);
                });

                // Agregar botón "Seleccionar todos" para cada categoría
                container.prepend(`
                    <div class="category-actions mb-3">
                        <button type="button" class="btn btn-sm btn-outline-primary select-all-btn" 
                                data-category="${category}">
                            <i class="fas fa-check-circle"></i> Seleccionar todos
                        </button>
                    </div>
                `);
            });

            // Configurar el evento click para el botón maestro
            $('.master-permission-btn').off('click').on('click', function () {
                const totalCheckboxes = $('.permission-switch').length;
                const checkedCheckboxes = $('.permission-switch:checked').length;
                const isAllChecked = checkedCheckboxes === totalCheckboxes;

                // Activar/desactivar todos los checkboxes de permisos
                $('.permission-switch').prop('checked', !isAllChecked).trigger('change');

                // Actualizar el texto del botón maestro
                $(this).html(`<i class="fas fa-check-double"></i> ${!isAllChecked ? 'Deseleccionar' : 'Seleccionar'} todos`);

                // Actualizar los botones "Seleccionar todos" de cada categoría
                $('.select-all-btn').each(function () {
                    const category = $(this).data('category');
                    const container = $(`#content-${category} .permission-items-container`);
                    const checkboxes = container.find('.permission-switch');
                    const allChecked = checkboxes.length === checkboxes.filter(':checked').length;

                    $(this).html(`<i class="fas fa-${allChecked ? 'times' : 'check'}-circle"></i> ${allChecked ? 'Deseleccionar' : 'Seleccionar'} todos`);
                });
            });

            // Actualizar el texto del botón maestro cuando cambien otros checkboxes
            $('.permission-switch').off('change').on('change', function () {
                const totalCheckboxes = $('.permission-switch').length;
                const checkedCheckboxes = $('.permission-switch:checked').length;
                const isAllChecked = checkedCheckboxes === totalCheckboxes;

                $('.master-permission-btn').html(`<i class="fas fa-check-double"></i> ${isAllChecked ? 'Deseleccionar' : 'Seleccionar'} todos`);
            });
        });
    }

    // Función auxiliar para obtener iconos
    function getPermissionIcon(permission) {
        const action = permission.split('.').pop();
        switch (action) {
            case 'Ver': return 'fa-eye';
            case 'Crear': return 'fa-plus-circle';
            case 'Editar': return 'fa-edit';
            case 'Anular': return 'fa-trash-alt';
            default: return 'fa-key';
        }
    }

    // Seleccionar todos los permisos de una categoría
    $(document).on('click', '.select-all-btn', function () {
        const category = $(this).data('category');
        const container = $(`#content-${category} .permission-items-container`);
        const checkboxes = container.find('.permission-switch');

        // Verificar si todos están seleccionados
        const allChecked = checkboxes.length === checkboxes.filter(':checked').length;

        // Toggle estado
        checkboxes.prop('checked', !allChecked).trigger('change');

        // Actualizar el texto del botón
        $(this).html(`<i class="fas fa-${!allChecked ? 'times' : 'check'}-circle"></i> ${!allChecked ? 'Deseleccionar' : 'Seleccionar'} todos`);

        // Actualizar el texto del botón maestro
        const totalCheckboxes = $('.permission-switch').length;
        const checkedCheckboxes = $('.permission-switch:checked').length;
        const isAllChecked = checkedCheckboxes === totalCheckboxes;
        $('.master-permission-btn').html(`<i class="fas fa-check-double"></i> ${isAllChecked ? 'Deseleccionar' : 'Seleccionar'} todos`);
    });

    // Guardar permisos
    $('#savePermissions').on('click', function () {
        if (!currentUserId) return;

        // Obtener el token antiforgery
        const token = $('input[name="__RequestVerificationToken"]').val();

        // Recopilar permisos seleccionados
        const selectedPermissions = [];
        $('.permission-switch:checked').each(function () {
            selectedPermissions.push($(this).data('permission'));
        });

        const requestData = {
            userId: currentUserId,
            selectedPermissions: selectedPermissions
        };

        console.log("Payload que se enviará:", JSON.stringify(requestData));

        $.ajax({
            url: '/AdministrarPermisos/UpdatePermissions',
            type: 'POST',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(requestData),
            headers: {
                'X-RequestVerificationToken': token   
            },
            success: function (response) {
                if (response.success) {
                    showToast('Permisos actualizados correctamente', 'success');
                    $('#permissionsCard').fadeOut();
                    $('.master-permission-btn').remove();
                } else {
                    const errors = response.errors ? response.errors.join(', ') : 'Error desconocido';
                    showToast('Error: ' + errors, 'error');
                }
            },
            error: function (xhr) {
                const errorMsg = xhr.responseJSON?.errors || xhr.statusText;
                console.error("Error completo:", xhr);
                showToast('Error al actualizar: ' + errorMsg, 'error');
            }
        });
    });

    // Búsqueda de usuarios
    $('#userSearch').on('keyup', function () {
        const value = $(this).val().toLowerCase();
        $('#usersTable tbody tr').filter(function () {
            $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
        });
    });

    // Mostrar notificación toast
    function showToast(message, type) {
        const toast = `<div class="toast ${type}">${message}</div>`;
        $('body').append(toast);
        setTimeout(() => $('.toast').remove(), 3000);
    }
});