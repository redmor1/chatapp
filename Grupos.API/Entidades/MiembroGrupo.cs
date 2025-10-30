using Microsoft.EntityFrameworkCore;

namespace Grupos.API.Entidades
{
    [PrimaryKey(nameof(GrupoId), nameof(UsuarioId))]
    public class MiembroGrupo
    {
        // FK a la tabla Grupo
        public Guid GrupoId { get; set; }
        public Grupo Grupo { get; set; } // Propiedad de navegación

        // FK al "Usuario".
        // Nota: El usuario "vive" en Keycloak y en Usuarios.API.
        // Aca solo guardamos su ID para saber que es miembro.
        public Guid UsuarioId { get; set; }
    }
}
