using Grupos.API.DTOs;

namespace Grupos.API.Services
{
    public interface IUsuariosApiClient
    {
        // Obtiene perfiles de usuario por lote desde Usuarios.API
        Task<List<UsuarioResumenResponse>> GetUsuariosBatchAsync(List<string> usuarioIds);
    }
}
