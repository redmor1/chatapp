namespace Mensajes.API.DTOs
{
    public class MensajesPaginadosResponse
    {
        public List<MensajeResponse> Mensajes { get; set; } = new List<MensajeResponse>();
        public string? NextCursor { get; set; }
    }
}
