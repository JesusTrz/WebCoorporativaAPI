using Microsoft.AspNetCore.Identity;
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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IPerfilService _perfilService;
        private readonly IPermisosPerfilService _permisosPerfilService;
        private readonly IModuloService _moduloService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IPerfilService perfilService,
            IPermisosPerfilService permisosPerfilService,
            IModuloService moduloService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _perfilService = perfilService;
            _permisosPerfilService = permisosPerfilService;
            _moduloService = moduloService;
        }

        // ================= LOGIN =================
        public async Task<string?> Login(string? userName, string password, string captchaToken)
        {
            var user = await _userManager.FindByNameAsync(userName);

            if (user == null || !user.Activo)
                return null;

            var validPassword = await _userManager.CheckPasswordAsync(user, password);
            if (!validPassword)
                return null;

            var captchaValido = await ValidarCaptcha(captchaToken);
            if (!captchaValido)
                return null;

            return await GenerateJwtToken(user);
        }

        // ================= REGISTER =================
        public async Task<IdentityResult> Register(RegisterDTO register)
        {
            string? imageUrl = null;

            if (!string.IsNullOrEmpty(register.Imagen))
            {
                try
                {
                    // 🔥 1. VALIDAR FORMATO
                    if (!register.Imagen.StartsWith("data:image"))
                    {
                        return IdentityResult.Failed(new IdentityError
                        {
                            Description = "Formato de imagen inválido"
                        });
                    }

                    // 🔥 2. VALIDAR TAMAÑO DEL STRING (ANTES DE DECODIFICAR)
                    int maxBase64Length = 3_000_000; // ~3MB
                    if (register.Imagen.Length > maxBase64Length)
                    {
                        return IdentityResult.Failed(new IdentityError
                        {
                            Description = "La imagen es demasiado grande (máx 2MB)"
                        });
                    }

                    // 🔥 3. SEPARAR BASE64
                    var partes = register.Imagen.Split(',');
                    if (partes.Length != 2)
                    {
                        return IdentityResult.Failed(new IdentityError
                        {
                            Description = "Imagen base64 inválida"
                        });
                    }

                    var base64 = partes[1].Trim();

                    // 🔥 4. CONVERTIR A BYTES
                    byte[] bytes;
                    try
                    {
                        bytes = Convert.FromBase64String(base64);
                    }
                    catch
                    {
                        return IdentityResult.Failed(new IdentityError
                        {
                            Description = "Error al decodificar la imagen"
                        });
                    }

                    // 🔥 5. VALIDAR PESO REAL
                    int maxBytes = 2 * 1024 * 1024; // 2MB
                    if (bytes.Length > maxBytes)
                    {
                        return IdentityResult.Failed(new IdentityError
                        {
                            Description = "La imagen no debe superar los 2MB"
                        });
                    }

                    // 🔥 6. GUARDAR IMAGEN
                    imageUrl = await GuardarImagen(bytes);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("🔥 ERROR PROCESANDO IMAGEN:");
                    Console.WriteLine(ex.ToString());

                    return IdentityResult.Failed(new IdentityError
                    {
                        Description = $"Error procesando imagen: {ex.Message}"
                    });
                }
            }

            // 🔥 CREAR USUARIO
            var user = new ApplicationUser
            {
                UserName = register.UserName,
                IdPerfil = register.IdPerfil,
                Activo = register.Activo,
                Imagen = imageUrl
            };

            return await _userManager.CreateAsync(user, register.Password);
        }

        // ================= JWT =================
        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
                         ?? "MindCorp@WebCoorporativa#2026$SecretKey!JWT@Secure123456789";

            var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "WebCorporativaAPI";
            var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "WebCorporativaAPI";

            var perfil = await _perfilService.GetById(user.IdPerfil);
            var permisos = await _permisosPerfilService.GetPermisosByPerfil(user.IdPerfil);

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
                var modulos = await _moduloService.GetAll();

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

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ================= CAPTCHA =================
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

        // ================= GUARDAR IMAGEN =================
        private async Task<string> GuardarImagen(byte[] bytes)
        {
            try
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var fileName = $"{Guid.NewGuid()}.png";
                var filePath = Path.Combine(folder, fileName);

                Console.WriteLine($"Guardando imagen: {filePath}");
                Console.WriteLine($"Peso: {bytes.Length} bytes");

                await File.WriteAllBytesAsync(filePath, bytes);

                return $"/images/{fileName}";
            }
            catch (Exception ex)
            {
                Console.WriteLine("🔥 ERROR GUARDANDO IMAGEN:");
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}