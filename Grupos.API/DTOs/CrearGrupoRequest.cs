using System.ComponentModel.DataAnnotations;

namespace Grupos.API.DTOs
{
    public record CrearGrupoRequest
    (
        [Required]
        string Nombre,
        List<string> MiembrosIniciales // Usamos 'string' para los IDs de Auth0
    );
}
