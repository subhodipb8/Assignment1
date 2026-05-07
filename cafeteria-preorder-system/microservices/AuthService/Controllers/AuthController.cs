using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthService.Data;
using AuthService.DTOs;
using AuthService.Models;
using AuthService.Services;

namespace AuthService.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthDbContext _context;
    private readonly IJwtService _jwtService;

    public AuthController(AuthDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Validate request
        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Name, email, and password are required" });
        }

        // Check if user already exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return Conflict(new { message = "User with this email already exists" });
        }

        // Validate role
        var validRoles = new[] { "student", "staff", "admin", "canteen" };
        if (!validRoles.Contains(request.Role.ToLower()))
        {
            return BadRequest(new { message = "Invalid role. Must be: student, staff, admin, or canteen" });
        }

        // Create new user
        var user = new User
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role.ToLower(),
            WalletBalance = 0
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Generate JWT token
        var token = _jwtService.GenerateToken(user);

        return Ok(new AuthResponse
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                WalletBalance = user.WalletBalance,
                CreatedAt = user.CreatedAt
            }
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Email and password are required" });
        }

        // Find user by email
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email.Trim().ToLower());

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        // Generate JWT token
        var token = _jwtService.GenerateToken(user);

        return Ok(new AuthResponse
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                DietaryPreferences = user.DietaryPreferences,
                Allergies = user.Allergies,
                WalletBalance = user.WalletBalance,
                CreatedAt = user.CreatedAt
            }
        });
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = GetUserIdFromToken();
        if (userId == null)
        {
            return Unauthorized(new { message = "Not authenticated" });
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            DietaryPreferences = user.DietaryPreferences,
            Allergies = user.Allergies,
            WalletBalance = user.WalletBalance,
            CreatedAt = user.CreatedAt
        });
    }

    private int? GetUserIdFromToken()
    {
        // First check for forwarded header from API Gateway
        if (Request.Headers.TryGetValue("X-User-Id", out var userIdValue) && int.TryParse(userIdValue, out var userIdFromHeader))
        {
            return userIdFromHeader;
        }

        // Fall back to validating token directly
        var authHeader = Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return null;
        }

        var token = authHeader.Substring(7);
        return _jwtService.ValidateToken(token);
    }
}
