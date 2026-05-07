using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthService.Data;
using AuthService.DTOs;
using AuthService.Services;

namespace AuthService.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly AuthDbContext _context;
    private readonly IJwtService _jwtService;

    public UsersController(AuthDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    [HttpGet("wallet")]
    public async Task<IActionResult> GetWalletBalance()
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

        return Ok(new { balance = user.WalletBalance });
    }

    [HttpPost("wallet/add")]
    public async Task<IActionResult> AddFunds([FromBody] UpdateWalletRequest request)
    {
        var userId = GetUserIdFromToken();
        if (userId == null)
        {
            return Unauthorized(new { message = "Not authenticated" });
        }

        if (request.Amount <= 0)
        {
            return BadRequest(new { message = "Amount must be positive" });
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        user.WalletBalance += request.Amount;
        await _context.SaveChangesAsync();

        return Ok(new { balance = user.WalletBalance });
    }

    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferences()
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

        return Ok(new
        {
            dietaryPreferences = user.DietaryPreferences,
            allergies = user.Allergies
        });
    }

    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences([FromBody] UpdatePreferencesRequest request)
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

        if (request.DietaryPreferences != null)
        {
            user.DietaryPreferences = request.DietaryPreferences;
        }

        if (request.Allergies != null)
        {
            user.Allergies = request.Allergies;
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            dietaryPreferences = user.DietaryPreferences,
            allergies = user.Allergies
        });
    }

    private int? GetUserIdFromToken()
    {
        var authHeader = Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return null;
        }

        var token = authHeader.Substring(7);
        return _jwtService.ValidateToken(token);
    }
}
