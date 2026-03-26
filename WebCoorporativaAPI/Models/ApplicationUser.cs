using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebCoorporativaAPI.Models
{
    public class ApplicationUser : IdentityUser
    {

        public int IdPerfil { get; set; }

        [DefaultValue(true)]
        public bool Activo { get; set; } = true;

        public string? Imagen { get; set; }
        [ForeignKey("IdPerfil")]
        public PerfilModel Perfil { get; set; }
    }
}
