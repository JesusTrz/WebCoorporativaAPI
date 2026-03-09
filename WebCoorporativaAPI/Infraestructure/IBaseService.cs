namespace WebCoorporativaAPI.Infraestructure
{
    public interface IBaseService<T> where T : class
    {
        Task<T> Create(T entity);
        Task<bool> Delete(int id);
        Task<List<T>> GetAll();
        Task<T?> GetById(int id);
        Task<bool> Update(int id, T entity);
    }
}
