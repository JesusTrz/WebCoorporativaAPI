using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebCoorporativaAPI.Models
{
    public class PerfilModel
    {
        [Key]
        public int IdPerfil { get; set; }
        [Required(ErrorMessage = "El nombre del perfil es obligatorio")]
        [MaxLength(80)]
        public string strNombrePerfil { get; set; }
        [DefaultValue(false)]
        public bool BitAdministrador { get; set; } = false;

        public ICollection<PermisosPerfilModel> PermisosPerfilModels { get; set; } = new List<PermisosPerfilModel>();
    }
}
