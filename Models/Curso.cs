using System;
using System.Collections.Generic;

namespace AsistenciaProcess.Models;

public partial class Curso
{
    public string Periodo { get; set; } = null!;

    public int Nrc { get; set; }

    public string Materia { get; set; } = null!;

    public string Curso1 { get; set; } = null!;

    public string? Seccion { get; set; }

    public string? NombreCurso { get; set; }

    public int? Capacidad { get; set; }

    public int? Ocupados { get; set; }

    public string? Campus { get; set; }

    public string CodigoDocente { get; set; } = null!;

    public DateTime? FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

}
