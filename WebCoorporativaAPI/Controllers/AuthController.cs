using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebCoorporativaAPI.Constant;
using WebCoorporativaAPI.DTOs;
using WebCoorporativaAPI.Helpers;
using WebCoorporativaAPI.Services;

namespace WebCoorporativaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO register)
        {
            if (!User.TienePermiso("usuario.agregar")) return Forbid();

            var result = await _authService.Register(register);

            if (result.Succeeded)
            {
                return Ok("Usuario Creado");
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO login)
        {
            var token = await _authService.Login(login.UserName, login.Password, login.CaptchaToken);

            if (token == null)
            {
                return Unauthorized();
            }
            else
            {
                return Ok(new { token });
            }
        }
    }
}
