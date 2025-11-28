using System;
using System.Collections.Generic;

namespace Conversaciones.API.Entities;

public partial class Conversacion
{
    public Guid Id { get; set; }

    public string Tipo { get; set; } = null!; // "directo" o "grupo"

    public string? Nombre { get; set; } // Solo para grupos

    public string? AvatarUrl { get; set; }

    public string? CreadorId { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public virtual ICollection<MiembrosConversacion> MiembrosConversacion { get; set; } = new List<MiembrosConversacion>();
}
