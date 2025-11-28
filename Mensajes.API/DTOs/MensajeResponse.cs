namespace Mensajes.API.DTOs
{
    public class MensajeResponse
    {
        public Guid Id { get; set; }
        public string ConversacionId { get; set; } = string.Empty;
        public string AutorId { get; set; } = string.Empty;
        public string Contenido { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public List<string> LeidoPor { get; set; } = new List<string>();
    }
}
