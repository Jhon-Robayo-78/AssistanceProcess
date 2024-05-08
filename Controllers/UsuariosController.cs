using AsistenciaProcess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web.Resource;

//using System.IdentityModel.Tokens.Jwt;

namespace AsistenciaProcess.Controllers
{
    [Authorize]
    [RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly AssistanceProcessesContext _assistanceProcessesContext;

        public UsuariosController(AssistanceProcessesContext assistanceProcessesContext)
        {
            _assistanceProcessesContext = assistanceProcessesContext;
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsers()
        {
            if (_assistanceProcessesContext == null)
            {
                return NotFound();
            }
            return await _assistanceProcessesContext.Usuarios.ToListAsync();
        }
        // GET: api/Students/5
        [HttpGet("{entraId}")]
        public async Task<ActionResult<Usuario>> GetUser(string entraId)
        {
            if (_assistanceProcessesContext.Usuarios == null)
            {
                return NotFound();
            }
            var user = await _assistanceProcessesContext.Usuarios.FirstOrDefaultAsync(u => u.EntraId == entraId);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        [Authorize(Roles = "admin,estudiante")]
        // PUT: api/Students/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{entraId}")]
        public async Task<IActionResult> PutUser(string entraId, [FromBody] Usuario user)
        {
            if (entraId != user.EntraId)
            {
                return BadRequest();
            }

            // Buscar el usuario por el campo EntraId
            var existingUser = await _assistanceProcessesContext.Usuarios.FirstOrDefaultAsync(u => u.EntraId == entraId);

            if (existingUser == null)
            {
                return NotFound();
            }

            

            try
            {
                // Elimina la entidad existente
                _assistanceProcessesContext.Usuarios.Remove(existingUser);
                await _assistanceProcessesContext.SaveChangesAsync();
                Usuario newUser = new Usuario();
                // Actualizar las propiedades del usuario existente
                newUser.Id = user.Id;
                newUser.Nombre = user.Nombre;
                newUser.Apellido = user.Apellido;
                newUser.Rol = user.Rol;
                newUser.Email = user.Email;
                newUser.EntraId = entraId;
                // Agrega y guarda los cambios
                _assistanceProcessesContext.Usuarios.Add(newUser);
                await _assistanceProcessesContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(entraId))
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

        /*[Authorize(Roles = "admin")]
        // POST: api/Students
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUser([FromBody] Usuario user)
        {
            

            if (_assistanceProcessesContext.Usuarios == null)
            {
                return Problem("Entity set 'AssistanceProcessesDBContext.Students'  is null.");
            }
            var searchUser = (from userSearch in _assistanceProcessesContext.Usuarios
                              where userSearch.Id == user.Id
                              select user).FirstOrDefault();
            if (searchUser == null)
            {
                _assistanceProcessesContext.Usuarios.Add(user);
                await _assistanceProcessesContext.SaveChangesAsync();

                return CreatedAtAction("GetUser", new { idUser = user.Id }, user);
            }
            else
            {
                return Ok("el usuario existe");
            }

        }*/
        [Authorize(Roles = "admin")]
        // DELETE: api/Students/5
        [HttpDelete]
        public async Task<IActionResult> DeleteStudent([FromQuery] string id)
        {
            if (_assistanceProcessesContext.Usuarios == null)
            {
                return NotFound();
            }

            var user = await _assistanceProcessesContext.Usuarios.FirstOrDefaultAsync(u => u.Id == id);

            var asistenciaExists = await _assistanceProcessesContext.Asistencia.AnyAsync(asistencia => asistencia.IdUser == id);
            var matriculaExists = await _assistanceProcessesContext.Matriculas.AnyAsync(matricula => matricula.UserId == id);

            if (user == null)
            {
                return NotFound();
            }
            else if (asistenciaExists || matriculaExists)
            {
                return Conflict($"El Ususario con {id} esta asociado a Asistencias y Matriculas");
            }

            _assistanceProcessesContext.Usuarios.Remove(user);
            await _assistanceProcessesContext.SaveChangesAsync();

            return NoContent();
        }

        private bool StudentExists(string id)
        {
            return (_assistanceProcessesContext.Usuarios?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
