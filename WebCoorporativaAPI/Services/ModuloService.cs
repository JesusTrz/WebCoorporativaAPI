using WebCoorporativaAPI.Data;
using WebCoorporativaAPI.Infraestructure;

namespace WebCoorporativaAPI.Services
{
    public class ModuloService : IModuloService
    {
        private readonly AppDBContext _context;

        public ModuloService(AppDBContext context)
        {
            _context = context;
        }
    }
}
