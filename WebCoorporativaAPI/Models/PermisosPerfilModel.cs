using System.ComponentModel.DataAnnotations;

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
        public Models.ModuloModel Modulo { get; set; }
        public Models.PerfilModel Perfil { get; set; }

    }
}
