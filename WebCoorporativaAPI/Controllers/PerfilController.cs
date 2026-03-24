using Microsoft.AspNetCore.Authentication.JwtBearer;
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
    //[Authorize]
    //[AllowAnonymous]
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

        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public async Task<IActionResult> Create(PerfilModel perfil)
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            Console.WriteLine($"=== BACKEND AUTH HEADER: '{authHeader?.Substring(0, Math.Min(50, authHeader?.Length ?? 0))}'");
            Console.WriteLine($"=== USER AUTHENTICATED: {User.Identity?.IsAuthenticated}");
            foreach (var claim in User.Claims)
                Console.WriteLine($"  {claim.Type}: {claim.Value}");

            //if (!User.TienePermiso("perfil.agregar")) return Forbid();

            var result = await _perfilService.Create(perfil);
            return result == null ? BadRequest() : Ok(result);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, PerfilModel perfil)
        {
            if (!User.TienePermiso("perfil.editar")) return Forbid();

            var existing = await _perfilService.GetById(id);
            if (existing == null) return NotFound();

            // Solo actualiza los campos editables
            existing.strNombrePerfil = perfil.strNombrePerfil;
            existing.BitAdministrador = perfil.BitAdministrador;

            var result = await _perfilService.Update(id, existing);
            return result ? Ok() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!User.TienePermiso("perfil.eliminar")) return Forbid();

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
