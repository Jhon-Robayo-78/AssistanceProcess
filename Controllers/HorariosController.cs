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
    public class HorariosController : ControllerBase
    {
        private readonly AssistanceProcessesContext _context;

        public HorariosController(AssistanceProcessesContext context)
        {
            _context = context;
        }

        // GET: api/Horarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Horario>>> GetHorarios([FromQuery] string periodo)
        {
            var horarios = await _context.Horarios.ToListAsync();
            var searchHorario = (from horario in horarios where horario.Periodo == periodo select horario);
            return searchHorario.ToList();
        }

        // GET: api/Horarios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Horario>> GetHorario(int id)
        {
            var horario = await _context.Horarios.FindAsync(id);

            if (horario == null)
            {
                return NotFound();
            }

            return horario;
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<Horario>>> GetHorariosForMateria([FromQuery]string periodo, [FromQuery]int nrc)
        {
            var horarios = await _context.Horarios.ToListAsync();
            var searchHorarios = (from horario in horarios where horario.Periodo == periodo && horario.Nrc == nrc select horario);

            if (searchHorarios == null)
            {
                return NotFound();
            } 
            return searchHorarios.ToList();
        }

        [Authorize(Roles ="admin")]
        // PUT: api/Horarios/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHorario(int id, Horario horario)
        {
            if (id != horario.Id)
            {
                return BadRequest();
            }

            _context.Entry(horario).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HorarioExists(id))
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

        [Authorize(Roles = "admin")]
        // POST: api/Horarios
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Horario>> PostHorario(Horario horario)
        {
            if (horario.Edf == "NA" && horario.Salon == "--")
            {
                var conflictedHorario = await _context.Horarios.FirstOrDefaultAsync(x =>
                   x.Periodo == horario.Periodo &&
                   x.HoraInicio == horario.HoraInicio &&
                   x.HoraFin == horario.HoraFin  &&
                   x.DayWeek == horario.DayWeek &&
                   x.Nrc == horario.Nrc
                   
                );
                if (conflictedHorario != null)
                {
                    if(horario.Nrc == conflictedHorario.Nrc)
                    {
                        return Conflict($"ya esta registrado es horario con nrc: {horario.Nrc}");
                    }
                    // Si hay conflicto, retornar un mensaje de conflicto indicando el conflicto de horario
                    return Conflict($"El salón {horario.Edf}-{horario.Salon} ya está ocupado en el periodo de tiempo especificado.");
                }

                // Si no hay conflicto, agregar el nuevo horario y guardar los cambios
                _context.Horarios.Add(horario);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetHorario", new { id = horario.Id }, horario);
            }
            else
            {
                // Verificar si hay algún horario existente que tenga un conflicto de horario con el nuevo horario
                var conflictedHorario = await _context.Horarios.FirstOrDefaultAsync(x =>
                   x.Edf == horario.Edf &&
                   x.Salon == horario.Salon &&
                   x.Periodo == horario.Periodo &&
                   ((horario.HoraInicio >= x.HoraInicio && horario.HoraInicio < x.HoraFin) ||
                   (horario.HoraFin > x.HoraInicio && horario.HoraFin <= x.HoraFin) ||
                   (horario.HoraInicio <= x.HoraInicio && horario.HoraFin >= x.HoraFin)) &&
                   x.DayWeek == horario.DayWeek
                   
               );
                if (conflictedHorario != null)
                {
                    if (horario.Nrc == conflictedHorario.Nrc)
                    {
                        return Conflict($"ya esta registrado es horario con nrc: {horario.Nrc}");
                    }
                    // Si hay conflicto, retornar un mensaje de conflicto indicando el conflicto de horario
                    return Conflict($"El salón {horario.Edf}-{horario.Salon} ya está ocupado en el periodo de tiempo especificado.");
                }

                // Si no hay conflicto, agregar el nuevo horario y guardar los cambios
                _context.Horarios.Add(horario);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetHorario", new { id = horario.Id }, horario);
            }      

        }

        [Authorize(Roles = "admin")]
        // DELETE: api/Horarios/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHorario(int id)
        {
            var horario = await _context.Horarios.FindAsync(id);
            if (horario == null)
            {
                return NotFound();
            }

            _context.Horarios.Remove(horario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool HorarioExists(int id)
        {
            return _context.Horarios.Any(e => e.Id == id);
        }
    }
}
