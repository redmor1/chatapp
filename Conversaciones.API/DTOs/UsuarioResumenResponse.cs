namespace Conversaciones.API.DTOs
{
    public record UsuarioResumenResponse(
        string Id,
        string Nombre,
        string? AvatarUrl
    );
}
