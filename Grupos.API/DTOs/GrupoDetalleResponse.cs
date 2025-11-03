namespace Grupos.API.DTOs
{
    public record GrupoDetalleResponse
(
        string Id,
        string Nombre,
        string? AvatarUrl,
        List<UsuarioResumenResponse> Miembros

        );
}
