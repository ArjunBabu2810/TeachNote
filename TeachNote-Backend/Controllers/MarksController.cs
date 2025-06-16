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

// GET: api/marks/studentid/3
    [HttpGet("student/{studentId}")]
    public async Task<ActionResult<object>> GetMarksByStudentId(int studentId)
    {
        var marks = await _context.Marks
            .Include(m => m.Subjects)
            .Include(m => m.User)
                .ThenInclude(u => u.Department) // Include department here
            .Where(m => m.userId == studentId)
            .ToListAsync();

        if (!marks.Any())
            return NotFound("No marks found for this student.");

        var student = marks.First().User;

        return Ok(new
        {
            StudentId = student.id,
            StudentName = student.name,
            StudentEmail = student.email,
            DepartmentId = student.departmentId,
            DepartmentName = student.Department?.name, 
            Marks = marks.Select(m => new
            {
                SubjectId = m.subjectId,
                SubjectName = m.Subjects.name,
                Semester = m.Subjects.semester,
                Internal1 = m.internal1,
                Internal2 = m.internal2,
                External = m.external,
                Total = m.internal1 + m.internal2 + m.external
            }).ToList()
        });
    }
    
    [HttpGet("department/{departmentId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetMarksByDepartmentId(int departmentId)
    {
        Console.WriteLine("Marks fetching by department id");
        var marks = await _context.Marks
            .Include(m => m.Subjects)
            .Include(m => m.User)
                .ThenInclude(u => u.Department) // Include department here
            .Where(m => m.User.departmentId == departmentId)
            .ToListAsync();

        if (!marks.Any())
            return NotFound("No marks found for this department.");

        var student = marks.First().User;
        return marks;
    }

    // GET: api/marks/subject/3
    [HttpGet("subject/{subjectId}")]
public async Task<ActionResult<object>> GetMarksBySubjectId(int subjectId)
{
    var marks = await _context.Marks
        .Include(m => m.User)
        .Include(m => m.Subjects)
            .ThenInclude(s => s.Department) // Include Department
        .Where(m => m.subjectId == subjectId)
        .ToListAsync();

    if (!marks.Any())
        return NotFound("No marks found for this subject.");

    var subjectInfo = marks.First().Subjects;

    var result = new
    {
        SubjectId = subjectId,
        SubjectName = subjectInfo.name,
        Semester = subjectInfo.semester,
        DepartmentId = subjectInfo.departmentId,
        DepartmentName = subjectInfo.Department?.name,
        Students = marks.Select(m => new
        {
            StudentId = m.User.id,
            StudentName = m.User.name,
            StudentEmail = m.User.email,
            Internal1 = m.internal1,
            Internal2 = m.internal2,
            External = m.external,
            Total = m.internal1 + m.internal2 + m.external
        }).ToList()
    };

    return Ok(result);
}

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
    // [HttpPost]
    // public async Task<ActionResult<Marks>> PostMarks(Marks mark)
    // {
    //     _context.Marks.Add(mark);
    //     await _context.SaveChangesAsync();
    //     return CreatedAtAction(nameof(PostMarks), new { id = mark.id }, mark);
    // }
    public async Task<ActionResult<Marks>> PostMarks(Marks mark)
    {
        // Check if mark already exists for the given userId and subjectId
        var existingMark = await _context.Marks
            .FirstOrDefaultAsync(m => m.userId == mark.userId && m.subjectId == mark.subjectId);


        if (existingMark != null)
        {
            // Update existing values
            existingMark.internal1 = mark.internal1;
            existingMark.internal2 = mark.internal2;
            existingMark.external = mark.external;

            await _context.SaveChangesAsync();
            return Ok(existingMark); // return updated mark
        }
        var student = await _context.Users.FirstOrDefaultAsync(u => u.id == mark.userId);
        var subject = await _context.Subjects.FirstOrDefaultAsync(s => s.id == mark.subjectId);

        if (student==null || student.departmentId != subject.departmentId)
        {
            return BadRequest(new { message = "This student is not from your department" });
        }
        // If not found, insert new mark
        _context.Marks.Add(mark);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(PostMarks), new { id = mark.id }, mark); // return newly created mark
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
