using System.ComponentModel.DataAnnotations;

namespace Mensajes.API.DTOs
{

    public record MarcarComoLeidoRequest(
        [Required]
        DateTime Timestamp
    );
}
