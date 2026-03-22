using System.ComponentModel.DataAnnotations;

namespace WebCoorporativaAPI.Models
{
    public class PerfilModel
    {
        [Key]
        public int IdPerfil { get; set; }
        public string strNombrePerfil { get; set; }
        public bool BitAdministrador { get; set; }

        public ICollection<PermisosPerfilModel> PermisosPerfilModels { get; set; } = new List<PermisosPerfilModel>();
    }
}
