using System.ComponentModel.DataAnnotations;

namespace Grupos.API.DTOs

{
    public record AgregarMiembroRequest
(
        [Required]
        string UsuarioId
        );
}
