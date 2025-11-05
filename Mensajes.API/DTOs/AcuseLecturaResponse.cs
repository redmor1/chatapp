namespace Mensajes.API.DTOs
{

    public record AcuseLecturaResponse(
        string UsuarioId,
        string NombreUsuario,
        string? AvatarUrl,
        DateTime LeidoEn
    );
}
