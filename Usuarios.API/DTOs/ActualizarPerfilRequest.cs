namespace Usuarios.API.DTOs
{
    public record ActualizarPerfilRequest
(
        string Nombre,
        string? AvatarUrl
        );
}
