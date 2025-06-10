using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly JwtTokenService _jwt;

    public AuthController(JwtTokenService jwt)
    {
        _jwt = jwt;
    }

    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok(new { Message = "Auth controller is working!" });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        Console.WriteLine($"Login attempt by: {request.Email}");
        try
        {

            if (request.Email == "admin" && request.Password == "admin")
            {
                var token = _jwt.GenerateToken(request.Email, "Admin");
                return Ok(new { Token = token });
            }

            Console.WriteLine("Login failed.");
        }
        catch (System.Exception)
        {
            Console.WriteLine("Login failed.");
            throw;
        }
        
            return Unauthorized();
            
    }
}

public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}
