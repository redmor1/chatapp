using System;
using System.Collections.Generic;

namespace Mensajes.API.Entities;

public partial class AcusesLectura
{
    public Guid Id { get; set; }

    public Guid MensajeId { get; set; }

    public string UsuarioId { get; set; } = null!;

    public DateTime? LeidoEn { get; set; }

    public virtual Mensaje Mensaje { get; set; } = null!;
}
