namespace Conversaciones.API.DTOs
{
    public record GrupoDetalleResponse
(
        Guid Id,
        string Nombre,
        string? AvatarUrl,
        List<UsuarioResumenResponse> Miembros

        );
}
