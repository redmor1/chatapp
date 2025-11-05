using System.ComponentModel.DataAnnotations;

namespace Mensajes.API.DTOs
{

    public record CrearMensajeRequest(
        [Required]
        [MinLength(1)]
        [MaxLength(5000)]
        string Contenido
    );
}
