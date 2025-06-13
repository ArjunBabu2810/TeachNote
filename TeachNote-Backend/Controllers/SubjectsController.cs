using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeachNote_Backend.Models;

namespace TeachNote_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubjectsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SubjectsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Subjects      tested succesfully
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Subjects>>> GetSubjects()
        {
            return await _context.Subjects.Include(s => s.Department).ToListAsync();
        }

        // GET: api/Subjects/5     tested succesfully
        [HttpGet("{id}")]
        public async Task<ActionResult<Subjects>> GetSubject(int id)
        {
            var subject = await _context.Subjects
                .Include(s => s.Department)
                .FirstOrDefaultAsync(s => s.id == id);

            if (subject == null)
                return NotFound();

            return subject;
        }

        [HttpGet("department/{id}")]
        public async Task<ActionResult<IEnumerable<Subjects>>> GetSubjectsByDepartment(int id)
        {
            Console.WriteLine($"Subject fetch by department id :{id}");
            var subjects = await _context.Subjects.Include(s => s.Department)
                                            .Where(s => s.departmentId == id)
                                            .ToListAsync();
            if (subjects == null)
            {
                return NoContent();
            }
            return subjects;
        }
        // POST: api/Subjects    tested succesfully
        [HttpPost]
        public async Task<ActionResult<Subjects>> PostSubject(Subjects subject)
        {
            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSubject), new { id = subject.id }, subject);
        }

        // PUT: api/Subjects/5       tested succesfully
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSubject(int id, Subjects updatedSubject)
        {
            if (id != updatedSubject.id)
                return BadRequest("ID mismatch");

            // _context.Entry(updatedSubject).State = EntityState.Modified;

            // try
            // {
            //     await _context.SaveChangesAsync();
            // }
            // catch (DbUpdateConcurrencyException)
            // {
            //     if (!SubjectExists(id))
            //         return NotFound();
            //     else
            //         throw;
            // }

            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null)
                return NotFound();

            // Update only fields that should change
            subject.name = updatedSubject.name;
            subject.semester = updatedSubject.semester;
            subject.departmentId = updatedSubject.departmentId;

            await _context.SaveChangesAsync();


            return NoContent();
        }

        // DELETE: api/Subjects/5       tested succesfully
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubject(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null)
                return NotFound();

            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // private bool SubjectExists(int id)
        // {
        //     return _context.Subjects.Any(e => e.id == id);
        // }
    }
}
