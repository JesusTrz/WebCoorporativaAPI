using WebCoorporativaAPI.Data;
using WebCoorporativaAPI.Infraestructure;
using WebCoorporativaAPI.Models;

namespace WebCoorporativaAPI.Services
{
    public class ModuloService : BaseService<ModuloModel>, IModuloService
    {
        private readonly AppDBContext _context;

        public ModuloService(AppDBContext context) : base(context)
        {

        }

    }
}
