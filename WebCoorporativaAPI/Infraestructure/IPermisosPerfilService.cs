using WebCoorporativaAPI.DTOs;
using WebCoorporativaAPI.Models;

namespace WebCoorporativaAPI.Infraestructure
{
    public interface IPermisosPerfilService : IBaseService<PermisosPerfilModel>
    {
        Task<List<PermisosPerfilModel>> GetPermisosByPerfil(int perfilId);
        Task<bool> GuardarPermisos(PermisosPerfilDTO dto);
    }
}
