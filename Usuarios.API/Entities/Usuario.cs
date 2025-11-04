using System;
using System.Collections.Generic;

namespace Usuarios.API.Entities;

public partial class Usuario
{
    public string Id { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? AvatarUrl { get; set; }
}
