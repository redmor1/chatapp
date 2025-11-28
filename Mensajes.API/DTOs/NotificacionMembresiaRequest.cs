namespace Mensajes.API.DTOs
{
    public class NotificacionMembresiaRequest
    {
        public string Tipo { get; set; } // "Agregar" o "Quitar"
        public string ConversacionId { get; set; }
        public string UsuarioId { get; set; } // El usuario afectado
        public string? NombreGrupo { get; set; } // Opcional, para mostrar en UI
    }
}
