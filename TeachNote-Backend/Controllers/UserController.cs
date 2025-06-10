using Microsoft.AspNetCore.Mvc;
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
    public ActionResult<IEnumerable<User>> GetUsers()
    {
        return new List<User> { };
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

        var newUser = new User
        {
            name = user.name,
            email = user.email,
            password = hashedPassword,
            role = user.role,
            departmentId = user.departmentId
        };
        _context.Users.Add(newUser);
        await _context.SaveChangesAsync(); 
        return Ok(new { message = "New user added succesfully " });
    }

    [HttpGet("test")]

    public IActionResult Test()
    {
        Console.WriteLine("Call received");
        return Ok(new { Message = "Auth controller is working!" });
    }
}
