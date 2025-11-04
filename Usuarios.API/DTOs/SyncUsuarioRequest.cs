using System.ComponentModel.DataAnnotations;

namespace Usuarios.API.DTOs
{
    public record SyncUsuarioRequest
(
    [Required]
    string Id,
    [Required]
    string Nombre,
    [Required]
    string Email,
    string? AvatarUrl
        );
}
