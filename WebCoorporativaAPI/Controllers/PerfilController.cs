using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebCoorporativaAPI.Constant;
using WebCoorporativaAPI.Helpers;
using WebCoorporativaAPI.Infraestructure;
using WebCoorporativaAPI.Models;

namespace WebCoorporativaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [AllowAnonymous]
    public class PerfilController : ControllerBase
    {
        private readonly IPerfilService _perfilService;

        public PerfilController(IPerfilService perfilService)
        {
            _perfilService = perfilService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            //if (!User.TienePermiso("2.consultar"))
            //    return Forbid();

            var perfiles = await _perfilService.GetAll();
            return Ok(perfiles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            //if (!User.TienePermiso("2.consultar"))
            //    return Forbid();

            var perfil = await _perfilService.GetById(id);
            if (perfil == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(perfil);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(PerfilModel perfil)
        {

            //if (!User.TienePermiso("2.agregar"))
              //  return Forbid();

            var result = await _perfilService.Create(perfil);
            if (result == null)
            {
                return BadRequest();
            }
            else
            {
                return Ok(result);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, PerfilModel perfil)
        {
            if (!User.TienePermiso("2.editar"))
                return Forbid();

            var result = await _perfilService.Update(id, perfil);
            if (!result)
            {
                return NotFound();
            }
            else
            {
                return Ok(result);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!User.TienePermiso("2.eliminar"))
                return Forbid();

            var result = await _perfilService.Delete(id);
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
