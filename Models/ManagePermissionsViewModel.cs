using System.Collections.Generic;

namespace Alquileres.Models
{
    public class ManagePermissionsViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public Dictionary<string, List<PermissionViewModel>> PermissionCategories { get; set; }
            = new Dictionary<string, List<PermissionViewModel>>();
        public List<string> SelectedPermissions { get; set; } = new List<string>();
    }

    public class PermissionViewModel
    {
        public string Value { get; set; }
        public string DisplayName { get; set; }
        public bool IsSelected { get; set; }
    }
}