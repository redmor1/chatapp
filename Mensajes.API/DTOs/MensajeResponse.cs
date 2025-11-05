namespace Mensajes.API.DTOs
{

    public record MensajeResponse(
        string Id,
        string ConversacionId,
        string AutorId,
        string Contenido,
        DateTime CreadoEn
    );
}
