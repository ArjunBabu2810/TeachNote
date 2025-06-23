using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TeachNote_Backend.Models;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "teacher,student")] // ✅ GLOBAL: only teacher & student can access this controller
public class MarksController : ControllerBase
{
    private readonly AppDbContext _context;

    public MarksController(AppDbContext context)
    {
        _context = context;
    }

    private string? GetUserEmail() => User.FindFirst(ClaimTypes.Email)?.Value;
    private string? GetUserRole() => User.FindFirst(ClaimTypes.Role)?.Value;

    // ✅ GET marks by studentId
    [HttpGet("student/{studentId}")]
    public async Task<ActionResult<object>> GetMarksByStudentId(int studentId)
    {
        var email = GetUserEmail();
        var role = GetUserRole();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role))
            return Unauthorized("Invalid token.");

        var student = await _context.Users.Include(u => u.Department).FirstOrDefaultAsync(u => u.id == studentId);
        if (student == null)
            return NotFound("Student not found.");

        if (role == "teacher")
        {
            var teacher = await _context.Users.FirstOrDefaultAsync(u => u.email == email);
            if (teacher == null || teacher.departmentId != student.departmentId)
                return Forbid("Teachers can only access marks of students in their department.");
        }
        else if (role == "student")
        {
            if (student.email != email)
                return Forbid("Students can only access their own marks.");
        }

        var marks = await _context.Marks
            .Include(m => m.Subjects)
            .Where(m => m.userId == studentId)
            .ToListAsync();

        if (!marks.Any())
            return NotFound("No marks found for this student.");

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
            })
        });
    }

    // ✅ GET marks by subjectId — teacher only
    [Authorize(Roles = "teacher")]
    [HttpGet("subject/{subjectId}")]
    public async Task<ActionResult<object>> GetMarksBySubjectId(int subjectId)
    {
        var email = GetUserEmail();
        var role = GetUserRole();

        if (string.IsNullOrEmpty(email) || role != "teacher")
            return Unauthorized("Invalid token or role.");

        var subject = await _context.Subjects.Include(s => s.Department).FirstOrDefaultAsync(s => s.id == subjectId);
        if (subject == null)
            return NotFound("Subject not found.");

        var teacher = await _context.Users.FirstOrDefaultAsync(u => u.email == email);
        if (teacher == null || teacher.departmentId != subject.departmentId)
            return Forbid("Teachers can only access subjects in their department.");

        var marks = await _context.Marks
            .Include(m => m.User)
            .Include(m => m.Subjects)
            .Where(m => m.subjectId == subjectId)
            .ToListAsync();

        if (!marks.Any())
            return NotFound("No marks found for this subject.");

        return Ok(new
        {
            SubjectId = subject.id,
            SubjectName = subject.name,
            Semester = subject.semester,
            DepartmentId = subject.departmentId,
            DepartmentName = subject.Department?.name,
            Students = marks.Select(m => new
            {
                StudentId = m.User.id,
                StudentName = m.User.name,
                StudentEmail = m.User.email,
                Internal1 = m.internal1,
                Internal2 = m.internal2,
                External = m.external,
                Total = m.internal1 + m.internal2 + m.external
            })
        });
    }

    // ✅ GET marks by student + semester — teacher & student
    [HttpGet("student/{studentId}/semester/{semesterNumber}")]
    public async Task<ActionResult<object>> GetMarksByStudentAndSemester(int studentId, int semesterNumber)
    {
        var email = GetUserEmail();
        var role = GetUserRole();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role))
            return Unauthorized("Invalid token.");

        var student = await _context.Users.Include(u => u.Department).FirstOrDefaultAsync(u => u.id == studentId);
        if (student == null)
            return NotFound("Student not found.");

        if (role == "teacher")
        {
            var teacher = await _context.Users.FirstOrDefaultAsync(u => u.email == email);
            if (teacher == null || teacher.departmentId != student.departmentId)
                return Forbid("Teachers can only access marks of students in their department.");
        }
        else if (role == "student")
        {
            if (student.email != email)
                return Forbid("Students can only access their own marks.");
        }

        var marks = await _context.Marks
            .Include(m => m.Subjects)
            .Where(m => m.userId == studentId && m.Subjects.semester == semesterNumber)
            .ToListAsync();

        if (!marks.Any())
            return NotFound("No marks found for this student in this semester.");

        float semesterTotal = marks.Sum(m => m.internal1 + m.internal2 + m.external);

        return Ok(new
        {
            Semester = semesterNumber,
            Subjects = marks.Select(m => new
            {
                Subject = m.Subjects,
                Mark = new
                {
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

    // ✅ POST: add/update mark — teacher only
    [Authorize(Roles = "teacher")]
    [HttpPost]
    public async Task<ActionResult<Marks>> PostMarks(Marks mark)
    {
        var email = GetUserEmail();
        var role = GetUserRole();

        if (string.IsNullOrEmpty(email) || role != "teacher")
            return Unauthorized("Invalid token or role.");

        var student = await _context.Users.FirstOrDefaultAsync(u => u.id == mark.userId);
        var subject = await _context.Subjects.FirstOrDefaultAsync(s => s.id == mark.subjectId);

        if (student == null || subject == null)
            return BadRequest("Invalid student or subject.");

        var teacher = await _context.Users.FirstOrDefaultAsync(u => u.email == email);
        if (teacher == null || teacher.departmentId != student.departmentId || teacher.departmentId != subject.departmentId)
            return Forbid("Teachers can only add marks for students and subjects in their department.");

        var existingMark = await _context.Marks
            .FirstOrDefaultAsync(m => m.userId == mark.userId && m.subjectId == mark.subjectId);

        if (existingMark != null)
        {
            existingMark.internal1 = mark.internal1;
            existingMark.internal2 = mark.internal2;
            existingMark.external = mark.external;

            await _context.SaveChangesAsync();
            return Ok(existingMark);
        }

        _context.Marks.Add(mark);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(PostMarks), new { id = mark.id }, mark);
    }

    // ✅ DELETE: Teacher only
[Authorize(Roles = "teacher")]
[HttpDelete("{id}")]
public async Task<ActionResult<Marks>> DeleteMarks(int id)
{
    var email = GetUserEmail();
    var role = GetUserRole();

    if (string.IsNullOrEmpty(email) || role != "teacher")
        return Unauthorized("Invalid token or role.");

    var mark = await _context.Marks
        .Include(m => m.User)
        .Include(m => m.Subjects)
        .FirstOrDefaultAsync(m => m.id == id);

    if (mark == null)
        return NotFound("Mark not found.");

    var teacher = await _context.Users.FirstOrDefaultAsync(u => u.email == email);

    if (teacher == null)
        return Unauthorized("Teacher not found.");

    // ✅ Check teacher's department matches both student and subject
    if (teacher.departmentId != mark.User.departmentId || teacher.departmentId != mark.Subjects.departmentId)
        return Forbid("Teachers can only delete marks for students and subjects in their department.");

    _context.Marks.Remove(mark);
    await _context.SaveChangesAsync();

    return Ok(mark);
}


    // ✅ No PUT or DELETE: not allowed for teacher/student
}
