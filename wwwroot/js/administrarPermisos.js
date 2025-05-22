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
    });

    // Cerrar la card de permisos
    $('#closePermissionsCard, #cancelPermissions').on('click', function () {
        $('#permissionsCard').fadeOut();
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
        checkboxes.prop('checked', !allChecked);

        // Actualizar el texto del botón
        $(this).html(`<i class="fas fa-${!allChecked ? 'times' : 'check'}-circle"></i> ${!allChecked ? 'Deseleccionar' : 'Seleccionar'} todos`);
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

        $.ajax({
            url: '/AdministrarPermisos/UpdatePermissions',
            type: 'POST',
            contentType: 'application/json',
            headers: {
                'RequestVerificationToken': token
            },
            data: JSON.stringify(requestData),
            success: function (response) {
                if (response.success) {
                    showToast('Permisos actualizados correctamente', 'success');
                    $('#permissionsCard').fadeOut();
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