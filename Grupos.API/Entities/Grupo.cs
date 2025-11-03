using System;
using System.Collections.Generic;

namespace Grupos.API.Entidades;

public partial class Grupo
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public string CreadorId { get; set; } = null!;

    public DateTime? FechaCreacion { get; set; }

    public virtual ICollection<MiembrosGrupo> MiembrosGrupos { get; set; } = new List<MiembrosGrupo>();
}
