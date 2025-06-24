using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // Needed for ClaimTypes
using TeachNote_Backend.Models;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public DepartmentsController(AppDbContext context)
    {
        _context = context;
    }

    // ✅ FIXED: Use comma for multiple roles!
   [Authorize(Roles = "admin,teacher")]
[HttpGet]
public async Task<ActionResult<IEnumerable<Department>>> GetDepartments()
{
    var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
    var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role))
    {
        return Unauthorized(new { message = "Invalid token: email or role missing." });
    }

    if (role == "admin")
    {
        // Admin: get all
        return await _context.Departments.ToListAsync();
    }
    else if (role == "teacher")
    {
        // Teacher: get only their own department

        // Find this user in DB:
        var user = await _context.Users.FirstOrDefaultAsync(u => u.email == email);

        if (user == null)
            return Unauthorized(new { message = "User not found." });

        var dept = await _context.Departments
            .Where(d => d.id == user.departmentId)
            .ToListAsync();

        return dept;
    }
    else
    {
        return Forbid();
    }
}

    // ✅ Consider adding [Authorize(Roles = "admin")] for sensitive actions
    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<ActionResult<Department>> PostDepartment(Department department)
    {
        var exists = await _context.Departments
                               .FirstOrDefaultAsync(d => d.name == department.name);


        if (exists != null)
        {
            return BadRequest(new { message = "Department name already exists." });
        }

        _context.Departments.Add(department);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetDepartmentById), new { id = department.id }, department);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Department>> GetDepartmentById(int id)
    {
        var department = await _context.Departments.FindAsync(id);

        if (department == null)
        {
            return NotFound();
        }

        return department;
    }

    // ✅ Recommended: only admin can delete departments
    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult<Department>> DeleteDepartmentById(int id)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department == null)
            return NotFound();

        bool hasUsers = await _context.Users.AnyAsync(u => u.departmentId == id);
        bool hasSubjects = await _context.Subjects.AnyAsync(s => s.departmentId == id);

        if (hasUsers || hasSubjects)
            return BadRequest("Cannot delete department: it is referenced by existing users or subjects.");

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync();

        return department;
    }
}
