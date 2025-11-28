namespace Mensajes.API.DTOs
{
    public class AcuseLecturaResponse
    {
        public Guid MensajeId { get; set; }
        public string UsuarioId { get; set; } = string.Empty;
        public DateTime LeidoEn { get; set; }
    }
}
