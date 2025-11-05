using System;
using System.Collections.Generic;

namespace Mensajes.API.Entities;

public partial class Mensaje
{
    public Guid Id { get; set; }

    public string Contenido { get; set; } = null!;

    public string ConversacionId { get; set; } = null!;

    public string ConversacionTipo { get; set; } = null!;

    public string AutorId { get; set; } = null!;

    public DateTime? FechaCreacion { get; set; }

    public virtual ICollection<AcusesLectura> AcusesLecturas { get; set; } = new List<AcusesLectura>();
}
