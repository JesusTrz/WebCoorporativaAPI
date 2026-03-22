using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebCoorporativaAPI.Models
{
    public class ApplicationUser : IdentityUser
    {

        public int IdPerfil { get; set; }

        public bool Activo { get; set; }

        public string? Imagen { get; set; }
        [ForeignKey("IdPerfil")]
        public PerfilModel Perfil { get; set; }
    }
}
