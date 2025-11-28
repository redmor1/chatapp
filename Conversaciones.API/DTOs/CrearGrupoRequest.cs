using System.ComponentModel.DataAnnotations;

namespace Conversaciones.API.DTOs
{
    public record CrearGrupoRequest
    (
        [Required]
        string Nombre,
        List<string>? EmailsMiembros // Lista de emails en lugar de IDs
    );
}
