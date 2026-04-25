using Microsoft.AspNetCore.Identity;

namespace Parcial.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Navegación hacia el perfil de cliente (si aplica)
        public Cliente? Cliente { get; set; }
    }
}
