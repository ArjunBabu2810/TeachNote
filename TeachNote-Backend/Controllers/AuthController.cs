using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeachNote_Backend.Models;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly JwtTokenService _jwt;
    private readonly AppDbContext _context;
    public AuthController(AppDbContext context,JwtTokenService jwt)
    {
        _context = context;
        _jwt = jwt;
    }

    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok(new { Message = "Auth controller is working!" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        Console.WriteLine($"Login attempt by: {request.Email}");
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Email and password are needed" });
            }
            var user = await _context.Users.FirstOrDefaultAsync(u => u.email == request.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.password))
            {
                return Unauthorized(new {message = "Invalid Credentials !"});
            }
            var role = user.role;
            Console.WriteLine("Login Success.");
            var token = _jwt.GenerateToken(user.email, user.role);
            foreach (var claim in User.Claims){
                Console.WriteLine($"Claim type:{claim.Type} value : {claim.Value}");
            }
            return Ok(new { Token = token, role });
            
        }
        catch (System.Exception ex)
        {
            Console.WriteLine("Login failed.");
            Console.WriteLine($"Login error: {ex.Message}");
            return StatusCode(500, new { message = "Internal Server Error. Please try again later." });
            throw;
        }
    }
}

public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}
