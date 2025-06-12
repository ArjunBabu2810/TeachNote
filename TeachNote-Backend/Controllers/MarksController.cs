using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeachNote_Backend.Models;

[ApiController]
[Route("api/[controller]")]
public class MarksController : ControllerBase
{
    private readonly AppDbContext _context;

    public MarksController(AppDbContext context)
    {
        _context = context;
    }


    // GET: api/marks/student/5
    [HttpGet("student/{studentId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetMarksByStudentId(int studentId)
    {
        var marks = await _context.Marks
            .Include(m => m.Subjects)
            .Where(m => m.userId == studentId)
            .Select(m => new
            {
                SubjectName = m.Subjects.name,
                Internal1 = m.internal1,
                Internal2 = m.internal2,
                External = m.external,
                Total = m.internal1 + m.internal2 + m.external
            })
            .ToListAsync();

        if (!marks.Any())
            return NotFound("No marks found for this student.");

        return Ok(marks);
    }
// GET: api/marks/subject/3
[HttpGet("subject/{subjectId}")]
public async Task<ActionResult<IEnumerable<object>>> GetMarksBySubjectId(int subjectId)
{
    var marks = await _context.Marks
        .Include(m => m.User)
        .Where(m => m.subjectId == subjectId)
        .Select(m => new {
            StudentName = m.User.name,
            Internal1 = m.internal1,
            Internal2 = m.internal2,
            External = m.external,
            Total = m.internal1 + m.internal2 + m.external
        })
        .ToListAsync();

    if (!marks.Any())
        return NotFound("No marks found for this subject.");

    return Ok(marks);
}
// GET: api/marks/student/5/semester/2
// [HttpGet("student/{studentId}/semester/{semesterNumber}")]
// public async Task<ActionResult<object>> GetMarksByStudentAndSemester(int studentId, int semesterNumber)
// {
//     var marks = await _context.Marks
//         .Include(m => m.Subjects)
//         .Where(m => m.userId == studentId && m.Subjects.semester == semesterNumber) // 🔄 changed here
//         .Select(m => new {
//             SubjectName = m.Subjects.name,
//             Internal1 = m.internal1,
//             Internal2 = m.internal2,
//             External = m.external,
//             Total = m.internal1 + m.internal2 + m.external
//         })
//         .ToListAsync();

//     if (!marks.Any())
//         return NotFound("No marks found for this student in the given semester.");

//     float semesterTotal = marks.Sum(m => m.Total);

//     return Ok(new {
//         Semester = semesterNumber,
//         Subjects = marks,
//         SemesterTotal = semesterTotal
//     });
// }
[HttpGet("student/{studentId}/semester/{semesterNumber}")]
public async Task<ActionResult<object>> GetMarksByStudentAndSemester(int studentId, int semesterNumber)
{
    var marks = await _context.Marks
        .Include(m => m.Subjects)
        .Where(m => m.userId == studentId && m.Subjects.semester == semesterNumber)
        .ToListAsync();

    if (!marks.Any())
        return NotFound("No marks found for this student in the given semester.");

    float semesterTotal = marks.Sum(m => m.internal1 + m.internal2 + m.external);

    return Ok(new {
        Semester = semesterNumber,
        Subjects = marks.Select(m => new {
            Subject = m.Subjects,    // ✅ full subject object
            Mark = new {
                m.id,
                m.userId,
                m.subjectId,
                m.internal1,
                m.internal2,
                m.external,
                Total = m.internal1 + m.internal2 + m.external
            }
        }),
        SemesterTotal = semesterTotal
    });
}



    // POST: api/marks
    [HttpPost]
    public async Task<ActionResult<Marks>> PostMarks(Marks mark)
    {
        _context.Marks.Add(mark);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(PostMarks), new { id = mark.id }, mark);
    }

    // PUT: api/marks/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutMarks(int id, Marks mark)
    {
        if (id != mark.id)
        {
            return BadRequest("ID mismatch.");
        }

        _context.Entry(mark).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Marks.Any(m => m.id == id))
            {
                return NotFound("Mark not found.");
            }
            throw;
        }

        return NoContent();
    }

    // DELETE: api/marks/5
    [HttpDelete("{id}")]
    public async Task<ActionResult<Marks>> DeleteMarks(int id)
    {
        var mark = await _context.Marks.FindAsync(id);
        if (mark == null)
        {
            return NotFound("Mark not found.");
        }

        _context.Marks.Remove(mark);
        await _context.SaveChangesAsync();
        return mark;
    }
}
