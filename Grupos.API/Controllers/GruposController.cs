using Grupos.API.Data;
using Grupos.API.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Grupos.API.Controllers
{
    [ApiController]
    [Route("api/v1/grupo")]
    [Authorize]
    public class GruposController : ControllerBase
    {

        private readonly ILogger<GruposController> _logger;
        private readonly GruposDbContext _context;

        public GruposController(ILogger<GruposController> logger, GruposDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Grupo>>> GetGrupos()
        {

            // Obtener ID del usuario actual desde el token
            var usuarioActualId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(usuarioActualId))
            {
                return Unauthorized();
            }

            var grupos = await _context.Grupos.Where(g => g.MiembrosGrupos.Any(m => m.UsuarioId == usuarioActualId)).ToListAsync();
            return Ok(grupos);
        }
    }
}
