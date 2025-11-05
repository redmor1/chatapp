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

            // TODO: Validar que tipo sea "grupo" o "directo"
            // TODO: Validar que limit <= 100
            // TODO: Llamar al servicio
            // TODO: Manejar casos de error (403, 404)

            throw new NotImplementedException();
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

            // TODO: Validar que tipo sea "grupo" o "directo"
            // TODO: Llamar al servicio
            // TODO: Retornar 201 Created
            // TODO: Manejar casos de error (400, 403, 404)

            throw new NotImplementedException();
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

            // TODO: Llamar al servicio
            // TODO: Manejar casos de error (401, 403)

            throw new NotImplementedException();
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

            // TODO: Llamar al servicio
            // TODO: Retornar 200 OK
            // TODO: Manejar casos de error (401, 404)

            throw new NotImplementedException();
        }
    }
}
