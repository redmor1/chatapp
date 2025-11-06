namespace Conversaciones.API.DTOs
{
    public record ConversacionResponse(
        Guid Id,
        string Tipo, // "directo" o "grupo"
        string? Nombre, // Solo para grupos
        string? AvatarUrl,
        List<UsuarioResumenResponse> Miembros
    );
}
