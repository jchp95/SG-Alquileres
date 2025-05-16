using Alquileres.Context;
using Alquileres.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Alquileres
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            await EnsureRolesAsync(roleManager);

            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            await EnsureTestAdminAsync(userManager, services);
        }

        private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync(AppConstants.AdministratorRole))
            {
                await roleManager.CreateAsync(new IdentityRole(AppConstants.AdministratorRole));
            }
        }

        private static async Task EnsureTestAdminAsync(UserManager<IdentityUser> userManager, IServiceProvider services)
        {
            var testAdmin = await userManager.Users
                .Where(x => x.UserName == "admin@todo.local")
                .SingleOrDefaultAsync();

            if (testAdmin == null)
            {
                testAdmin = new IdentityUser
                {
                    UserName = "admin@todo.local",
                    Email = "admin@todo.local"
                };

                var result = await userManager.CreateAsync(testAdmin, "Admin2025*");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(testAdmin, AppConstants.AdministratorRole);
                }
                else
                {
                    throw new Exception("Error creando el usuario administrador: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }

            var dbContext = services.GetRequiredService<ApplicationDbContext>();

            bool exists = await dbContext.TbUsuarios.AnyAsync(u => u.IdentityId == testAdmin.Id);

            if (!exists)
            {
                var nuevoUsuario = new TbUsuario
                {

                    Fnombre = "Administrador",
                    Fusuario = "admin",
                    Fnivel = 1,
                    Fpassword = testAdmin.PasswordHash, // Puedes manejar la contraseña en otro flujo si es necesario
                    Factivado = true,
                    FkidUsuario = 1,
                    FkidSucursal = 1,
                    FestadoSync = "A",
                    Factivo = true,
                    IdentityId = testAdmin.Id
                };

                dbContext.TbUsuarios.Add(nuevoUsuario);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
