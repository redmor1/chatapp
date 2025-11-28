using Mensajes.API.DTOs;
using Mensajes.API.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Mensajes.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class NotificacionesController : ControllerBase
    {
        private readonly IHubContext<MensajesHub> _hubContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotificacionesController> _logger;

        public NotificacionesController(
            IHubContext<MensajesHub> hubContext, 
            IConfiguration configuration,
            ILogger<NotificacionesController> logger)
        {
            _hubContext = hubContext;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("membresia")]
        public async Task<IActionResult> NotificarCambioMembresia([FromBody] NotificacionMembresiaRequest request, [FromHeader(Name = "X-API-Key")] string apiKey)
        {
            var configuredApiKey = _configuration["Services:InterServiceApiKey"];
            if (apiKey != configuredApiKey)
            {
                return Unauthorized("API Key inválida");
            }

            _logger.LogInformation("Recibida notificación de membresía: {Tipo} usuario {UsuarioId} en conversacion {ConversacionId}", 
                request.Tipo, request.UsuarioId, request.ConversacionId);

            if (request.Tipo == "Agregar")
            {
                // 1. Notificar al grupo que alguien nuevo entró (para actualizar lista de miembros en UI)
                await _hubContext.Clients.Group(request.ConversacionId).SendAsync("UsuarioAgregado", new 
                { 
                    ConversacionId = request.ConversacionId, 
                    UsuarioId = request.UsuarioId 
                });

                // 2. Notificar al usuario ESPECÍFICO que fue agregado (para que le aparezca el chat en su lista)
                // Usamos el grupo con el nombre del UserIdentifier que configuramos en OnConnectedAsync
                await _hubContext.Clients.Group(request.UsuarioId).SendAsync("TeHanAgregado", new 
                { 
                    ConversacionId = request.ConversacionId,
                    Nombre = request.NombreGrupo
                });
            }
            else if (request.Tipo == "Quitar")
            {
                // 1. Notificar al grupo que alguien se fue
                await _hubContext.Clients.Group(request.ConversacionId).SendAsync("UsuarioEliminado", new 
                { 
                    ConversacionId = request.ConversacionId, 
                    UsuarioId = request.UsuarioId 
                });

                // 2. Notificar al usuario que fue eliminado (para quitar el chat de su lista o avisarle)
                await _hubContext.Clients.Group(request.UsuarioId).SendAsync("TeHanEliminado", new 
                { 
                    ConversacionId = request.ConversacionId 
                });
            }

            return Ok();
        }
    }
}
