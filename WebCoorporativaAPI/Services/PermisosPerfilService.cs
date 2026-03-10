using WebCoorporativaAPI.Data;
using WebCoorporativaAPI.Infraestructure;
using WebCoorporativaAPI.Models;

namespace WebCoorporativaAPI.Services
{
    public class PermisosPerfilService : BaseService<PermisosPerfilModel>, IPermisosPerfilService
    {
        public PermisosPerfilService(AppDBContext context) : base(context)
        {
        }
    }
}
