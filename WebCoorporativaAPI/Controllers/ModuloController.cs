using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.X509Certificates;
using WebCoorporativaAPI.Constant;
using WebCoorporativaAPI.Data;
using WebCoorporativaAPI.DTOs;
using WebCoorporativaAPI.Helpers;
using WebCoorporativaAPI.Infraestructure;
using WebCoorporativaAPI.Models;

namespace WebCoorporativaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [AllowAnonymous]
    public class ModuloController : ControllerBase
    {
        private readonly IModuloService _moduloService;
        private readonly AppDBContext _context;

        public ModuloController(IModuloService moduloService, AppDBContext context)
        {
            _moduloService = moduloService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            //if (!User.TienePermiso($"{Modulos.Modulo}.{Acciones.Consultar}"))
            //    return Forbid();

            var modulos = await _moduloService.GetAll();
            return Ok(modulos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            //if (!User.TienePermiso($"{Modulos.Modulo}.{Acciones.Consultar}"))
            //    return Forbid();

            var modulo = await _moduloService.GetById(id);
            if (modulo == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(modulo);
            }
        }
        [HttpPost]
        public async Task<IActionResult> Post(ModuloDTO dto)
        {
            //if (!User.TienePermiso($"{Modulos.Modulo}.{Acciones.Agregar}"))
            //    return Forbid();

            var modulo = new ModuloModel
            {
                strNombreModulo = dto.strNombreModulo,
                Ruta = dto.Ruta
            };

            _context.Modulos.Add(modulo);
            await _context.SaveChangesAsync();

            return Ok(modulo);
        }

        //[HttpPost]
        //public async Task<IActionResult> Create(ModuloModel modulo)
        //{
        //    var result = await _moduloService.Create(modulo);
        //    if (result == null)
        //    {
        //        return BadRequest();
        //    }
        //    else
        //    {
        //        return Ok(result);
        //    }
        //}

        [HttpPut]
        public async Task<IActionResult> Update(int id, ModuloModel modulo)
        {
            //if (!User.TienePermiso($"{Modulos.Modulo}.{Acciones.Editar}"))
            //    return Forbid();

            var result = await _moduloService.Update(id, modulo);
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
            //if (!User.TienePermiso($"{Modulos.Modulo}.{Acciones.Eliminar}"))
            //    return Forbid();

            var result = await _moduloService.Delete(id);
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
