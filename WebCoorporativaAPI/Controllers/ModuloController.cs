using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebCoorporativaAPI.Infraestructure;

namespace WebCoorporativaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModuloController : ControllerBase
    {
        private readonly IModuloService _moduloService;

        public ModuloController(IModuloService moduloService)
        {
            _moduloService = moduloService;
        }
    }
}
