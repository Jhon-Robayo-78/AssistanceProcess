using System;
using System.Collections.Generic;

namespace AsistenciaProcess.Models;

public partial class Asistencium
{
    public string IdUser { get; set; } = null!;

    public int IdHorario { get; set; }

    public int Id { get; set; }

    public string Dato { get; set; } = null!;

    public DateTime? Fecha { get; set; }

}
