namespace Usuarios.API.DTOs
{
    public record UsuarioPerfilResponse
(
        string Id,
        string Nombre,
        string Email,
        string? AvatarUrl
        );
}
