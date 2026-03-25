using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebCoorporativaAPI.DTOs;
using WebCoorporativaAPI.Helpers;
using WebCoorporativaAPI.Infraestructure;
using WebCoorporativaAPI.Models;
using WebCoorporativaAPI.Services;

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

        // =========================
        // GET ALL
        // =========================
        [HttpGet]
        public IActionResult GetAll()
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

        // =========================
        // GET BY ID
        // =========================
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

        // =========================
        // EDITAR USUARIO (🔥 CORREGIDO)
        // =========================
        [HttpPut("{id}")]
        public async Task<IActionResult> EditarUsuario(string id, [FromBody] EditarUsuarioDto dto)
        {
            if (!User.TienePermiso("usuario.editar"))
                return Forbid();

            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null)
                return NotFound();

            // ✅ actualizar datos básicos
            usuario.IdPerfil = dto.IdPerfil;
            usuario.Activo = dto.Activo;

            // =========================
            // 🔥 VALIDACIÓN DE IMAGEN BASE64
            // =========================
            if (!string.IsNullOrEmpty(dto.Imagen))
            {
                try
                {
                    // 1. Validar formato base64 imagen
                    if (!dto.Imagen.StartsWith("data:image"))
                        return BadRequest("Formato de imagen inválido");

                    var partes = dto.Imagen.Split(',');
                    if (partes.Length != 2)
                        return BadRequest("Base64 inválido");

                    var base64 = partes[1];

                    byte[] bytes;
                    try
                    {
                        bytes = Convert.FromBase64String(base64);
                    }
                    catch
                    {
                        return BadRequest("Error al decodificar la imagen");
                    }

                    // 🔥 VALIDAR TAMAÑO (2MB)
                    int maxSize = 2 * 1024 * 1024;

                    if (bytes.Length > maxSize)
                        return BadRequest("La imagen no debe superar 2MB");

                    // 🔥 (Opcional pero recomendado) eliminar imagen anterior
                    if (!string.IsNullOrEmpty(usuario.Imagen))
                    {
                        var rutaAnterior = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            usuario.Imagen.TrimStart('/')
                        );

                        if (System.IO.File.Exists(rutaAnterior))
                            System.IO.File.Delete(rutaAnterior);
                    }

                    // Guardar nueva imagen
                    usuario.Imagen = await GuardarImagen(bytes);
                }
                catch (Exception ex)
                {
                    return BadRequest($"Error procesando imagen: {ex.Message}");
                }
            }

            // =========================
            // GUARDAR
            // =========================
            var result = await _userManager.UpdateAsync(usuario);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Usuario actualizado correctamente");
        }

        // =========================
        // DELETE
        // =========================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!User.TienePermiso("usuario.eliminar"))
                return Forbid();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok();
        }

        // =========================
        // GUARDAR IMAGEN
        // =========================
        private async Task<string> GuardarImagen(byte[] bytes)
        {
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}.png";
            var filePath = Path.Combine(folder, fileName);

            await System.IO.File.WriteAllBytesAsync(filePath, bytes);

            return $"/images/{fileName}";
        }
    }
}