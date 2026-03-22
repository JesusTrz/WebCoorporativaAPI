using Microsoft.EntityFrameworkCore;
using WebCoorporativaAPI.Data;
using WebCoorporativaAPI.DTOs;
using WebCoorporativaAPI.Infraestructure;

namespace WebCoorporativaAPI.Services
{
    public class MenuService : IMenuService
    {
        private readonly AppDBContext _context;

        public MenuService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<List<MenuDTO>> GetMenuByUser(string userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return new List<MenuDTO>();

            var menu = await (
                from permiso in _context.PermisosPerfil.AsNoTracking()
                join modulo in _context.Modulos.AsNoTracking()
                    on permiso.IdModulo equals modulo.IdModulo
                where permiso.IdPerfil == user.IdPerfil
                select new MenuDTO
                {
                    ModuloId = modulo.IdModulo,
                    Nombre = modulo.strNombreModulo,
                    Ruta = modulo.Ruta,
                    Icono = modulo.Icono,

                    Agregar = permiso.BitAgregar,
                    Editar = permiso.BitEditar,
                    Eliminar = permiso.BitEliminar,
                    Consultar = permiso.BitConsulta,
                    Detalle = permiso.BitDetalle
                }
            )
            // 🔥 FILTRADO EN BD (mejor que en memoria)
            .Where(m => m.Agregar || m.Editar || m.Eliminar || m.Consultar || m.Detalle)
            // 🔥 AGRUPACIÓN EN BD
            .GroupBy(m => m.ModuloId)
            .Select(g => g.First())
            .ToListAsync();

            return menu;
        }
    }
}