using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Usuarios.API.DTOs;
using Usuarios.API.Services;

namespace Usuarios.API.Controllers
{
    [ApiController]
    [Route("api/v1/usuario")]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly ILogger<UsuariosController> _logger;
        private readonly IUsuarioService _usuarioService;
        private readonly IConfiguration _configuration;

        public UsuariosController(
            ILogger<UsuariosController> logger, 
            IUsuarioService usuarioService,
            IConfiguration configuration)
        {
            _logger = logger;
            _usuarioService = usuarioService;
            _configuration = configuration;
        }

        // GET /api/v1/usuario/me
        [HttpGet("me")]
        public async Task<ActionResult<UsuarioPerfilResponse>> GetMiPerfil()
        {
            var usuarioActualId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(usuarioActualId))
            {
                return Unauthorized();
            }

            var usuarioDto = await _usuarioService.GetPerfilPorIdAsync(usuarioActualId);

            if (usuarioDto == null)
            {
                return NotFound(new ErrorResponse(
                    "https://api.chatapp.com/errores/usuario-no-encontrado",
                    "Usuario no encontrado",
                    404,
                    "Perfil de usuario no encontrado en la base de datos local. Pendiente de sincronización."
                ));
            }

            return Ok(usuarioDto);
        }

        // PATCH /api/v1/usuario/me
        [HttpPatch("me")]
        public async Task<ActionResult<UsuarioPerfilResponse>> UpdateMiPerfil([FromBody] ActualizarPerfilRequest request)
        {
            var usuarioActualId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(usuarioActualId))
            {
                return Unauthorized();
            }

            var usuarioDto = await _usuarioService.UpdatePerfilAsync(usuarioActualId, request);

            if (usuarioDto == null)
            {
                return NotFound(new ErrorResponse(
                    "https://api.chatapp.com/errores/usuario-no-encontrado",
                    "Usuario no encontrado",
                    404,
                    "No se pudo actualizar el perfil porque el usuario no existe."
                ));
            }

            return Ok(usuarioDto);
        }

        // POST /api/v1/usuario/batch
        [HttpPost("batch")]
        [AllowAnonymous] // Requiere API Key en header en lugar de JWT
        public async Task<ActionResult<List<UsuarioPerfilResponse>>> GetUsuariosBatch(
            [FromBody] BatchIdsRequest request,
            [FromHeader(Name = "X-API-Key")] string? apiKey)
        {
            // Validar API Key
            var expectedApiKey = _configuration["Services:InterServiceApiKey"];
            if (string.IsNullOrEmpty(apiKey) || apiKey != expectedApiKey)
            {
                _logger.LogWarning("Intento de acceso al endpoint /batch sin API Key válida");
                return Unauthorized(new ErrorResponse(
                    "https://api.chatapp.com/errores/no-autorizado",
                    "API Key inválida",
                    401,
                    "La API Key es requerida para este endpoint."
                ));
            }

            if (request.Ids == null || request.Ids.Count == 0)
            {
                return BadRequest(new ErrorResponse(
                    "https://api.chatapp.com/errores/validacion",
                    "Error de validación",
                    400,
                    "La lista de IDs no puede estar vacía."
                ));
            }

            var usuarios = await _usuarioService.GetUsuariosBatchAsync(request.Ids);
            return Ok(usuarios);
        }

        // POST /api/v1/usuario/sync
        [HttpPost("sync")]
        [AllowAnonymous] // No requiere JWT, usa API Key
        public async Task<IActionResult> SyncUsuarioDesdeAuth0(
            [FromBody] SyncUsuarioRequest request, 
            [FromHeader(Name = "X-Sync-Key")] string? syncKey)
        {
            // Validar API Key desde appsettings
            var expectedKey = _configuration["Auth0:SyncKey"];
            
            if (string.IsNullOrEmpty(syncKey) || syncKey != expectedKey)
            {
                return Unauthorized(new ErrorResponse(
                    "https://api.chatapp.com/errores/no-autorizado",
                    "API Key inválida",
                    401,
                    "La API Key proporcionada es inválida o faltante."
                ));
            }

            await _usuarioService.SyncUsuarioAsync(request);
            return Ok(new { message = "Usuario sincronizado exitosamente" });
        }
    }
}
