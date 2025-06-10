using Microsoft.AspNetCore.Mvc;
using TeachNote_Backend.Models;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly AppDbContext _context;
    public DepartmentsController(AppDbContext context) {
        _context = context;
        } 

    [HttpGet("")]
    public ActionResult<IEnumerable<Department>> GetDepartments()
    {
        return new List<Department> { };
    }

    [HttpGet("{id}")]
    public ActionResult<Department> GetDepartmentById(int id)
    {
        return null;
    }

    [HttpPost("")]
    public ActionResult<Department> PostTDepartment(Department department)
    {
        return null;
    }

    [HttpPut("{id}")]
    public IActionResult PutDepartment(int id, Department department)
    {
        
        return NoContent();
    }

    [HttpDelete("{id}")]
    public ActionResult<Department> DeleteDepartmentById(int id)
    {
        return null;
    }
}
