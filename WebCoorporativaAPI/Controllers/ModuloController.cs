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
using Microsoft.EntityFrameworkCore;

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
            if (!User.TienePermiso("modulo.agregar")) return Forbid();

            // 🔹 Validar si ya existe la misma CLAVE
            var existeClave = await _context.Modulos
                .AnyAsync(m => m.Clave == dto.Clave);

            if (existeClave)
            {
                return BadRequest("Ya existe un módulo con la misma clave.");
            }

            // 🔹 Validar si ya existe misma CLAVE + RUTA
            var existeClaveRuta = await _context.Modulos
                .AnyAsync(m => m.Clave == dto.Clave && m.Ruta == dto.Ruta);

            if (existeClaveRuta)
            {
                return BadRequest("Ya existe un módulo con la misma clave y ruta.");
            }

            var modulo = new ModuloModel
            {
                strNombreModulo = dto.strNombreModulo,
                Ruta = dto.Ruta,
                Clave = dto.Clave
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

        //[HttpPut]
        //public async Task<IActionResult> Update(int id, ModuloModel modulo)
        //{
        //    //if (!User.TienePermiso($"{Modulos.Modulo}.{Acciones.Editar}"))
        //    //    return Forbid();

        //    var result = await _moduloService.Update(id, modulo);
        //    if (!result)
        //    {
        //        return NotFound();
        //    }
        //    else
        //    {
        //        return Ok(result);
        //    }
        //}

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] ModuloDTO dto)
        {
            if (!User.TienePermiso("modulo.editar")) return Forbid();

            var modulo = await _context.Modulos.FindAsync(id);
            if (modulo == null)
            {
                return NotFound("Módulo no encontrado");
            }

            // 🔹 Validar CLAVE duplicada (excluyendo el actual)
            var existeClave = await _context.Modulos
                .AnyAsync(m => m.Clave == dto.Clave && m.IdModulo != id);

            if (existeClave)
            {
                return BadRequest("Ya existe otro módulo con la misma clave.");
            }

            // 🔹 Validar CLAVE + RUTA duplicada (excluyendo el actual)
            var existeClaveRuta = await _context.Modulos
                .AnyAsync(m => m.Clave == dto.Clave && m.Ruta == dto.Ruta && m.IdModulo != id);

            if (existeClaveRuta)
            {
                return BadRequest("Ya existe otro módulo con la misma clave y ruta.");
            }

            // Actualizar
            modulo.strNombreModulo = dto.strNombreModulo;
            modulo.Ruta = dto.Ruta;
            modulo.Clave = dto.Clave;

            _context.Modulos.Update(modulo);
            await _context.SaveChangesAsync();

            return Ok(modulo);
        }

        //[HttpDelete]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    //if (!User.TienePermiso($"{Modulos.Modulo}.{Acciones.Eliminar}"))
        //    //    return Forbid();

        //    var result = await _moduloService.Delete(id);
        //    if (!result)
        //    {
        //        return NotFound();
        //    }
        //    else
        //    {
        //        return Ok(result);
        //    }
        //}

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!User.TienePermiso("modulo.eliminar")) return Forbid();

            var modulo = await _context.Modulos.FindAsync(id);
            if (modulo == null)
            {
                return NotFound("Módulo no encontrado");
            }

            _context.Modulos.Remove(modulo);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Módulo eliminado correctamente" });
        }
    }
}
