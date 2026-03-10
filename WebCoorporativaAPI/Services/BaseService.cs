using Microsoft.EntityFrameworkCore;
using WebCoorporativaAPI.Data;
using WebCoorporativaAPI.Infraestructure;

namespace WebCoorporativaAPI.Services
{
    /*<T> es el modelo*/
    public class BaseService<T> : IBaseService<T> where T : class
    {
        /* Esta clase base va a heredar a los demás modelos, pues ya contiene los metodos del crud, así no es necesario tener que estar escribiendo el CRUD completo en cada servicio*/
        private readonly AppDBContext _context;
        private readonly DbSet<T> _dbSet;
        /* Ya contiene la "Conexion" con base de datos, asi que ya no es necesario referenciarla desde los servicios*/
        public BaseService(AppDBContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<List<T>> GetAll()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T?> GetById(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<T> Create(T entity)
        {
            _dbSet.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> Update(int id, T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Delete(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null)
            {
                return false;
            }
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
