using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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

    [Authorize(Roles = "admin")]
    [HttpGet("all")]
    public async Task<ActionResult<List<User>>> GetUsers()
    {
        var users = await _context.Users.Include(u => u.Department).ToListAsync();
        if (users == null)
        {
            return BadRequest(new { message = "There are no users" });
        }
        return Ok(users);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUserById(int id)
    {
        if (string.IsNullOrWhiteSpace(id.ToString()))
        {
            return BadRequest(new { message = "Id cannot be null" });
        }
        Console.WriteLine($"Fetching user by id {id}");
        var users = await _context.Users.Include(u => u.Department).FirstOrDefaultAsync(u => u.id == id);
        if (users == null)
        {
            return NoContent();
        }

        return Ok(users);
    }

    [Authorize(Roles = "admin,teacher,student")]
    [HttpGet("profile")]
    public async Task<ActionResult<User>> GetProfile()
    {               
        var email = User.FindFirstValue(ClaimTypes.Email);
        Console.WriteLine($"User Email : {User.FindFirstValue(ClaimTypes.Email)}");
        var user = await _context.Users.Include(u=>u.Department).FirstOrDefaultAsync(u => u.email == email);

        if (user == null)
        {
            return NotFound(new { message = "User Not Found" });
        }

        return Ok(user);
        // foreach (var item in User.Claims)
        // {
        //     Console.WriteLine($"Claim type : {item.Type} value : {item.Value}");
        // }
    }

    [Authorize(Roles = "admin")]
    [HttpPost("add")]
    public async Task<IActionResult> AddUser([FromBody] User user)
    {
        Console.WriteLine($"attempt to add user");
        Console.WriteLine(user);
        if (user == null)
        {
            return BadRequest(new { message = "Invalid user data" });
        }
        var existing = await _context.Users.FirstOrDefaultAsync(u => u.email == user.email);
        if (existing != null)
        {
            Console.WriteLine($"Adding user : {existing.id} given id : {user.id}");
            return BadRequest("user already exist");
        }
        // var dept = await _context.Departments.FindAsync(user.departmentId);
        var dept = await _context.Departments.FirstOrDefaultAsync(d => d.id == user.departmentId);
        if (dept == null)
        {
            return NotFound(new { message = "Department not found!" });
        }
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.password);
        Console.WriteLine($"attempt to add user department={dept.id}");
        var newUser = new User
        {
            name = user.name,
            email = user.email,
            password = hashedPassword,
            role = user.role,
            departmentId = user.departmentId,
        };
        try
        {
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return Ok(new { message = "New user added succesfully " });
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error in inserting user : {e.Message}");
            return StatusCode(500);

        }
    }

    [Authorize(Roles = "admin,teacher")]
    [HttpGet("bydepartment")]
    public async Task<ActionResult<List<User>>> GetUsersByDeparment(int id)
    {
        var department = await _context.Departments.FirstOrDefaultAsync(d => d.id == id);
        if (department == null)
        {
            return BadRequest(new { message = $"There no department with id : {id}" });
        }
        var users = await _context.Users.Include(u => u.Department).Where(u => u.departmentId == id).ToListAsync();
        if (users == null)
        {
            return NotFound(new { message = "Found no users" });
        }
        return Ok(users);
    }


    [Authorize(Roles = "admin")]
    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateUser(int id, User updatedUser)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound(new { message = "user not found" });
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

    [Authorize(Roles = "admin")]
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


    [Authorize]
    [HttpPost("updatePassword/{id}")]
    public async Task<IActionResult> UpdatePassword(int id, User updatedUser)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound(new { message = "user not found" });
        }
        try
        {
            if (!BCrypt.Net.BCrypt.Verify(updatedUser.password, user.password))
            {
                return BadRequest(new { message = "Enter previous password correctly" });
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

    [Authorize]
    [HttpGet("test")]
    public IActionResult Test()
    {
        Console.WriteLine("Call received");
        return Ok(new { Message = "Auth controller is working!" });
    }
}
