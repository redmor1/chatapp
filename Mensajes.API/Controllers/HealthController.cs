using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mensajes.API.Controllers
{
    [ApiController]
    [Route("api/v1/health")]
    [AllowAnonymous]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;

        public HealthController(ILogger<HealthController> logger)
        {
            _logger = logger;
        }

        // Verifica el estado de salud de la API
        [HttpGet]
        public IActionResult Health()
        {
            _logger.LogInformation("Health check llamado");
            
            return Ok(new 
            { 
                status = "healthy",
                service = "Usuarios.API",
                timestamp = DateTime.UtcNow
            });
        }
    }
}