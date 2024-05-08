using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AsistenciaProcess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web.Resource;
using DocumentFormat.OpenXml.Vml;

namespace AsistenciaProcess.Controllers
{
    [Authorize]
    [RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
    [Route("api/[controller]")]
    [ApiController]
    public class AsistenciasController : ControllerBase
    {
        private readonly AssistanceProcessesContext _context;

        public AsistenciasController(AssistanceProcessesContext context)
        {
            _context = context;
        }

        // GET: api/Asistencias
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Asistencium>>> GetAsistencia()
        {
            return await _context.Asistencia.ToListAsync();
        }
        //[Authorize(Roles = "estudiante, profesor")]
        // GET: api/Asistencias/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Asistencium>> GetAsistencium(int id)
        {
            var asistencium = await _context.Asistencia.FindAsync(id);

            if (asistencium == null)
            {
                return NotFound();
            }

            return asistencium;
        }
        
        [HttpGet("[action]")]
        public async Task<IActionResult> GetAsistenciumPerCurso([FromQuery] int NRC, [FromQuery] string periodo)
        {
            /*var asistencium = await _context.Asistencia.ToListAsync();
            var horarios = await _context.Horarios.ToListAsync();
            var searchHorariosIds = (from horario in horarios where horario.Nrc == NRC select horario);
            var searchAsistencium = (from asistencia in searchHorariosIds where asistencia.Id == searchHorariosIds);*/
            var horarios = await _context.Horarios.Where(h => h.Nrc == NRC && h.Periodo == periodo).Select(h => h.Id).ToListAsync();

            var asistencium = await _context.Asistencia.Where(a => horarios.Contains(a.IdHorario)).ToListAsync();
            if (asistencium == null)
            {
                return NotFound();
            }
            List<object> listado = new List<object>();
            foreach(Asistencium asis in asistencium)
            {
                var user = await _context.Usuarios.Where(asisU => asisU.Id == asis.IdUser).FirstOrDefaultAsync();
                if(user != null)
                {
                    var objAsis = new
                    {
                        asis.Id,
                        asis.IdUser,
                        user.Nombre,
                        user.Apellido,
                        asis.IdHorario,
                        asis.Dato,
                        asis.Fecha,
                        horario = await _context.Horarios.Where(ho => ho.Id == asis.IdHorario).FirstOrDefaultAsync()
                    };
                    listado.Add(objAsis);
                }
                    
            }

            return Ok(listado);
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> GetAssistanceList([FromQuery]int NRC, [FromQuery] string periodo)
        {
            /*var asistencium = await _context.Asistencia.ToListAsync();
            var horarios = await _context.Horarios.ToListAsync();
            var searchHorariosIds = (from horario in horarios where horario.Nrc == NRC select horario);
            var searchAsistencium = (from asistencia in searchHorariosIds where asistencia.Id == searchHorariosIds);*/
            List<object> assistanceList = new List<object>();
            var horarios = await _context.Horarios.Where(h => h.Nrc == NRC && h.Periodo==periodo).Select(h => h.Id).ToListAsync();

            
            var searchUser = await _context.Matriculas.Where(ma => ma.Periodo == periodo && ma.FkNrc == NRC).Select(u=>u.UserId).ToListAsync();
            if (searchUser == null)
            {
                return NotFound();
            }
            for (int i = 0; i < searchUser.Count; i++) 
            {
                string idUser = searchUser[i] ?? "";
                //Console.WriteLine(idUser);
                var cursoName = await _context.Cursos.Where(cu => cu.Nrc==NRC && cu.Periodo == periodo).Select(cu => cu.NombreCurso).FirstOrDefaultAsync();
                if (cursoName != null)
                {
                    var asistencium = await _context.Asistencia.Where(a => horarios.Contains(a.IdHorario) && a.IdUser == idUser).ToListAsync();
                    int asistencias = asistencium.Count(a => a.Dato == "P");
                    int faltas = asistencium.Count(a => a.Dato == "F");
                    int ausencias = asistencium.Count(a => a.Dato == "E");
                    var assistanceObject = new
                    {
                        id_User = idUser,
                        NombreDeLaAsignatura = cursoName,
                        name = await _context.Usuarios.Where(u => u.Id == idUser).Select(ud => ud.Nombre).FirstOrDefaultAsync(),
                        lastname = await _context.Usuarios.Where(u => u.Id == idUser).Select(ud => ud.Apellido).FirstOrDefaultAsync(),
                        NRC,
                        Asistencias = asistencias,
                        Faltas = faltas,
                        Ausencias = ausencias,
                        horario = await _context.Horarios.Where(ho => ho.Nrc == NRC && ho.Periodo == periodo).ToListAsync()
                    };

                    assistanceList.Add(assistanceObject);
                }
                
            }
            

            return Ok(assistanceList);
        }

        // PUT: api/Asistencias/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsistencium(int id, Asistencium asistencium)
        {
            if (id != asistencium.Id)
            {
                return BadRequest();
            }

            _context.Entry(asistencium).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AsistenciumExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: https://localhost:7039/api/Asistencias?fecha=2024-02-21&curso=1295&periodo=202410&idHorario=1
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Asistencium>> PostAsistencium([FromQuery] int idHorario, [FromQuery] DateTime fecha, Asistencium asistencium)
        {
            asistencium.IdHorario = idHorario;
            asistencium.Fecha = fecha;
            _context.Asistencia.Add(asistencium);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAsistencium", new { id = asistencium.Id }, asistencium);
        }

        // DELETE: api/Asistencias/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsistencium(int id)
        {
            var asistencium = await _context.Asistencia.FindAsync(id);
            if (asistencium == null)
            {
                return NotFound();
            }

            _context.Asistencia.Remove(asistencium);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AsistenciumExists(int id)
        {
            return _context.Asistencia.Any(e => e.Id == id);
        }
    }
}
