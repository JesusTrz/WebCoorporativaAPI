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
                        Clave = p.Modulo.Clave
                    }
                })
                .ToListAsync();
        }

        public async Task<bool> GuardarPermisos(PermisosPerfilDTO dto)
        {
            // 1. VALIDACIÓN CRÍTICA: ¿Existe realmente este perfil?
            var perfilExiste = await _context.Perfiles.AnyAsync(p => p.IdPerfil == dto.IdPerfil);
            if (!perfilExiste) return false;

            // Transacción: Si algo falla al borrar o insertar, se revierte todo
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 2. Borrar permisos existentes
                var existentes = await _context.PermisosPerfil
                    .Where(x => x.IdPerfil == dto.IdPerfil)
                    .ToListAsync();

                if (existentes.Any())
                {
                    _context.PermisosPerfil.RemoveRange(existentes);
                }

                // 3. Crear los nuevos basados en el DTO
                var nuevos = dto.Modulos.Select(m => new PermisosPerfilModel
                {
                    IdPerfil = dto.IdPerfil,
                    IdModulo = m.IdModulo,
                    BitAgregar = m.BitAgregar,
                    BitEditar = m.BitEditar,
                    BitConsulta = m.BitConsulta,
                    BitEliminar = m.BitEliminar,
                    BitDetalle = m.BitDetalle
                }).ToList(); // Obligamos a materializar la lista aquí

                // 4. Insertar y guardar
                if (nuevos.Any())
                {
                    await _context.PermisosPerfil.AddRangeAsync(nuevos);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Aquí podrías agregar un Log (ej. _logger.LogError(ex.Message))
                return false;
            }
        }
    }
}