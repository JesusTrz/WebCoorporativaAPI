using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebCoorporativaAPI.DTOs;
using WebCoorporativaAPI.Infraestructure;
using WebCoorporativaAPI.Models;

namespace WebCoorporativaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPerfilService _perfilService;

        public UsuarioController(UserManager<ApplicationUser> userManager, IPerfilService perfilService)
        {
            _userManager = userManager;
            _perfilService = perfilService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var usuarios = _userManager.Users
                .Include(u => u.Perfil)
                .Select(u => new UsuarioDto
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    IdPerfil = u.IdPerfil,
                    NombrePerfil = u.Perfil.strNombrePerfil,
                    Activo = u.Activo,
                    Imagen = u.Imagen
                }).ToList();

            return Ok(usuarios);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var perfil = await _perfilService.GetById(user.IdPerfil);

            return Ok(new UsuarioDto
            {
                Id = user.Id,
                UserName = user.UserName,
                IdPerfil = user.IdPerfil,
                NombrePerfil = perfil?.strNombrePerfil,
                Activo = user.Activo,
                Imagen = user.Imagen
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] EditarUsuarioDto dto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IdPerfil = dto.IdPerfil;
            user.Activo = dto.Activo;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok();
        }
    }
}
