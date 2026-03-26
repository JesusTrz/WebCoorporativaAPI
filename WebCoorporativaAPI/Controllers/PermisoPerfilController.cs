using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebCoorporativaAPI.Constant;
using WebCoorporativaAPI.DTOs;
using WebCoorporativaAPI.Helpers;
using WebCoorporativaAPI.Infraestructure;
using WebCoorporativaAPI.Models;

namespace WebCoorporativaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    //[AllowAnonymous]
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

        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetById(int id)
        //{
        //    var permisoPerfil = await _permisosPerfilService.GetById(id);
        //    if (permisoPerfil == null)
        //    {
        //        return NotFound();
        //    }
        //    else
        //    {
        //        return Ok(permisoPerfil);
        //    }
        //}

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPermisosByPerfil(int id)
        {
            //if (!User.TienePermiso("3.consultar"))
            //    return Forbid();

            var permisosById = await _permisosPerfilService.GetPermisosByPerfil(id);
            if (permisosById == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(permisosById);
            }
        }

        [HttpPost("guardar-permisos")]
        public async Task<IActionResult> GuardarPermisos([FromBody] PermisosPerfilDTO dto)
        {
            if (!User.TienePermiso("permisosperfil.editar")) return Forbid();

            if (dto == null || dto.Modulos == null || !dto.Modulos.Any())
                return BadRequest("La solicitud está vacía o no contiene módulos.");

            var result = await _permisosPerfilService.GuardarPermisos(dto);

            if (!result) return BadRequest("Error al guardar los permisos. Verifica que el perfil exista.");

            return Ok(new { message = "Permisos Actualizados Correctamente" });
        }

        [HttpPost]
        public async Task<IActionResult> Create(PermisosPerfilModel permisoPerfil)
        {
            if (!User.TienePermiso("permisosperfil.agregar")) return Forbid();

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

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, PermisosPerfilModel permisoPerfil)
        {
            if (!User.TienePermiso("permisosperfil.editar")) return Forbid();

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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!User.TienePermiso("permisosperfil.eliminar")) return Forbid();

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

        [HttpDelete("perfil/{perfilId}")]
        public async Task<IActionResult> DeleteByPerfil(int perfilId)
        {
            if (!User.TienePermiso("permisosperfil.eliminar")) return Forbid();

            var permisos = await _permisosPerfilService.GetPermisosByPerfil(perfilId);

            foreach (var p in permisos)
            {
                await _permisosPerfilService.Delete(p.IdPperfil);
            }

            return Ok();
        }
    }
}
