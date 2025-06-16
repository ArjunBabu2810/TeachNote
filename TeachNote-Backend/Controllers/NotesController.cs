using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeachNote_Backend.Models;
using System.IO;
using Microsoft.Extensions.Logging.Console;

namespace TeachNote_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public NotesController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ───────────────────────────────────────────────
        // GET: api/notes     tested successfully
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Notes>>> GetAllNotes()
        {
            return await _context.Notes
                                 .Include(n => n.Subjects).ThenInclude(s=>s.Department)
                                 .ToListAsync();
        }

        // ───────────────────────────────────────────────
        // GET: api/notes/dept/5     tested successfully
        [HttpGet("dept/{id}")]
        public async Task<ActionResult<IEnumerable<Notes>>> GetNoteByDept(int id)
        {
            var note = await _context.Notes
                                     .Where(n => n.Subjects.departmentId == id)
                                     .Include(n => n.Subjects)
                                     .ToListAsync();
            
            if (note == null)
                return NotFound(new { message = "Notes not found for specified department." });

            return note;
        }

        // ───────────────────────────────────────────────
        // GET: api/notes/5     tested successfully
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Notes>> GetNote(int id)
        {
            var note = await _context.Notes
                                     .Include(n => n.Subjects)
                                     .FirstOrDefaultAsync(n => n.id == id);

            return note is null ? NotFound(new { message = "Notes not found for specified id." }) : note;
        }

        [HttpGet("department/{id:int}")]
        public async Task<ActionResult<IEnumerable<Notes>>> GetNoteByDepartment(int id)
        {
            Console.WriteLine($"Department wise note fetching:{id}");
            var note = await _context.Notes
                                     .Include(n => n.Subjects)
                                     .Where(n=>n.Subjects.departmentId==id)
                                     .ToListAsync();
            Console.WriteLine("Note id "+note[0].subjectId);
            return note is null ? NotFound(new { message = "Notes not found for specified department." }) : note;
        }


        [HttpGet("teacher/{id:int}")]
        public async Task<ActionResult<IEnumerable<Notes>>> GetNoteByTeacher(int id)
        {
            var note = await _context.Notes
                                     .Include(n => n.Subjects)
                                     .Where(n => n.userId == id)
                                     .ToListAsync();

            return note is null ? NotFound(new { message = "Notes not found for specified teacher." }) : note;
        }

        // ───────────────────────────────────────────────
        // POST: api/notes/upload          tested successfully
        // Receives a PDF via multipart/form-data
        [HttpPost("upload")]
        public async Task<IActionResult> UploadNote(
            [FromForm] IFormFile pdfFile,
            [FromForm] int subjectId,
            [FromForm] int userId,
            [FromForm] string description)
            
        {
            Console.WriteLine($"user: ------------------{userId}");
            Console.WriteLine($"{subjectId}");
            Console.WriteLine($"{description}");
            // 1️⃣ Validate file
            if (pdfFile is null || pdfFile.Length == 0 || Path.GetExtension(pdfFile.FileName).ToLower() != ".pdf")
                return BadRequest("Please upload a valid PDF file.");

            // 🔐 Subject existence check
            var subjectExists = await _context.Subjects.AnyAsync(s => s.id == subjectId);
            if (!subjectExists)
                return NotFound($"Subject with id {subjectId} not found.");

            // 2️⃣ Ensure uploads folder
            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsRoot = Path.Combine(webRoot, "uploads");
            Directory.CreateDirectory(uploadsRoot);      // Creates both if missing

            // var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads");
            // if (!Directory.Exists(uploadsRoot))
            //     Directory.CreateDirectory(uploadsRoot);

            // 3️⃣ Create unique filename
            var fileName = $"{Guid.NewGuid()}.pdf";
            var fullPath = Path.Combine(uploadsRoot, fileName);

            // 4️⃣ Save file to disk
            await using (var fs = new FileStream(fullPath, FileMode.Create))
            {
                await pdfFile.CopyToAsync(fs);
            }
            // 5️⃣ Persist note record
            var note = new Notes
            {
                postedDate = DateTime.UtcNow,
                userId = userId,
                subjectId = subjectId,
                description = description,
                pdfFile = fileName // ← only stores "abc123.pdf"
                // pdfFile = Path.Combine("uploads", fileName) // relative, e.g., uploads/abc.pdf
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetNote), new { id = note.id }, note);
        }

        // ───────────────────────────────────────────────
        // DELETE: api/notes/5       tested successfully
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note is null) return NotFound(new { message = "Notes not found with specified id." });

            // Optionally delete the physical file
            var physicalPath = Path.Combine(_env.WebRootPath, note.pdfFile);
            if (System.IO.File.Exists(physicalPath))
                System.IO.File.Delete(physicalPath);

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ───────────────────────────────────────────────
        // GET: api/notes/download/abc123.pdf       tested successfully
        [HttpGet("download/{fileName}")]
        public IActionResult Download(string fileName)
        {
            var filePath = Path.Combine(_env.WebRootPath, "uploads", fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound("File not found.");

            var mimeType = "application/pdf";
            return PhysicalFile(filePath, mimeType, fileName);
        }
        
        // ───────────────────────────────────────────────
        // GET: api/notes/view/abc123.pdf       tested successfully
        [HttpGet("view/{fileName}")]
        public IActionResult view(string fileName)
        {
            var filePath = Path.Combine(_env.WebRootPath, "uploads", fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound("File not found.");

            var mimeType = "application/pdf";

            // ✅ NEW: lets browser display PDF inline
            return PhysicalFile(filePath, mimeType);
        }

        // Filter by semester only	/api/notes/filter?semester=5
        // Filter by subject only	/api/notes/filter?subjectId=3
        // Filter by both	/api/notes/filter?semester=5&subjectId=3
        // No filters (get all)	/api/notes/filter

        // GET: api/notes/filter?semester=5&subjectId=3        tested successfully
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<Notes>>> FilterNotes([FromQuery] int? semester, [FromQuery] int? subjectId)
        {
            var query = _context.Notes
                .Include(n => n.Subjects)
                .AsQueryable();

            if (semester.HasValue)
                query = query.Where(n => n.Subjects.semester == semester.Value);

            if (subjectId.HasValue)
                query = query.Where(n => n.subjectId == subjectId.Value);

            var result = await query.ToListAsync();

            if (result == null)
                return NotFound(new { message = "Notes not found for specified filteration." });

            return Ok(result);
        }

    }
}
