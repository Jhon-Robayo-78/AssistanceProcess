using System;
using System.Collections.Generic;

namespace AsistenciaProcess.Models;

public partial class Matricula
{
    public int Id { get; set; }

    public int FkNrc { get; set; }

    public string? Periodo { get; set; }

    public string? UserId { get; set; }

    public DateTime? FechaDeMatricula { get; set; }

}
