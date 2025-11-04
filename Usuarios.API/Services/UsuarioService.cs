using Microsoft.EntityFrameworkCore;
using Usuarios.API.Data;
using Usuarios.API.DTOs;
using Usuarios.API.Entities;

namespace Usuarios.API.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly UsuariosDbContext _context;
        private readonly ILogger<UsuarioService> _logger;

        public UsuarioService(UsuariosDbContext context, ILogger<UsuarioService> logger)
        {
            _context = context;
            _logger = logger;
        }


        public async Task<UsuarioPerfilResponse?> GetPerfilPorIdAsync(string usuarioId)
        {
            _logger.LogInformation("Buscando perfil para el usuario {Id}", usuarioId);

            var usuario = await _context.Usuarios.FindAsync(usuarioId);

            if (usuario == null)
            {
                _logger.LogWarning("Perfil de usuario no encontrado en BD local para Auth0 ID: {Id}", usuarioId);
                return null;
            }

            return MapToDto(usuario);
        }


        public async Task<UsuarioPerfilResponse?> UpdatePerfilAsync(string usuarioId, ActualizarPerfilRequest request)
        {
            _logger.LogInformation("Actualizando perfil para el usuario {Id}", usuarioId);

            var usuario = await _context.Usuarios.FindAsync(usuarioId);

            if (usuario == null)
            {
                _logger.LogWarning("Usuario no encontrado para actualizar: {Id}", usuarioId);
                return null;
            }

            // Actualizar solo los campos proporcionados
            if (!string.IsNullOrWhiteSpace(request.Nombre))
            {
                usuario.Nombre = request.Nombre;
            }

            if (request.AvatarUrl != null)
            {
                usuario.AvatarUrl = request.AvatarUrl;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Perfil actualizado exitosamente para {Id}", usuarioId);

            return MapToDto(usuario);
        }


        public async Task<List<UsuarioPerfilResponse>> GetUsuariosBatchAsync(List<string> usuarioIds)
        {
            _logger.LogInformation("Consultando {Count} usuarios en lote", usuarioIds.Count);

            var usuarios = await _context.Usuarios
                .Where(u => usuarioIds.Contains(u.Id))
                .ToListAsync();

            _logger.LogInformation("Encontrados {Count} usuarios de {Total} solicitados", usuarios.Count, usuarioIds.Count);

            return usuarios.Select(MapToDto).ToList();
        }

        public async Task SyncUsuarioAsync(SyncUsuarioRequest request)
        {
            _logger.LogInformation("Sincronizando usuario {Id} desde Auth0", request.Id);

            var usuario = await _context.Usuarios.FindAsync(request.Id);

            if (usuario == null)
            {
                // Crear nuevo usuario
                usuario = new Usuario
                {
                    Id = request.Id,
                    Nombre = request.Nombre,
                    Email = request.Email,
                    AvatarUrl = request.AvatarUrl
                };

                _context.Usuarios.Add(usuario);
                _logger.LogInformation("Nuevo usuario creado: {Id}", request.Id);
            }
            else
            {
                // Actualizar usuario existente
                usuario.Nombre = request.Nombre;
                usuario.Email = request.Email;
                usuario.AvatarUrl = request.AvatarUrl;

                _logger.LogInformation("Usuario actualizado: {Id}", request.Id);
            }

            await _context.SaveChangesAsync();
        }

        // Método auxiliar: Mapear Entidad -> DTO
        private UsuarioPerfilResponse MapToDto(Usuario usuario)
        {
            return new UsuarioPerfilResponse(
                usuario.Id,
                usuario.Nombre,
                usuario.Email,
                usuario.AvatarUrl
            );
        }
    }
}
