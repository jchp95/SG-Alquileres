using Microsoft.AspNetCore.Identity;

namespace Alquileres.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(IdentityUser user, IList<string> roles, IList<string> permissions);
    }
}
