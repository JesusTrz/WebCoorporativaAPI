using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public class UsuarioController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPerfilService _perfilService;
        private readonly Cloudinary _cloudinary;

        public UsuarioController(UserManager<ApplicationUser> userManager, IPerfilService perfilService, Cloudinary cloudinary)
        {
            _userManager = userManager;
            _perfilService = perfilService;
            _cloudinary = cloudinary;
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
        // PATCH - ACTUALIZAR IMAGEN PROPIA
        // =========================
        [HttpPatch("mi-perfil/imagen")]
        public async Task<IActionResult> ActualizarMiImagen([FromBody] ActualizarImagenDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (string.IsNullOrEmpty(dto.Imagen))
                return BadRequest("No se recibió imagen.");

            try
            {
                if (!dto.Imagen.StartsWith("data:image"))
                    return BadRequest("Formato de imagen inválido.");

                var partes = dto.Imagen.Split(',');
                if (partes.Length != 2)
                    return BadRequest("Imagen inválida.");

                byte[] bytes;
                try { bytes = Convert.FromBase64String(partes[1].Trim()); }
                catch { return BadRequest("Error al decodificar imagen."); }

                if (bytes.Length > 2 * 1024 * 1024)
                    return BadRequest("La imagen no debe superar 2MB.");

                user.Imagen = await GuardarImagen(bytes);

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                    return BadRequest(result.Errors);

                return Ok(new { imagen = user.Imagen });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
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
                    // VALIDAR FORMATO
                    if (!dto.Imagen.StartsWith("data:image"))
                        return BadRequest("Formato de imagen inválido");

                    // VALIDAR LONGITUD BASE64
                    if (dto.Imagen.Length > 3_000_000)
                        return BadRequest("Imagen demasiado grande");

                    // SEPARAR BASE64
                    var partes = dto.Imagen.Split(',');
                    if (partes.Length != 2)
                        return BadRequest("Imagen inválida");

                    var base64 = partes[1].Trim();

                    byte[] bytes;
                    try
                    {
                        bytes = Convert.FromBase64String(base64);
                    }
                    catch
                    {
                        return BadRequest("Error al decodificar imagen");
                    }

                    // VALIDAR PESO REAL
                    if (bytes.Length > 2 * 1024 * 1024)
                        return BadRequest("La imagen no debe superar 2MB");

                    // GUARDAR
                    var imageUrl = await GuardarImagen(bytes);
                    usuario.Imagen = imageUrl;
                }
                catch (Exception ex)
                {
                    return BadRequest($"Error imagen: {ex.Message}");
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

        [HttpGet("mi-perfil")]
        public async Task<IActionResult> GetMiPerfil()
        {
            // Identity recupera automáticamente al usuario basado en el token/cookie de la petición
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized(new { exito = false, mensaje = "No hay una sesión activa." });
            }

            // Buscamos el detalle del perfil con el servicio que ya tienes inyectado
            var perfil = await _perfilService.GetById(user.IdPerfil);

            // Retornamos un objeto ligero solo con lo necesario para la barra de navegación
            var vistaRapida = new
            {
                id = user.Id,
                userName = user.UserName,
                nombrePerfil = perfil?.strNombrePerfil ?? "Sin perfil asignado",
                imagen = user.Imagen,
                activo = user.Activo
            };

            return Ok(new { exito = true, data = vistaRapida });
        }

        // =========================
        // GUARDAR IMAGEN
        // =========================
        private async Task<string> GuardarImagen(byte[] bytes)
        {
            using var stream = new MemoryStream(bytes);

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription("avatar", stream),
                Folder = "usuarios",
                Transformation = new Transformation()
                    .Width(200).Height(200).Crop("fill")
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
                throw new Exception($"Cloudinary error: {result.Error.Message}");

            return result.SecureUrl.ToString();
        }
    }
}