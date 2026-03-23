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
        public bool BitAgregar { get; set; }

        public bool BitEditar { get; set; }

        public bool BitConsulta { get; set; }

        public bool BitEliminar { get; set; }

        public bool BitDetalle { get; set; }
        [ForeignKey("IdModulo")]
        public Models.ModuloModel Modulo { get; set; }
        [ForeignKey("IdPerfil")]
        public Models.PerfilModel Perfil { get; set; }

    }
}
