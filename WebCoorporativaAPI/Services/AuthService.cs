using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using WebCoorporativaAPI.DTOs;
using WebCoorporativaAPI.Infraestructure;
using WebCoorporativaAPI.Models;

namespace WebCoorporativaAPI.Services
{
    public class AuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IPermisosPerfilService _permisosPerfilService;
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration, IPermisosPerfilService permisosPerfilService, IHttpClientFactory httpClientFactory)
        {
            _userManager = userManager;
            _configuration = configuration;
            _permisosPerfilService = permisosPerfilService;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string?> Login(string? userName, string password, string captchaToken)
        {
            var user = await _userManager.FindByNameAsync(userName);

            if (user == null || !user.Activo)
            {
                return null; // Usuario no encontrado, Devolver excepcion o mensaje de error
            }

            var validPassword = await _userManager.CheckPasswordAsync(user, password);

            if (!validPassword)
            {
                return null;
            }

            var captchaValido = await ValidarCaptcha(captchaToken);

            if (!captchaValido)
            {
                return null;
            }

            return await GenerateJwtToken(user);
        }

        public async Task<IdentityResult> Register(RegisterDTO register)
        {
            var user = new ApplicationUser
            {
                UserName = register.UserName,
                IdPerfil = register.IdPerfil,
                Activo = register.Activo
            };

            return await _userManager.CreateAsync(user, register.Password);
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");

            var permisos = await _permisosPerfilService.GetPermisosByPerfil(user.IdPerfil);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("perfilId", user.IdPerfil.ToString())
            };

            var permisosUnicos = permisos
            .SelectMany(p => new[]
            {
                p.BitAgregar ? $"{p.IdModulo}.agregar" : null,
                p.BitEditar ? $"{p.IdModulo}.editar" : null,
                p.BitEliminar ? $"{p.IdModulo}.eliminar" : null,
                p.BitConsulta ? $"{p.IdModulo}.consultar" : null,
                p.BitDetalle ? $"{p.IdModulo}.detalle" : null
            })
            .Where(p => p != null)
            .Distinct();

            foreach (var permiso in permisosUnicos)
            {
                claims.Add(new Claim("permiso", permiso));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<bool> ValidarCaptcha(string token)
        {
            var secretKey = _configuration["Turnstile:SecretKey"];

            var client = _httpClientFactory.CreateClient();

            var response = await client.PostAsync(
                "https://challenges.cloudflare.com/turnstile/v0/siteverify",
                new FormUrlEncodedContent(new[]
                {
            new KeyValuePair<string, string>("secret", secretKey),
            new KeyValuePair<string, string>("response", token)
                })
            );

            var json = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(json);
            return document.RootElement.GetProperty("success").GetBoolean();
        }
    }
}
