using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeachNote_Backend.Models;

namespace TeachNote_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/notes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Notes>>> GetAllNotes()
        {
            return await _context.Notes
                .Include(n => n.Subjects)
                .ToListAsync();
        }

        // GET: api/notes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Notes>> GetNote(int id)
        {
            var note = await _context.Notes
                .Include(n => n.Subjects)
                .FirstOrDefaultAsync(n => n.id == id);

            if (note == null)
                return NotFound();

            return note;
        }

        // POST: api/notes
        [HttpPost]
        public async Task<ActionResult<Notes>> PostNote(Notes note)
        {
            note.postedDate = DateTime.UtcNow;

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetNote), new { id = note.id }, note);
        }

        // PUT: api/notes/5
        // [HttpPut("{id}")]
        // public async Task<IActionResult> PutNote(int id, Notes updatedNote)
        // {
        //     if (id != updatedNote.id)
        //         return BadRequest("Note ID mismatch.");

        //     var existingNote = await _context.Notes.FindAsync(id);
        //     if (existingNote == null)
        //         return NotFound();

        //     existingNote.subjectId = updatedNote.subjectId;
        //     existingNote.description = updatedNote.description;
        //     existingNote.pdfFile = updatedNote.pdfFile;

        //     await _context.SaveChangesAsync();
        //     return NoContent();
        // }

        // DELETE: api/notes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note == null)
                return NotFound();

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
