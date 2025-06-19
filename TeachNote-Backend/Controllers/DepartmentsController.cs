using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    // GET: api/departments
    [Authorize(Roles = "admin/teacher")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Department>>> GetDepartments()
    {
        return await _context.Departments.ToListAsync();
    }

    // GET: api/departments/5
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

    // POST: api/departments
    [HttpPost]
    public async Task<ActionResult<Department>> PostDepartment(Department department, int id)
    {

        var exist = await _context.Departments.FindAsync(id);

        if (exist != null)
        {
            return BadRequest(new { message = "Already exist departmet id" });
        }

        _context.Departments.Add(department);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetDepartmentById), new { id = department.id }, department);
    }

    // PUT: api/departments/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutDepartment(int id, Department department)
    {
        if (id != department.id)
        {
            return BadRequest("ID in URL does not match ID in body.");
        }

        _context.Entry(department).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Departments.Any(d => d.id == id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    // DELETE: api/departments/5
    [HttpDelete("{id}")]
    public async Task<ActionResult<Department>> DeleteDepartmentById(int id)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department == null)
            return NotFound();

        // Check if any users or subjects are still using this department
        bool hasUsers = await _context.Users.AnyAsync(u => u.departmentId == id);
        bool hasSubjects = await _context.Subjects.AnyAsync(s => s.departmentId == id);

        if (hasUsers || hasSubjects)
            return BadRequest("Cannot delete department. It is referenced by existing users or subjects.");

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync();

        return department;
    }

}
