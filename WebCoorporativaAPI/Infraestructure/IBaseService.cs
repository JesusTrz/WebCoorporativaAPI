namespace WebCoorporativaAPI.Infraestructure
{
    public interface IBaseService<T> where T : class
    {
        /*Mismo caso que con BaseService, Ya no es necesario volver a escribir los metodos
         dentro de las interfaces de los Servicios*/
        Task<T> Create(T entity);
        Task<bool> Delete(int id);
        Task<List<T>> GetAll();
        Task<T?> GetById(int id);
        Task<bool> Update(int id, T entity);
    }
}
