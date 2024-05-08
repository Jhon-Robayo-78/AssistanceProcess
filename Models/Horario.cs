using System;
using System.Collections.Generic;

namespace AsistenciaProcess.Models;

public partial class Horario
{
    public int Id { get; set; }

    public string Edf { get; set; } = null!;

    public string Salon { get; set; } = null!;

    public int? Nrc { get; set; }

    public string? Periodo { get; set; }

    public TimeSpan? HoraInicio { get; set; }

    public TimeSpan? HoraFin { get; set; }

    public string? DayWeek { get; set; }

}
