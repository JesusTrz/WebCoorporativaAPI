using Microsoft.AspNetCore.Identity;

namespace WebCoorporativaAPI.Models
{
    public class ApplicationUser : IdentityUser
    {

        public int IdPerfil { get; set; }

        public int IdEstadoUsuario { get; set; }

        public string? Imagen { get; set; }
        public Models.PerfilModel Perfil { get; set; }
    }
}
