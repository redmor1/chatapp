using Usuarios.API.DTOs;

namespace Usuarios.API.Services
{
    public interface IUsuarioService
    {
        Task<UsuarioPerfilResponse> GetPerfilPorIdAsync(string usuarioActualId);
    }
}
