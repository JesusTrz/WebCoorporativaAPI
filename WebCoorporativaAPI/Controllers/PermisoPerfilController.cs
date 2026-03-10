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
    public class PermisoPerfilController : ControllerBase
    {
        private readonly IPermisosPerfilService _permisosPerfilService;
        public PermisoPerfilController(IPermisosPerfilService permisosPerfilService)
        {
            _permisosPerfilService = permisosPerfilService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var permisosPerfil = await _permisosPerfilService.GetAll();
            return Ok(permisosPerfil);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var permisoPerfil = await _permisosPerfilService.GetById(id);
            if (permisoPerfil == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(permisoPerfil);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(PermisosPerfilModel permisoPerfil)
        {
            var result = await _permisosPerfilService.Create(permisoPerfil);
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
        public async Task<IActionResult> Update(int id, PermisosPerfilModel permisoPerfil)
        {
            var result = await _permisosPerfilService.Update(id, permisoPerfil);
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
            var result = await _permisosPerfilService.Delete(id);
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
