using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AsistenciaProcess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web.Resource;
using SpreadsheetLight;
using System.Data;

namespace AsistenciaProcess.Controllers
{

    [RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
    [Route("api/[controller]")]
    [ApiController]
    public class ReporteAsistenciasController : ControllerBase
    {
        private readonly AssistanceProcessesContext _context;
        public ReporteAsistenciasController(AssistanceProcessesContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "estudiante,admin,profesor")]
        [HttpGet("[action]")]
        public async Task<IActionResult> AssistancePerStudents([FromQuery] string id, [FromQuery] string periodo)
        {
            var searchMatriculas = await _context.Matriculas.Where(ma => ma.UserId == id && ma.Periodo == periodo).ToListAsync();

            List<object> assistanceList = new List<object>();

            foreach (Matricula matricula in searchMatriculas)
            {
                var horario = await _context.Horarios.FirstOrDefaultAsync(h => h.Periodo == periodo && h.Nrc == matricula.FkNrc);

                if (horario != null)
                {
                    var searchAsistencia = await _context.Asistencia.Where(a => a.IdUser == id && a.IdHorario == horario.Id).ToListAsync();

                    int asistencias = searchAsistencia.Count(a => a.Dato == "P");
                    int faltas = searchAsistencia.Count(a => a.Dato == "F");
                    int ausencias = searchAsistencia.Count(a => a.Dato == "E");

                    var curso = await _context.Cursos.FirstOrDefaultAsync(cu => cu.Nrc == horario.Nrc && cu.Periodo == periodo);

                    if (curso != null)
                    {
                        var assistanceObject = new
                        {
                            NombreDeLaAsignatura = curso.NombreCurso,
                            NRC = curso.Nrc,
                            Asistencias = asistencias,
                            Faltas = faltas,
                            Ausencias = ausencias
                        };

                        assistanceList.Add(assistanceObject);
                    }
                }
            }

            return Ok(assistanceList);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> ReportByFaculty([FromQuery] string periodo, [FromQuery] string facultad) 
        {
            
            SLDocument document = new SLDocument();
            DataTable dt = new DataTable();
            dt.Columns.Add("nrc", typeof(string));
            dt.Columns.Add("Nombre Materia", typeof(string));
            dt.Columns.Add("Materia", typeof(string));
            dt.Columns.Add("Seccion", typeof(string));
            dt.Columns.Add("periodo", typeof(string));
            dt.Columns.Add("codigo docente", typeof(string));
            dt.Columns.Add("nombre", typeof(string));
            dt.Columns.Add("apellido", typeof(string));
            dt.Columns.Add("email", typeof(string));
            dt.Columns.Add("numero asistencias", typeof(string));
            dt.Columns.Add("faltas", typeof (string));
            dt.Columns.Add("excusas", typeof(string));
            dt.Columns.Add("porcentaje", typeof(string));

            List<Usuario> usuarios = new List<Usuario>();
            List<object> reporte = new List<object>();
            List<Horario> horarios = new List<Horario>();
            var searchCursos = await _context.Cursos.Where(cu => cu.Periodo == periodo && cu.Materia == facultad).ToListAsync();
            if(searchCursos == null)
            {
                return NotFound();
            }
            foreach (Curso curso in searchCursos)
            {
                var searchUser = await _context.Usuarios.Where(user => user.Id == curso.CodigoDocente).FirstOrDefaultAsync();
                if (searchUser == null)
                {
                    // Si no se encuentra el usuario, establecer valores predeterminados
                    searchUser = new Usuario { Nombre = "", Apellido = "", Email = "" };
                }
                var searchHorarios = await _context.Horarios.Where(Ho => Ho.Nrc == curso.Nrc && Ho.Periodo == periodo).Select(h => h.Id).ToListAsync();
                var searchAsistencias = await _context.Asistencia.Where(asis => searchHorarios.Contains(asis.IdHorario)).ToListAsync();
                if (searchUser == null && searchHorarios == null)
                {
                    return NotFound(); 
                }
                DataRow fila = dt.NewRow();
                fila["nrc"] = curso.Nrc;
                fila["Nombre Materia"] = curso.NombreCurso;
                fila["Materia"] = curso.Materia;
                fila["Seccion"] = curso.Seccion;
                fila["periodo"] = periodo;
                fila["codigo docente"] = curso.CodigoDocente ?? "";
                fila["nombre"] = searchUser?.Nombre ?? "";
                fila["apellido"] = searchUser?.Apellido ?? "";
                fila["email"] = searchUser?.Email ?? "";
                int numeroAsistencias = searchAsistencias.Count(dato => dato.Dato == "P");
                int numeroFaltas = searchAsistencias.Count(dato => dato.Dato == "F");
                int numeroExcusas = searchAsistencias.Count(dato => dato.Dato == "E");
                fila["numero asistencias"] = numeroAsistencias;
                fila["faltas"] = numeroFaltas;
                fila["excusas"] =  numeroExcusas;
                fila["porcentaje"] = CalcularPorcentaje(numeroAsistencias, numeroFaltas, numeroExcusas);
                dt.Rows.Add(fila);
            }
            document.ImportDataTable(1, 1, dt, true);
            // Guardar el archivo Excel en el servidor
            string pathFile = AppDomain.CurrentDomain.BaseDirectory + "Prueba_Reporte_Facultad_Excel.xlsx";
            document.SaveAs(pathFile);

            // Leer el contenido del archivo Excel como bytes
            byte[] fileBytes = System.IO.File.ReadAllBytes(pathFile);

            // Devolver el archivo Excel como respuesta HTTP
            // Establecer el tipo de contenido en la respuesta
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            // Nombre del archivo que se enviará al cliente
            string fileName = $"Reporte_Facultad_{facultad}_{periodo}.xlsx";
            // Devolver el archivo como un File Content Result
            return File(fileBytes, contentType, fileName);
        }
        /*
        [HttpGet("[action]")]
        public async Task<IActionResult> ReportByFaculty([FromQuery] string periodo, [FromQuery] string facultad)
        {
            Workbook workbook = new Workbook();
            Worksheet worksheet = workbook.Worksheets[0];

            // Tu lógica para obtener los datos y llenar el DataTable dt aquí...
            DataTable dt = new DataTable();
            dt.Columns.Add("nrc", typeof(string));
            dt.Columns.Add("Nombre Materia", typeof(string));
            dt.Columns.Add("Materia", typeof(string));
            dt.Columns.Add("Seccion", typeof(string));
            dt.Columns.Add("periodo", typeof(string));
            dt.Columns.Add("codigo docente", typeof(string));
            dt.Columns.Add("nombre", typeof(string));
            dt.Columns.Add("apellido", typeof(string));
            dt.Columns.Add("email", typeof(string));
            dt.Columns.Add("numero asistencias", typeof(string));
            dt.Columns.Add("faltas", typeof(string));
            dt.Columns.Add("excusas", typeof(string));
            dt.Columns.Add("porcentaje", typeof(string));

            // Crear una fila para los encabezados de las columnas
            DataRow headerRow = dt.NewRow();
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                headerRow[i] = dt.Columns[i].ColumnName;
            }
            dt.Rows.InsertAt(headerRow, 0);

            List<Usuario> usuarios = new List<Usuario>();
            List<object> reporte = new List<object>();
            List<Horario> horarios = new List<Horario>();
            var searchCursos = await _context.Cursos.Where(cu => cu.Periodo == periodo && cu.Materia == facultad).ToListAsync();
            if (searchCursos == null)
            {
                return NotFound();
            }
            foreach (Curso curso in searchCursos)
            {
                var searchUser = await _context.Usuarios.Where(user => user.Id == curso.CodigoDocente).FirstOrDefaultAsync();
                if (searchUser == null)
                {
                    // Si no se encuentra el usuario, establecer valores predeterminados
                    searchUser = new Usuario { Nombre = "", Apellido = "", Email = "" };
                }
                var searchHorarios = await _context.Horarios.Where(Ho => Ho.Nrc == curso.Nrc && Ho.Periodo == periodo).Select(h => h.Id).ToListAsync();
                var searchAsistencias = await _context.Asistencia.Where(asis => searchHorarios.Contains(asis.IdHorario)).ToListAsync();
                if (searchUser == null && searchHorarios == null)
                {
                    return NotFound();
                }
                DataRow fila = dt.NewRow();
                fila["nrc"] = curso.Nrc;
                fila["Nombre Materia"] = curso.NombreCurso;
                fila["Materia"] = curso.Materia;
                fila["Seccion"] = curso.Seccion;
                fila["periodo"] = periodo;
                fila["codigo docente"] = curso.CodigoDocente ?? "";
                fila["nombre"] = searchUser?.Nombre ?? "";
                fila["apellido"] = searchUser?.Apellido ?? "";
                fila["email"] = searchUser?.Email ?? "";
                int numeroAsistencias = searchAsistencias.Count(dato => dato.Dato == "P");
                int numeroFaltas = searchAsistencias.Count(dato => dato.Dato == "F");
                int numeroExcusas = searchAsistencias.Count(dato => dato.Dato == "E");
                fila["numero asistencias"] = numeroAsistencias;
                fila["faltas"] = numeroFaltas;
                fila["excusas"] = numeroExcusas;
                fila["porcentaje"] = CalcularPorcentaje(numeroAsistencias, numeroFaltas, numeroExcusas);
                dt.Rows.Add(fila);
            }
            // Llenar los datos en el worksheet
            for (int rowIndex = 0; rowIndex < dt.Rows.Count; rowIndex++)
            {
                DataRow row = dt.Rows[rowIndex];
                for (int colIndex = 0; colIndex < dt.Columns.Count; colIndex++)
                {
                    worksheet.Cells[rowIndex + 1, colIndex].PutValue(row[colIndex]);
                }
            }

            // Guardar el archivo Excel
            string pathFile = AppDomain.CurrentDomain.BaseDirectory + "Prueba_Reporte_Facultad_Excel.xlsx";
            workbook.Save(pathFile, SaveFormat.Xlsx);

            // Leer el contenido del archivo Excel como bytes
            byte[] fileBytes = System.IO.File.ReadAllBytes(pathFile);

            // Devolver el archivo Excel como respuesta HTTP
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reporte_Facultad_" + facultad + "_" + periodo + ".xlsx");
        }
        */
        [HttpGet("[action]")]
        public async Task<IActionResult> GetAssistancePerProfessor([FromQuery] string periodo, [FromQuery] string idUser) 
        {
            SLDocument document = new SLDocument();
            System.Data.DataTable dt = new System.Data.DataTable();
            var nrcs = await _context.Matriculas.Where(ma => ma.UserId == idUser && ma.Periodo == periodo).Select(ma=>ma.FkNrc).ToListAsync();
                     
            dt.Columns.Add("Curso", typeof(string));
            dt.Columns.Add("NRC", typeof(int));
            dt.Columns.Add("Seccion", typeof(string));
            dt.Columns.Add("Periodo", typeof(string));
            dt.Columns.Add("ID", typeof(string));
            dt.Columns.Add("Nombres", typeof(string));
            dt.Columns.Add("Apellidos", typeof(string));
            dt.Columns.Add("Email", typeof(string));
            dt.Columns.Add("Asistencias", typeof(int));
            dt.Columns.Add("Faltas", typeof(int));
            dt.Columns.Add("Ausencias notificadas", typeof(int));
            dt.Columns.Add("Porcentaje", typeof(string));
            var user = await _context.Usuarios.Where(u => u.Id == idUser).FirstOrDefaultAsync();
            foreach (var nrc in nrcs)
            {
                var searchCurso = await _context.Cursos.Where(cu => cu.Nrc == nrc && cu.Periodo == periodo).FirstOrDefaultAsync();
                var searchHorarioid = await _context.Horarios.Where(ho => ho.Nrc == nrc && ho.Periodo == periodo).Select(ho => ho.Id).ToListAsync();
                var searchAsistencia = await _context.Asistencia.Where(asis => searchHorarioid.Contains(asis.IdHorario) && asis.IdUser == idUser).ToListAsync();
                DataRow fila = dt.NewRow();
                // Asignar los valores fijos a la fila
                fila["Curso"] = searchCurso?.NombreCurso ?? "";
                fila["NRC"] = nrc;
                fila["Seccion"] = searchCurso?.Seccion;
                fila["Periodo"] = periodo;
                fila["ID"] = idUser;
                fila["Nombres"] = user?.Nombre ?? "";
                fila["Apellidos"] = user?.Apellido;
                fila["Email"] = user?.Email;
                int numeroAsistencias = searchAsistencia.Count(dato => dato.Dato == "P");
                int numeroFaltas = searchAsistencia.Count(dato => dato.Dato == "F");
                int numeroExcusas = searchAsistencia.Count(dato => dato.Dato == "E");
                fila["Asistencias"] = numeroAsistencias;
                fila["Faltas"] = numeroFaltas;
                fila["Ausencias notificadas"] = numeroExcusas;
                fila["Porcentaje"] = CalcularPorcentaje(numeroAsistencias, numeroFaltas, numeroExcusas);
                dt.Rows.Add(fila);
            }
            
            document.ImportDataTable(1, 1, dt, true);
            // Guardar el archivo Excel en el servidor
            string pathFile = AppDomain.CurrentDomain.BaseDirectory + "Prueba_Reporte_Excel.xlsx";
            document.SaveAs(pathFile);

            // Leer el contenido del archivo Excel como bytes
            byte[] fileBytes = System.IO.File.ReadAllBytes(pathFile);

            // Devolver el archivo Excel como respuesta HTTP
            // Establecer el tipo de contenido en la respuesta
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            // Nombre del archivo que se enviará al cliente
            string fileName = $"Reporte_{periodo}_docente_{idUser}.xlsx";
            // Devolver el archivo como un File Content Result
            return File(fileBytes, contentType, fileName);

        }
        

        [Authorize(Roles = "admin")]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetAssistancePerClass([FromQuery] int[] nrc, [FromQuery] string periodo) {

            SLDocument document = new SLDocument();
            System.Data.DataTable dt = new System.Data.DataTable();
            List<Horario> searchHorario = new List<Horario>();
            List<Matricula> matriculas = new List<Matricula>();
            List<Asistencium> asistenciaPerClass = new List<Asistencium>();
            List<Usuario> usuarios = new List<Usuario>();
            List<Curso> cursos = new List<Curso>();

            foreach (int item in nrc)
            {
                var horario = await _context.Horarios
                    .FirstOrDefaultAsync(h => h.Periodo == periodo && h.Nrc == item);

                if (horario != null)
                    searchHorario.Add(horario);
                
                var matriculados = await _context.Matriculas.Where(ma => ma.FkNrc == item && ma.Periodo == periodo).ToListAsync();
                if (matriculados != null)
                    matriculas.AddRange(matriculados);

                var SearchCursos = await _context.Cursos.FirstOrDefaultAsync(curs =>  curs.Periodo == periodo && curs.Nrc == item);
                if(SearchCursos != null)
                    cursos.Add(SearchCursos);
            }
            
            foreach (Horario item in searchHorario)
            {
                var asistencia = await _context.Asistencia.Where(a => a.IdHorario == item.Id).ToListAsync();
                if (asistencia != null)
                    asistenciaPerClass.AddRange(asistencia);

            }
            
            
            foreach (Matricula item in matriculas)
            {
                var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == item.UserId);
                if (user != null && !usuarios.Exists(u => u.Id == user.Id))
                {
                    usuarios.Add(user);
                }
            }
            dt.Columns.Add("Id", typeof(string));
            dt.Columns.Add("Nombres", typeof(string));
            dt.Columns.Add("Apellidos", typeof(string));
            dt.Columns.Add("rol", typeof(string));
            dt.Columns.Add("Email", typeof(string));
            dt.Columns.Add("Periodo", typeof(string));
            foreach(int item in nrc)
            {
                dt.Columns.Add($"Curso-{item}", typeof(string));
                dt.Columns.Add($"NRC-{item}", typeof(int));
                dt.Columns.Add($"Asistencias-{item}", typeof(int));
                dt.Columns.Add($"Faltas-{item}", typeof(int));
                dt.Columns.Add($"Ausencias notificadas-{item}", typeof(int));
                dt.Columns.Add($"Porcentaje-{item}", typeof(string));
            }
            foreach (Usuario user in usuarios)
            {
                DataRow fila = dt.NewRow();
                // Asignar los valores fijos a la fila
                fila["Id"] = user.Id;
                fila["Nombres"] = user.Nombre;
                fila["Apellidos"] = user.Apellido;
                fila["Rol"] = user.Rol;
                fila["Email"] = user.Email;
                fila["Periodo"] = periodo;
                foreach(Horario nrcActual in searchHorario) 
                {
                    // Obtener información adicional según el usuario y el NRC
                    int numAsistencias = asistenciaPerClass.Count(u => u.IdUser == user.Id && u.Dato == "P" && u.IdHorario == nrcActual.Id);
                    int faltas = asistenciaPerClass.Count(u => u.IdUser == user.Id && u.Dato == "F" && u.IdHorario == nrcActual.Id);
                    int ausencias = asistenciaPerClass.Count(u => u.IdUser == user.Id && u.Dato == "E" && u.IdHorario == nrcActual.Id);

                    // Asignar valores dinámicos a la fila
                    // Asignar valores dinámicos a la fila
                    fila[$"Curso-{nrcActual.Nrc}"] = cursos.FirstOrDefault(c => c.Nrc == nrcActual.Nrc)?.NombreCurso;
                    fila[$"NRC-{nrcActual.Nrc}"] = nrcActual.Nrc;
                    fila[$"Asistencias-{nrcActual.Nrc}"] = numAsistencias;
                    fila[$"Faltas-{nrcActual.Nrc}"] = faltas;
                    fila[$"Ausencias notificadas-{nrcActual.Nrc}"] = ausencias;
                    fila[$"Porcentaje-{nrcActual.Nrc}"] = CalcularPorcentaje(numAsistencias, faltas, ausencias);


                }
                // Agregar la fila a la DataTable
                dt.Rows.Add(fila);

            }
            document.ImportDataTable(1,1,dt,true);
            // Guardar el archivo Excel en el servidor
            string pathFile = AppDomain.CurrentDomain.BaseDirectory + "Prueba_Reporte_Excel.xlsx";
            document.SaveAs(pathFile);

            // Leer el contenido del archivo Excel como bytes
            byte[] fileBytes = System.IO.File.ReadAllBytes(pathFile);

            // Devolver el archivo Excel como respuesta HTTP
            // Establecer el tipo de contenido en la respuesta
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            // Nombre del archivo que se enviará al cliente
            string fileName = $"Reporte_{periodo}_materias.xlsx";
            // Devolver el archivo como un File Content Result
            return File(fileBytes, contentType, fileName);

        }
        // Función para calcular el porcentaje
        private string CalcularPorcentaje(int asistencias, int faltas, int ausencias)
        {
            // Manejo de división por cero
            if (asistencias + faltas + ausencias == 0)
            {
                return "0.00%";
            }

            // Manejo de números negativos
            if (asistencias < 0 || faltas < 0 || ausencias < 0)
            {
                throw new ArgumentException("Los valores de asistencias, faltas y ausencias no pueden ser negativos.");
            }

            double porcentaje = ((double)asistencias / (asistencias + faltas)) * 100;
            return porcentaje.ToString("0.00") + "%";
        }

    }
}
