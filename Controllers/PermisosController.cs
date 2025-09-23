using Microsoft.AspNetCore.Mvc;
using Alquileres.Models;
using System.Collections.Generic;

namespace Alquileres.Controllers
{
    public class PermisosController : Controller
    {
        [HttpGet]
        public IActionResult GetAllPermissions()
        {
            var categories = Permissions.GetPermissionsByCategory();
            var result = new Dictionary<string, List<object>>();
            foreach (var cat in categories)
            {
                var perms = new List<object>();
                foreach (var perm in cat.Value)
                {
                    perms.Add(new
                    {
                        value = perm,
                        displayName = perm, // O puedes usar un método para obtener un nombre legible
                        isSelected = false
                    });
                }
                result[cat.Key] = perms;
            }
            return Json(new { permissionCategories = result });
        }
    }
}
