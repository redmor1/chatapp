using System;
using System.Collections.Generic;

namespace Grupos.API.Entidades;

public partial class MiembrosGrupo
{
    public Guid Id { get; set; }

    public Guid GrupoId { get; set; }

    public string UsuarioId { get; set; } = null!;

    public string Rol { get; set; } = null!;

    public virtual Grupo Grupo { get; set; } = null!;
}
