using System;
using System.Collections.Generic;

namespace Conversaciones.API.Entities;

public partial class MiembrosConversacion
{
    public Guid Id { get; set; }

    public Guid ConversacionId { get; set; }

    public string UsuarioId { get; set; } = null!;

    public string Rol { get; set; } = null!; // "miembro" o "admin"

    public virtual Conversacion Conversacion { get; set; } = null!;
}
