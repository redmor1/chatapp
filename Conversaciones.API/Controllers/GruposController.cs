using Conversaciones.API.Data;
using Conversaciones.API.DTOs;
using Conversaciones.API.Entidades;
using Conversaciones.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Conversaciones.API.Controllers
{
    [ApiController]
    [Route("api/v1/grupo")]
    [Authorize]
    public class GruposController : ControllerBase
    {

        private readonly ILogger<GruposController> _logger;
        private readonly IGrupoService _grupoService;

        public GruposController(ILogger<GruposController> logger, IGrupoService grupoService)
        {
            _logger = logger;
            _grupoService = grupoService;
        }

        // Listar los grupos del usuario actual.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GrupoDetalleResponse>>> GetGrupos()
        {

            // Obtener ID del usuario actual desde el token
            var usuarioActualId = User.FindFirstValue(ClaimTypes.NameIdentifier);


            _logger.LogInformation("Usuario {Id} esta pidiendo sus grupos", usuarioActualId);

            if (string.IsNullOrEmpty(usuarioActualId))
            {
                return Unauthorized();
            }

            var gruposDTOs = await _grupoService.GetGruposParaUsuarioAsync(usuarioActualId);
            return Ok(gruposDTOs);
        }



        [HttpPost]
        public async Task<ActionResult<GrupoDetalleResponse>> CrearGrupo([FromBody] CrearGrupoRequest request)
        {
            // Obtener ID del usuario actual desde el token
            var usuarioActualId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Usuario {Id} esta creando un nuevo grupo", usuarioActualId);
            if (string.IsNullOrEmpty(usuarioActualId))
            {
                return Unauthorized();
            }
            var nuevoGrupo = await _grupoService.CrearGrupoAsync(request, usuarioActualId);
            // Devuelve "201 Created"
            return CreatedAtAction(nameof(GetGrupos), new { }, nuevoGrupo);
        }
    }
}