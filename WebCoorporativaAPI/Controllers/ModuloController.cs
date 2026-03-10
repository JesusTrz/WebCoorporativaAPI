using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebCoorporativaAPI.Infraestructure;
using WebCoorporativaAPI.Models;

namespace WebCoorporativaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ModuloController : ControllerBase
    {
        private readonly IModuloService _moduloService;

        public ModuloController(IModuloService moduloService)
        {
            _moduloService = moduloService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var modulos = await _moduloService.GetAll();
            return Ok(modulos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var modulo = await _moduloService.GetById(id);
            if (modulo == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(modulo);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(ModuloModel modulo)
        {
            var result = await _moduloService.Create(modulo);
            if (result == null)
            {
                return BadRequest();
            }
            else
            {
                return Ok(result);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update(int id, ModuloModel modulo)
        {
            var result = await _moduloService.Update(id, modulo);
            if (!result)
            {
                return NotFound();
            }
            else
            {
                return Ok(result);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _moduloService.Delete(id);
            if (!result)
            {
                return NotFound();
            }
            else
            {
                return Ok(result);
            }
        }
    }
}
