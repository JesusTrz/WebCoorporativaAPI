using Microsoft.EntityFrameworkCore;
using WebCoorporativaAPI.Data;
using WebCoorporativaAPI.Infraestructure;
using WebCoorporativaAPI.Models;

namespace WebCoorporativaAPI.Services
{
    public class PermisosPerfilService : BaseService<PermisosPerfilModel>, IPermisosPerfilService
    {
        private readonly AppDBContext _context;
        public PermisosPerfilService(AppDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<PermisosPerfilModel>> GetPermisosByPerfil(int perfilId)
        {
            return await _context.PermisosPerfil
                .Include(p => p.Modulo)
                .Where(p => p.IdPerfil == perfilId)
                .ToListAsync();
        }
    }
}
