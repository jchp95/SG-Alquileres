using Microsoft.AspNetCore.Identity;

namespace Alquileres.Models
{
    internal class ManageUsersViewModel
    {
        public IdentityUser[] Administrators { get; set; }
        public IdentityUser[] Everyone { get; set; }
    }
}