using Usuarios.API.DTOs;

namespace Usuarios.API.Services
{
    public interface IUsuarioService
    {

        Task<UsuarioPerfilResponse?> GetPerfilPorIdAsync(string usuarioId);

        Task<UsuarioPerfilResponse?> UpdatePerfilAsync(string usuarioId, ActualizarPerfilRequest request);

        Task<List<UsuarioPerfilResponse>> GetUsuariosBatchAsync(List<string> usuarioIds);

        Task SyncUsuarioAsync(SyncUsuarioRequest request);

        Task<UsuarioPerfilResponse?> GetUsuarioPorEmailAsync(string email);
    }
}
