using Conversaciones.API.DTOs;
using Conversaciones.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Conversaciones.API.Controllers
{
    [ApiController]
    [Route("api/v1/conversacion")]
    [Authorize]
    public class ConversacionesController : ControllerBase
    {
        private readonly ILogger<ConversacionesController> _logger;
        private readonly IConversacionService _conversacionService;

        public ConversacionesController(ILogger<ConversacionesController> logger, IConversacionService conversacionService)
        {
            _logger = logger;
            _conversacionService = conversacionService;
        }

        // Listar todas las conversaciones del usuario actual (grupos y chats directos)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConversacionResponse>>> GetConversaciones()
        {
            var usuarioActualId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Usuario {Id} está pidiendo sus conversaciones", usuarioActualId);

            if (string.IsNullOrEmpty(usuarioActualId))
            {
                return Unauthorized();
            }

            var conversaciones = await _conversacionService.GetConversacionesParaUsuarioAsync(usuarioActualId);
            return Ok(conversaciones);
        }

        // GET: api/v1/conversacion/{id}
        // Obtener detalles de una conversación específica
        [HttpGet("{id}")]
        public async Task<ActionResult<ConversacionResponse>> GetConversacionPorId(Guid id)
        {
            var usuarioActualId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(usuarioActualId))
            {
                return Unauthorized();
            }

            var conversacion = await _conversacionService.GetConversacionPorIdAsync(id, usuarioActualId);

            if (conversacion == null)
            {
                return NotFound(new ErrorResponse(null, "Conversación no encontrada", 404, "La conversación solicitada no existe o no tienes acceso a ella"));
            }

            return Ok(conversacion);
        }

        // POST: api/v1/conversacion/grupo
        // Crear un nuevo grupo
        [HttpPost("grupo")]
        public async Task<ActionResult<ConversacionResponse>> CrearGrupo([FromBody] CrearGrupoRequest request)
        {
            var usuarioActualId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Usuario {Id} está creando un nuevo grupo", usuarioActualId);

            if (string.IsNullOrEmpty(usuarioActualId))
            {
                return Unauthorized();
            }

            var nuevoGrupo = await _conversacionService.CrearGrupoAsync(request, usuarioActualId);
            return CreatedAtAction(nameof(GetConversacionPorId), new { id = nuevoGrupo.Id }, nuevoGrupo);
        }

        // POST: api/v1/conversaciones/directo
        // Iniciar un chat directo con otro usuario
        [HttpPost("directo")]
        public async Task<ActionResult<ConversacionResponse>> IniciarChatDirecto([FromBody] IniciarChatDirectoRequest request)
        {
            var usuarioActualId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Usuario {Id} está iniciando chat directo con {OtroUsuarioId}", usuarioActualId, request.OtroUsuarioId);

            if (string.IsNullOrEmpty(usuarioActualId))
            {
                return Unauthorized();
            }

            var conversacion = await _conversacionService.IniciarChatDirectoAsync(usuarioActualId, request.OtroUsuarioId);
            return CreatedAtAction(nameof(GetConversacionPorId), new { id = conversacion.Id }, conversacion);
        }

        // PUT: api/v1/conversaciones/{id}
        // Actualizar información de un grupo (solo admins)
        [HttpPut("{id}")]
        public async Task<ActionResult<ConversacionResponse>> ActualizarGrupo(Guid id, [FromBody] ActualizarGrupoRequest request)
        {
            var usuarioActualId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(usuarioActualId))
            {
                return Unauthorized();
            }

            var conversacionActualizada = await _conversacionService.ActualizarGrupoAsync(id, request, usuarioActualId);

            if (conversacionActualizada == null)
            {
                return NotFound(new ErrorResponse(null, "Grupo no encontrado", 404, "El grupo no existe, no es de tipo grupo o no tienes permisos de admin"));
            }

            return Ok(conversacionActualizada);
        }

        // DELETE: api/v1/conversacio/{id}
        // Eliminar conversación o salir de ella
        [HttpDelete("{id}")]
        public async Task<ActionResult> EliminarConversacion(Guid id)
        {
            var usuarioActualId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(usuarioActualId))
            {
                return Unauthorized();
            }

            var eliminado = await _conversacionService.EliminarConversacionAsync(id, usuarioActualId);

            if (!eliminado)
            {
                return NotFound(new ErrorResponse(null, "No se pudo eliminar", 404, "La conversación no existe o no tienes permisos para eliminarla"));
            }

            return NoContent();
        }

        // POST: api/v1/conversacion/{id}/miembros
        // Agregar un miembro a un grupo (solo admins)
        [HttpPost("{id}/miembros")]
        public async Task<ActionResult> AgregarMiembro(Guid id, [FromBody] AgregarMiembroRequest request)
        {
            var usuarioActualId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(usuarioActualId))
            {
                return Unauthorized();
            }

            var agregado = await _conversacionService.AgregarMiembroAsync(id, request.UsuarioId, usuarioActualId);

            if (!agregado)
            {
                return BadRequest(new ErrorResponse(null, "No se pudo agregar el miembro", 400, "Verifica que tienes permisos de admin y que el usuario no sea ya miembro del grupo"));
            }

            return Ok(new { message = "Miembro agregado exitosamente" });
        }

        // DELETE: api/v1/conversacion/{id}/miembros/{usuarioId}
        // Quitar un miembro de un grupo (solo admins o el mismo usuario saliendo)
        [HttpDelete("{id}/miembros/{usuarioId}")]
        public async Task<ActionResult> QuitarMiembro(Guid id, string usuarioId)
        {
            var usuarioActualId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(usuarioActualId))
            {
                return Unauthorized();
            }

            var quitado = await _conversacionService.QuitarMiembroAsync(id, usuarioId, usuarioActualId);

            if (!quitado)
            {
                return BadRequest(new ErrorResponse(null, "No se pudo quitar el miembro", 400, "Verifica que tienes permisos para realizar esta acción"));
            }

            return NoContent();
        }

        // GET: api/v1/conversacion/{id}/miembros
        // Obtener lista de miembros de una conversación
        [HttpGet("{id}/miembros")]
        public async Task<ActionResult<List<UsuarioResumenResponse>>> GetMiembros(Guid id)
        {
            var usuarioActualId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(usuarioActualId))
            {
                return Unauthorized();
            }

            var miembros = await _conversacionService.GetMiembrosAsync(id, usuarioActualId);
            return Ok(miembros);
        }
    }
}