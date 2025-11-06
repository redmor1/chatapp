using Conversaciones.API.DTOs;

namespace Conversaciones.API.Services
{
    public interface IUsuariosApiClient
    {
        // Obtiene perfiles de usuario por lote desde Usuarios.API
        Task<List<UsuarioResumenResponse>> GetUsuariosBatchAsync(List<string> usuarioIds);
    }
}
