namespace Conversaciones.API.DTOs
{
    public record ErrorResponse
    (
        string? Tipo,
            string Titulo,
            int Estado,
            string? Detalle
    );
}
