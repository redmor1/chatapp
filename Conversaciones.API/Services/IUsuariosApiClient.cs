using Conversaciones.API.DTOs;

namespace Conversaciones.API.Services
{
    public interface IUsuariosApiClient
    {
        // Obtiene perfiles de usuario por lote desde Usuarios.API
        Task<List<UsuarioResumenResponse>> GetUsuariosBatchAsync(List<string> usuarioIds);
        
        // Verifica si un usuario existe
        Task<bool> UsuarioExisteAsync(string usuarioId);
    }
}
