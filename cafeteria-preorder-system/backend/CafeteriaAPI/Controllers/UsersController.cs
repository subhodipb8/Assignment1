using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CafeteriaAPI.Data;

namespace CafeteriaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly CafeteriaDbContext _context;

        public UsersController(CafeteriaDbContext context)
        {
            _context = context;
        }

        // GET: api/users/wallet
        [HttpGet("wallet")]
        public async Task<IActionResult> GetWalletBalance()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound(new { message = "User not found" });

            return Ok(new
            {
                balance = user.WalletBalance,
                userId = user.Id,
                userName = user.Name
            });
        }

        // POST: api/users/wallet/add
        [HttpPost("wallet/add")]
        public async Task<IActionResult> AddFunds([FromBody] AddFundsRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound(new { message = "User not found" });

            if (request.Amount <= 0 || request.Amount > 10000)
            {
                return BadRequest(new { message = "Amount must be between 1 and 10000" });
            }

            user.WalletBalance += request.Amount;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Funds added successfully",
                newBalance = user.WalletBalance
            });
        }

        // PUT: api/users/preferences
        [HttpPut("preferences")]
        public async Task<IActionResult> UpdatePreferences([FromBody] UpdatePreferencesRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound(new { message = "User not found" });

            user.DietaryPreferences = request.DietaryPreferences ?? user.DietaryPreferences;
            user.Allergies = request.Allergies ?? user.Allergies;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Preferences updated successfully",
                dietaryPreferences = user.DietaryPreferences,
                allergies = user.Allergies
            });
        }

        // GET: api/users/preferences
        [HttpGet("preferences")]
        public async Task<IActionResult> GetPreferences()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound(new { message = "User not found" });

            return Ok(new
            {
                dietaryPreferences = user.DietaryPreferences,
                allergies = user.Allergies
            });
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return null;
            return int.Parse(userIdClaim);
        }
    }

    public class AddFundsRequest
    {
        public decimal Amount { get; set; }
    }

    public class UpdatePreferencesRequest
    {
        public List<string>? DietaryPreferences { get; set; }
        public List<string>? Allergies { get; set; }
    }
}
