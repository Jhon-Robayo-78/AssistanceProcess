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

namespace AsistenciaProcess.Controllers
{
    [Authorize]
    [RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
    [Route("api/[controller]")]
    [ApiController]
    public class MatriculasController : ControllerBase
    {
        private readonly AssistanceProcessesContext _context;

        public MatriculasController(AssistanceProcessesContext context)
        {
            _context = context;
        }


        [Authorize(Roles = "admin")]
        // GET: api/Matriculas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Matricula>>> GetMatriculas([FromQuery] string periodo)
        {
            var searchMatriculas = await _context.Matriculas.ToListAsync();
            var matriculasInPeriodo = await Task.Run(() => (from matriculas in searchMatriculas where matriculas.Periodo == periodo select matriculas).ToList());

            return matriculasInPeriodo;
        }


        // GET: api/Matriculas/GetMatricula
        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<Matricula>>> GetMatricula([FromQuery] string periodo, [FromQuery] string id)
        {
            var matriculas = await _context.Matriculas.ToListAsync();
            var searchMatricula = await Task.Run(
                () => (from matricula in matriculas where matricula.Periodo == periodo && matricula.UserId == id select matricula).ToList()
            );



            return searchMatricula;
        }

        // PUT: api/Matriculas/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMatricula(int id, Matricula matricula)
        {
            if (id != matricula.Id)
            {
                return BadRequest();
            }

            _context.Entry(matricula).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MatriculaExists(id))
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
        [HttpGet("[action]")]
        public async Task<IActionResult> HorarioFromMatricula([FromQuery] string periodo, [FromQuery] string idUser) 
        {
            var searchHorarioFromMa = await _context.Matriculas.Where(Ma => Ma.UserId == idUser && Ma.Periodo == periodo).Select(ma=>ma.FkNrc).ToListAsync();
            List<object> result = new List<object>();
            foreach(int NRC in searchHorarioFromMa) 
            {
                var searchHorario = await _context.Horarios.Where(ho => ho.Nrc == NRC && ho.Periodo == periodo).ToListAsync();
                if(searchHorario != null)
                    result.AddRange(searchHorario);
            }
            return Ok(result);
        }
        // POST: api/Matriculas
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Matricula>> PostMatricula(Matricula matricula)
        {
            _context.Matriculas.Add(matricula);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMatricula", new { id = matricula.Id }, matricula);
        }
        [Authorize(Roles = "admin,estudiante")]
        // DELETE: api/Matriculas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMatricula(int id)
        {
            var matricula = await _context.Matriculas.FindAsync(id);
            if (matricula == null)
            {
                return NotFound();
            }

            _context.Matriculas.Remove(matricula);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MatriculaExists(int id)
        {
            return _context.Matriculas.Any(e => e.Id == id);
        }
    }
}
