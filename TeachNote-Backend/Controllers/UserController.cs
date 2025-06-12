using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using TeachNote_Backend.Models;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<User>>> GetUsers()
    {
        var users = await _context.Users.Include(u=>u.Department).ToListAsync();
        return users;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUserById(int id)
    {
        Console.WriteLine($"Fetching user by id {id}");
        var users = await _context.Users.Include(u=>u.Department).FirstOrDefaultAsync(u=>u.id==id);
        if (users == null)
        {
            return NoContent();
        }
        
        return users;
    }
    

    [HttpPost("add")]
    public  async Task<IActionResult> AddUser([FromBody] User user)
    {
        Console.WriteLine($"attempt to add user");
        Console.WriteLine(user);
        if (user == null)
        {
            return BadRequest(new { message = "Invalid user data" });
        }
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.password);
        // var dept = await _context.Departments.FindAsync(user.departmentId);
        var dept = await _context.Departments.AsNoTracking().FirstOrDefaultAsync(d => d.id == user.departmentId);
        if (dept == null)
        {
            return NotFound(new { message = "Department not found!" });
        }
        Console.WriteLine($"attempt to add user department={dept.id}");
        var newUser = new User
        {
            name = user.name,
            email = user.email,
            password = hashedPassword,
            role = user.role,
            departmentId = user.departmentId,
        };
        _context.Users.Add(newUser);
        await _context.SaveChangesAsync(); 
        return Ok(new { message = "New user added succesfully " });
    }


    [HttpGet("bydepartment")]
    public async Task<ActionResult<List<User>>> GetUsersByDeparment(int id)
    {
        var users = await _context.Users.Include(u => u.Department).Where(u => u.departmentId == id).ToListAsync();
        return Ok(users);
    }

    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateUser(int id, User updatedUser)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound(new {message = "user not found"});
        }
        try
        {
            user.name = updatedUser.name;
            user.email = updatedUser.email;
            user.role = updatedUser.role;
            user.departmentId = updatedUser.departmentId;
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, "Error in updating user");
        }
        return Ok(new { message = "user updated" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "User Not Found" });
        }
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return Ok(new { messsage = "User Deleted Successfully" });
    }

    [HttpPost("updatePassword/{id}")]
    public async Task<IActionResult> UpdatePassword(int id, User updatedUser)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound(new {message = "user not found"});
        }
        try
        {
            if (!BCrypt.Net.BCrypt.Verify(updatedUser.password, user.password))
            {
                return BadRequest(new { message = "Enter Previous password correctly" });
            }
            user.password = BCrypt.Net.BCrypt.HashPassword(updatedUser.password);
            
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, "Error in updating user");
        }
        return Ok(new { message = "user updated" });
    }
    [HttpGet("test")]
    public IActionResult Test()
    {
        Console.WriteLine("Call received");
        return Ok(new { Message = "Auth controller is working!" });
    }
}
