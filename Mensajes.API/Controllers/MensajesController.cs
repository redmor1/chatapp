using Mensajes.API.DTOs;
using Mensajes.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Mensajes.API.Controllers
{
    [ApiController]
    [Route("api/v1")]
    [Authorize]
    public class MensajesController : ControllerBase
    {
        private readonly ILogger<MensajesController> _logger;
        private readonly IMensajeService _mensajeService;

        public MensajesController(ILogger<MensajesController> logger, IMensajeService mensajeService)
        {
            _logger = logger;
            _mensajeService = mensajeService;
        }


        [HttpGet("conversaciones/{conversacionId}/mensajes")]
        public async Task<ActionResult<MensajesPaginadosResponse>> GetHistorialMensajes(
            [FromRoute] string conversacionId,
            [FromQuery] string tipo,
            [FromQuery] string? before,
            [FromQuery] int limit = 50)
        {
            var usuarioActualId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(usuarioActualId))
            {
                return Unauthorized();
            }

            try
            {
                var response = await _mensajeService.GetHistorialMensajesAsync(conversacionId, tipo, usuarioActualId, before, limit);
                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo mensajes");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("conversaciones/{conversacionId}/mensajes")]
        public async Task<ActionResult<MensajeResponse>> EnviarMensaje(
            [FromRoute] string conversacionId,
            [FromQuery] string tipo,
            [FromBody] CrearMensajeRequest request)
        {
            var usuarioActualId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(usuarioActualId))
            {
                return Unauthorized();
            }

            try
            {
                var response = await _mensajeService.EnviarMensajeAsync(conversacionId, tipo, request, usuarioActualId);
                return CreatedAtAction(nameof(GetHistorialMensajes), new { conversacionId = conversacionId, tipo = tipo }, response);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando mensaje");
                return StatusCode(500, "Error interno del servidor");
            }
        }


        [HttpGet("mensajes/{mensajeId}/lecturas")]
        public async Task<ActionResult<List<AcuseLecturaResponse>>> GetAcusesDeLectura(
            [FromRoute] string mensajeId)
        {
            var usuarioActualId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(usuarioActualId))
            {
                return Unauthorized();
            }

            try
            {
                var response = await _mensajeService.GetAcusesDeLecturaAsync(mensajeId, usuarioActualId);
                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo acuses de lectura");
                return StatusCode(500, "Error interno del servidor");
            }
        }


        [HttpPatch("mensajes/{mensajeId}/lectura")]
        public async Task<ActionResult<AcuseLecturaResponse>> MarcarMensajeComoLeido(
            [FromRoute] string mensajeId,
            [FromBody] MarcarComoLeidoRequest request)
        {
            var usuarioActualId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(usuarioActualId))
            {
                return Unauthorized();
            }

            try
            {
                var response = await _mensajeService.MarcarMensajeComoLeidoAsync(mensajeId, usuarioActualId, request);
                return Ok(response);
            }
             catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marcando mensaje como leído");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
