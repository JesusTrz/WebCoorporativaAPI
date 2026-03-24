using Microsoft.EntityFrameworkCore;
using WebCoorporativaAPI.Data;
using WebCoorporativaAPI.DTOs;
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

        //public async Task<List<PermisosPerfilModel>> GetPermisosByPerfil(int perfilId)
        //{
        //    return await _context.PermisosPerfil
        //        .Where(p => p.IdPerfil == perfilId)
        //        .Select(p => new PermisosPerfilModel
        //        {
        //            IdPperfil = p.IdPperfil,
        //            IdModulo = p.IdModulo,
        //            IdPerfil = p.IdPerfil,
        //            BitAgregar = p.BitAgregar,
        //            BitEditar = p.BitEditar,
        //            BitConsulta = p.BitConsulta,
        //            BitEliminar = p.BitEliminar,
        //            BitDetalle = p.BitDetalle
        //        })
        //        .ToListAsync();
        //}
        public async Task<List<PermisosPerfilModel>> GetPermisosByPerfil(int perfilId)
        {
            return await _context.PermisosPerfil
                .Include(p => p.Modulo)
                .Where(p => p.IdPerfil == perfilId)
                .Select(p => new PermisosPerfilModel
                {
                    IdPperfil = p.IdPperfil,
                    IdModulo = p.IdModulo,
                    IdPerfil = p.IdPerfil,
                    BitAgregar = p.BitAgregar,
                    BitEditar = p.BitEditar,
                    BitConsulta = p.BitConsulta,
                    BitEliminar = p.BitEliminar,
                    BitDetalle = p.BitDetalle,
                    Modulo = new ModuloModel
                    {
                        IdModulo = p.Modulo.IdModulo,
                        Clave = p.Modulo.Clave // ← Solo lo que necesitas
                    }
                })
                .ToListAsync();
        }

        public async Task<bool> GuardarPermisos(PermisosPerfilDTO dto)
        {
            // Agrega esto temporalmente
            Console.WriteLine($"IdPerfil: {dto.IdPerfil}");
            foreach (var m in dto.Modulos)
            {
                Console.WriteLine($"IdModulo: {m.IdModulo}");
            }

            var existentes = _context.PermisosPerfil
                .Where(x => x.IdPerfil == dto.IdPerfil);

            _context.PermisosPerfil.RemoveRange(existentes);

            var nuevos = dto.Modulos.Select(m => new PermisosPerfilModel
            {
                IdPerfil = dto.IdPerfil,
                IdModulo = m.IdModulo,
                BitAgregar = m.BitAgregar,
                BitEditar = m.BitEditar,
                BitConsulta = m.BitConsulta,
                BitEliminar = m.BitEliminar,
                BitDetalle = m.BitDetalle
            });

            await _context.PermisosPerfil.AddRangeAsync(nuevos);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
