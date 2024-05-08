using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AsistenciaProcess.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web.Resource;

namespace AsistenciaProcess.Controllers
{
    [Authorize]
    [RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
    [Route("api/[controller]")]
    [ApiController]
    public class CursosController : ControllerBase
    {
        private readonly AssistanceProcessesContext _context;

        public CursosController(AssistanceProcessesContext context)
        {
            _context = context;
        }

        // GET: api/Cursos?periodo=202020
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Curso>>> GetCursos([FromQuery] string periodo)
        {
            var cursos = await _context.Cursos.ToListAsync();
            var searchCurso = await Task.Run(() => 
                (from curso in cursos where curso.Periodo == periodo select curso).ToList()
            );
            return searchCurso;
        }

        // GET: api/Cursos/5?perido=202020
        [HttpGet("{nrc}")]
        public async Task<ActionResult<Curso>> GetCurso(int nrc, [FromQuery] string periodo)
        {
            var curso = await _context.Cursos.FindAsync(nrc, periodo);

            if (curso == null)
            {
                return NotFound();
            }

            return curso;
        }

        // PUT: api/Cursos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCurso(int nrc, Curso curso)
        {
            if (nrc != curso.Nrc)
            {
                return BadRequest();
            }

            _context.Entry(curso).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CursoExists(nrc, curso.Periodo))
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

        // POST: api/Cursos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Curso>> PostCurso(Curso curso)
        {
            _context.Cursos.Add(curso);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (CursoExists(curso.Nrc, curso.Periodo))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetCurso", new { id = curso.Nrc }, curso);
        }

        // DELETE: api/Cursos?nrc=1295&periodo=202410
        [HttpDelete]
        public async Task<IActionResult> DeleteCurso([FromQuery] int nrc, [FromQuery] string periodo)
        {
            var curso = await _context.Cursos.FindAsync(nrc,periodo);
            var horarios = await _context.Horarios.ToListAsync();
            var horarioWhere = (from horario in horarios where horario.Nrc == nrc && horario.Periodo == periodo select horario);

            if (curso == null)
            {
                return NotFound();
            }
            else if (horarioWhere != null) 
            {
                return Conflict("Existen Horarios Asociados");
            }

            _context.Cursos.Remove(curso);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CursoExists(int id, string periodo)
        {
            return _context.Cursos.Any(e => e.Nrc == id && e.Periodo == periodo);
        }
    }
}
