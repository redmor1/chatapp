using System.ComponentModel.DataAnnotations;

namespace Conversaciones.API.DTOs

{
    public record AgregarMiembroRequest
(
        [Required]
        [EmailAddress]
        string Email
        );
}
