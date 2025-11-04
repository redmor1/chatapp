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
        private readonly UsuarioService _usuarioService;

        public UsuariosController(ILogger<UsuariosController> logger, UsuarioService usuarioService)
        {
            _logger = logger;
            _usuarioService = usuarioService;
        }

        [HttpGet("me")]
        public async Task<ActionResult<UsuarioPerfilResponse>> GetMiPerfil()
        {
            var usuarioActualId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(usuarioActualId))
            {
                return Unauthorized();
            }

            // Llamar al servicio
            var usuarioDto = await _usuarioService.GetPerfilPorIdAsync(usuarioActualId);

            
            if (usuarioDto == null)
            {
                // El servicio devolvió null, así que respondemos con 404
                return NotFound("Perfil de usuario no encontrado. Pendiente de sincronización.");
            }

            // Si todo está bien, devuelve el DTO
            return Ok(usuarioDto);
        }

    }
}
