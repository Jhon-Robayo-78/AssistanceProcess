using System;
using System.Collections.Generic;

namespace AsistenciaProcess.Models;

public partial class Usuario
{
    public string Id { get; set; } = null!;

    public string? Nombre { get; set; }

    public string? Apellido { get; set; }

    public string? Rol { get; set; }

    public string Email { get; set; } = null!;

    public string EntraId { get; set; } = null!;
}
