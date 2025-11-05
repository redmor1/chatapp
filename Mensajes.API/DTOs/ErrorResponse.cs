namespace Mensajes.API.DTOs
{
    public record ErrorResponse(
        string Tipo,
        string Titulo,
        int Estado,
        string Detalle
    );
}
