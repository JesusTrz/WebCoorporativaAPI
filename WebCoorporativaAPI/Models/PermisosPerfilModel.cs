using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebCoorporativaAPI.Models
{
    public class PermisosPerfilModel
    {
        [Key]
        public int IdPperfil { get; set; }
        public int IdModulo { get; set; }
        public int IdPerfil { get; set; }
        [DefaultValue(false)]
        public bool BitAgregar { get; set; } = false;
        [DefaultValue(false)]
        public bool BitEditar { get; set; } = false;
        [DefaultValue(false)]
        public bool BitConsulta { get; set; } = false;
        [DefaultValue(false)]
        public bool BitEliminar { get; set; } = false;
        [DefaultValue(false)]
        public bool BitDetalle { get; set; } = false;
        [ForeignKey("IdModulo")]
        public Models.ModuloModel Modulo { get; set; }
        [ForeignKey("IdPerfil")]
        public Models.PerfilModel Perfil { get; set; }

    }
}
