namespace Mensajes.API.DTOs
{
    public record ErrorResponse(
        string Tipo,
        string Titulo,
        string Estado,
        string Detalle
    );
}
