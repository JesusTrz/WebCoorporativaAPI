using System.ComponentModel.DataAnnotations;

namespace WebCoorporativaAPI.Models
{
    public class ModuloModel
    {
        [Key]
        public int IdModulo { get; set; }
        [Required(ErrorMessage = "El nombre del módulo es obligatorio")]
        [MaxLength(100)]
        public string strNombreModulo { get; set; }
        [Required]
        [MaxLength(50)]
        public string Ruta { get; set; }
        [Required]
        [MaxLength(50)]
        public string Clave { get; set; }
        public ICollection<PermisosPerfilModel> PermisosPerfilModels { get; set; }

    }
}
