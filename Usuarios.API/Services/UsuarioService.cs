using Usuarios.API.Data;
using Usuarios.API.DTOs;

namespace Usuarios.API.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly UsuariosDbContext _context;
        private readonly ILogger<UsuarioService> _logger;


        public UsuarioService(UsuariosDbContext context, ILogger<UsuarioService> logger) {
            _context = context;
            _logger = logger;

        }

        public async Task<UsuarioPerfilResponse> GetPerfilPorIdAsync(string usuarioActualId)
        {
            _logger.LogInformation("Buscando perfil para el usuario {Id}", usuarioActualId);
            
            var usuario = await _context.Usuarios.FindAsync(usuarioActualId);

            // Si no se encuentra, devuelve null
            if (usuario == null)
            {
                _logger.LogWarning("Perfil de usuario no encontrado en BD local para Auth0 ID: {Id}", usuarioActualId);
                return null;
            }

            // Mapeo de Entidad -> DTO
            var responseDto = new UsuarioPerfilResponse(
                usuario.Id,
                usuario.Nombre,
                usuario.Email,
                usuario.AvatarUrl
            );

            return responseDto;

        }
    }
}
