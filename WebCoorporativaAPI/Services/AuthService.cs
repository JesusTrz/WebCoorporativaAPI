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
        private readonly IPerfilService _perfilService;
        private readonly IModuloService _moduloService;

        public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration, IPermisosPerfilService permisosPerfilService, IHttpClientFactory httpClientFactory, IPerfilService perfilService, IModuloService moduloService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _permisosPerfilService = permisosPerfilService;
            _httpClientFactory = httpClientFactory;
            _perfilService = perfilService;
            _moduloService = moduloService;
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

            //var captchaValido = await ValidarCaptcha(captchaToken);

            //if (!captchaValido)
            //{
            //    return null;
            //}

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
            //var permisos = await _permisosPerfilService.GetPermisosByPerfil(user.IdPerfil);
            var perfil = await _perfilService.GetById(user.IdPerfil);

            var permisos = await _permisosPerfilService.GetPermisosByPerfil(user.IdPerfil);

            // DIAGNÓSTICO - quitar después
            Console.WriteLine($"=== PERMISOS COUNT: {permisos?.Count() ?? -1}");
            Console.WriteLine($"=== PERFIL ADMIN: {perfil?.BitAdministrador}");
            foreach (var p in permisos ?? [])
                Console.WriteLine($"  Modulo: {p.Modulo?.Clave ?? "NULL"}, Agregar:{p.BitAgregar}");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("perfilId", user.IdPerfil.ToString()),
                new Claim("esAdmin", (perfil?.BitAdministrador ?? false).ToString().ToLower())
            };



            var permisosUnicos = permisos
            .SelectMany(p => new[]
            {
                p.BitAgregar ? $"{p.Modulo.Clave}.agregar" : null,
                p.BitEditar ? $"{p.Modulo.Clave}.editar" : null,
                p.BitEliminar ? $"{p.Modulo.Clave}.eliminar" : null,
                p.BitConsulta ? $"{p.Modulo.Clave}.consultar" : null,
                p.BitDetalle ? $"{p.Modulo.Clave}.detalle" : null
            })
            .Where(p => p != null)
            .Distinct();

            if (perfil?.BitAdministrador == true)
            {
                var modulos = await _moduloService.GetAll(); // necesitas esto

                foreach (var modulo in modulos)
                {
                    claims.Add(new Claim("permiso", $"{modulo.Clave}.agregar"));
                    claims.Add(new Claim("permiso", $"{modulo.Clave}.editar"));
                    claims.Add(new Claim("permiso", $"{modulo.Clave}.eliminar"));
                    claims.Add(new Claim("permiso", $"{modulo.Clave}.consultar"));
                    claims.Add(new Claim("permiso", $"{modulo.Clave}.detalle"));
                }
            }
            else
            {
                foreach (var permiso in permisosUnicos)
                {
                    claims.Add(new Claim("permiso", permiso));
                }
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
