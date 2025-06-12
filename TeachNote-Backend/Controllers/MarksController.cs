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
    public async Task<ActionResult<object>> GetMarksByStudentId(int studentId)
    {
        var marks = await _context.Marks
            .Include(m => m.Subjects)
            .Include(m => m.User)
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

// .......sample outpuut......
//    {
//   "StudentId": 5,
//   "StudentName": "John Doe",
//   "StudentEmail": "john@example.com",
//   "DepartmentId": 2,
//   "Marks": [
//     {
//       "SubjectId": 1,
//       "SubjectName": "Math",
//       "Semester": 1,
//       "Internal1": 12,
//       "Internal2": 13,
//       "External": 35,
//       "Total": 60
//     },
//     ...
//   ]
// }



    // GET: api/marks/subject/3
    [HttpGet("subject/{subjectId}")]
    public async Task<ActionResult<object>> GetMarksBySubjectId(int subjectId)
    {
        var marks = await _context.Marks
            .Include(m => m.User)
            .Include(m => m.Subjects)
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
// .............sample output............
//{
//   "SubjectId": 3,
//   "SubjectName": "Database Systems",
//   "Semester": 4,
//   "Students": [
//     {
//       "StudentId": 10,
//       "StudentName": "Alice Johnson",
//       "StudentEmail": "alice@example.com",
//       "Internal1": 13,
//       "Internal2": 15,
//       "External": 40,
//       "Total": 68
//     },
//     ...
//   ]
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
