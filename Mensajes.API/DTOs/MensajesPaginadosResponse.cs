namespace Mensajes.API.DTOs
{

    public record MensajesPaginadosResponse(
        List<MensajeResponse> Mensajes,
        string? NextCursor
    );
}
