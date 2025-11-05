namespace Grupos.API.DTOs
{
    public record UsuarioResumenResponse(
        string Id,
        string Nombre,
        string? AvatarUrl
    );
}
