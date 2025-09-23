using Alquileres.Context;
using Alquileres.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Alquileres
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                dbContext.IsSeeding = true;

                // First create roles without auditing
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                await EnsureRolesAsync(roleManager);

                // Then create admin user without auditing
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                await EnsureTestAdminAsync(userManager, dbContext);

                // Then add permissions (this happens after user is created)
                await EnsurePermissionsAsync(userManager, services);

                // Add currencies if they don't exist
                if (!await dbContext.TbMonedas.AnyAsync())
                {
                    var monedas = new List<TbMoneda>
            {
                new TbMoneda { Fmoneda = "Peso", Fsimbolo = "RD", Factivo = true },
                new TbMoneda { Fmoneda = "Dolar", Fsimbolo = "US", Factivo = true }
            };
                    dbContext.TbMonedas.AddRange(monedas);
                    await dbContext.SaveChangesAsync();
                }
            }
            finally
            {
                dbContext.IsSeeding = false;
            }
        }

        private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            // Crear todos los roles necesarios
            var roles = new[]
            {
        AppConstants.AdministratorRole,
        AppConstants.UserRole
    };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    Console.WriteLine($"Rol creado: {role}");
                }
            }
        }

        private static async Task EnsureTestAdminAsync(UserManager<IdentityUser> userManager, ApplicationDbContext dbContext)
        {
            var testAdmin = await userManager.Users
                .Where(x => x.UserName == "admin")
                .SingleOrDefaultAsync();

            if (testAdmin == null)
            {
                testAdmin = new IdentityUser
                {
                    UserName = "admin",
                    Email = "admin@todo.local",
                    EmailConfirmed = false
                };

                var result = await userManager.CreateAsync(testAdmin, "Admin2025*");
                if (!result.Succeeded)
                {
                    throw new Exception($"Error creating admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }

                // Add role after user is created
                result = await userManager.AddToRoleAsync(testAdmin, AppConstants.AdministratorRole);
                if (!result.Succeeded)
                {
                    throw new Exception($"Error adding role to admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }

                // Now create the TbUsuario record
                var nuevoUsuario = new TbUsuario
                {
                    Fnombre = "Administrador",
                    Fusuario = "admin",
                    Femail = "admin@todo.local",
                    Fnivel = 1,
                    Fpassword = testAdmin.PasswordHash,
                    Factivado = true,
                    FkidSucursal = 1,
                    Factivo = true,
                    IdentityId = testAdmin.Id
                };

                dbContext.TbUsuarios.Add(nuevoUsuario);
                await dbContext.SaveChangesAsync();
            }
            else
            {
                // ¡IMPORTANTE!: Si el usuario ya existe, verificar y asignar el rol si no lo tiene
                var userRoles = await userManager.GetRolesAsync(testAdmin);
                if (!userRoles.Contains(AppConstants.AdministratorRole))
                {
                    var result = await userManager.AddToRoleAsync(testAdmin, AppConstants.AdministratorRole);
                    if (!result.Succeeded)
                    {
                        throw new Exception($"Error adding role to existing admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                    Console.WriteLine("✅ Rol Administrator asignado al usuario admin existente");
                }
                else
                {
                    Console.WriteLine("ℹ️ Usuario admin ya tiene el rol Administrator");
                }
            }
        }

        private static async Task EnsurePermissionsAsync(UserManager<IdentityUser> userManager, IServiceProvider services)
        {
            var adminUser = await userManager.FindByNameAsync("admin");
            if (adminUser == null) return;

            var dbContext = services.GetRequiredService<ApplicationDbContext>();
            var allPermissions = Permissions.GetAllPermissions();

            foreach (var permission in allPermissions)
            {
                var existingClaim = await dbContext.UserClaims
                    .FirstOrDefaultAsync(uc =>
                        uc.UserId == adminUser.Id &&
                        uc.ClaimType == "Permission" &&
                        uc.ClaimValue == permission);

                if (existingClaim == null)
                {
                    await userManager.AddClaimAsync(adminUser, new Claim("Permission", permission));
                }
            }
        }
    }
}
