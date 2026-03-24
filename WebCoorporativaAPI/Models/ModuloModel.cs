using System.ComponentModel.DataAnnotations;

namespace WebCoorporativaAPI.Models
{
    public class ModuloModel
    {
        [Key]
        public int IdModulo { get; set; }
        public string strNombreModulo { get; set; }
        public string Ruta { get; set; }
        public string Clave { get; set; }
        public ICollection<PermisosPerfilModel> PermisosPerfilModels { get; set; }

    }
}
