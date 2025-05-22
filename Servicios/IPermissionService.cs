// IPermissionService.cs
using System.Security.Claims;
using Alquileres.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public interface IPermissionService
{
    Task<List<string>> GetUserPermissionsAsync(string userId);
    Task<IdentityResult> UpdateUserPermissionsAsync(string userId, List<string> permissions);
    Task<List<IdentityUser>> GetAllUsersAsync();
    Task<Dictionary<string, List<PermissionViewModel>>> GetCategorizedPermissionsAsync(string userId);
    Task<List<string>> GetAllAvailablePermissionsAsync();
}

// PermissionService.cs
public class PermissionService : IPermissionService
{
    private readonly UserManager<IdentityUser> _userManager;

    public PermissionService(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<List<string>> GetUserPermissionsAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return new List<string>();

        var claims = await _userManager.GetClaimsAsync(user);
        return claims
            .Where(c => c.Type == "Permission")
            .Select(c => c.Value)
            .ToList();
    }

    public async Task<IdentityResult> UpdateUserPermissionsAsync(string userId, List<string> permissions)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return IdentityResult.Failed(new IdentityError
            {
                Description = "El ID de usuario no puede estar vacío"
            });
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Description = "Usuario no encontrado"
            });
        }

        // Eliminar todos los permisos actuales
        var currentClaims = await _userManager.GetClaimsAsync(user);
        var currentPermissions = currentClaims
            .Where(c => c.Type == "Permission")
            .ToList();

        foreach (var claim in currentPermissions)
        {
            var result = await _userManager.RemoveClaimAsync(user, claim);
            if (!result.Succeeded) return result;
        }

        // Agregar los nuevos permisos seleccionados (si hay alguno)
        if (permissions != null)
        {
            foreach (var permission in permissions.Distinct())
            {
                var result = await _userManager.AddClaimAsync(
                    user,
                    new Claim("Permission", permission));

                if (!result.Succeeded) return result;
            }
        }

        return IdentityResult.Success;
    }

    public async Task<List<IdentityUser>> GetAllUsersAsync()
    {
        return await _userManager.Users.ToListAsync();
    }

    public Task<List<string>> GetAllAvailablePermissionsAsync()
    {
        return Task.FromResult(Permissions.GetAllPermissions());
    }

    public async Task<Dictionary<string, List<PermissionViewModel>>> GetCategorizedPermissionsAsync(string userId)
    {
        var userPermissions = await GetUserPermissionsAsync(userId);
        var allPermissions = Permissions.GetPermissionsByCategory();

        var result = new Dictionary<string, List<PermissionViewModel>>();

        foreach (var category in allPermissions)
        {
            if (category.Value == null || !category.Value.Any())
                continue;

            var permissionViewModels = category.Value
                .Select(p => new PermissionViewModel
                {
                    Value = p,
                    DisplayName = GetPermissionDisplayName(p),
                    IsSelected = userPermissions.Contains(p)
                })
                .ToList();

            result.Add(category.Key, permissionViewModels);
        }

        // Depuración: verificar permisos de CxC
        if (allPermissions.ContainsKey("CuentaPorCobrar"))
        {
            Console.WriteLine($"Permisos CxC encontrados: {string.Join(", ", allPermissions["CuentaPorCobrar"])}");
        }

        return result;
    }

    private string GetPermissionDisplayName(string permission)
    {
        var parts = permission.Split('.');
        if (parts.Length < 2) return permission;

        var action = parts.Last();
        var resource = parts[parts.Length - 2];

        return $"{GetActionName(action)} {GetResourceName(resource)}";
    }

    private string GetActionName(string action)
    {
        return action switch
        {
            "View" => "Ver",
            "Create" => "Crear",
            "Edit" => "Editar",
            "Delete" => "Eliminar",
            _ => action
        };
    }

    private string GetResourceName(string resource)
    {
        return resource switch
        {
            "Inquilinos" => "Inquilinos",
            "Propietarios" => "Propietarios",
            "Inmuebles" => "Inmuebles",
            "CxC" => "Cuentas por Cobrar",
            "Cobros" => "Cobros",
            "Reportes" => "Reportes",
            _ => resource
        };
    }
}