using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CafeteriaAPI.Data;
using CafeteriaAPI.DTOs;
using CafeteriaAPI.Models;
using CafeteriaAPI.Services;

namespace CafeteriaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly CafeteriaDbContext _context;
        private readonly IJwtService _jwtService;

        public AuthController(CafeteriaDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Check if user already exists
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest(new { message = "User with this email already exists" });
            }

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create new user
            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = passwordHash,
                Role = request.Role,
                DietaryPreferences = request.DietaryPreferences,
                Allergies = request.Allergies
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Generate token
            var token = _jwtService.GenerateToken(user);

            return Ok(new AuthResponse
            {
                Token = token,
                User = new UserDTO
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role,
                    WalletBalance = user.WalletBalance,
                    DietaryPreferences = user.DietaryPreferences,
                    Allergies = user.Allergies
                }
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Generate token
            var token = _jwtService.GenerateToken(user);

            return Ok(new AuthResponse
            {
                Token = token,
                User = new UserDTO
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role,
                    WalletBalance = user.WalletBalance,
                    DietaryPreferences = user.DietaryPreferences,
                    Allergies = user.Allergies
                }
            });
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(new UserDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                WalletBalance = user.WalletBalance,
                DietaryPreferences = user.DietaryPreferences,
                Allergies = user.Allergies
            });
        }
    }
}
