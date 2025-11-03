namespace Grupos.API.DTOs
{
    public record ActualizarGrupoRequest
    (
        string? Nombre,
        string? AvatarUrl
    );
}
