using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthService.Data;
using AuthService.DTOs;
using AuthService.Services;

namespace AuthService.Controllers;

/// <summary>
/// User management controller for wallet operations and preferences
/// </summary>
[ApiController]
[Route("api/users")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly AuthDbContext _context;
    private readonly IJwtService _jwtService;

    /// <summary>
    /// Initializes a new instance of the UsersController
    /// </summary>
    /// <param name="context">Database context for user operations</param>
    /// <param name="jwtService">Service for JWT token validation</param>
    public UsersController(AuthDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    /// <summary>
    /// Get current user's wallet balance
    /// </summary>
    /// <remarks>
    /// Returns the wallet balance for the authenticated user.
    /// Requires authentication via JWT token or X-User-Id header.
    /// </remarks>
    /// <returns>Current wallet balance</returns>
    /// <response code="200">Wallet balance retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="404">User not found</response>
    [HttpGet("wallet")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Add funds to user's wallet
    /// </summary>
    /// <remarks>
    /// Adds the specified amount to the authenticated user's wallet balance.
    /// Amount must be a positive value.
    /// </remarks>
    /// <param name="request">Amount to add to wallet</param>
    /// <returns>Updated wallet balance</returns>
    /// <response code="200">Funds added successfully</response>
    /// <response code="400">Invalid amount (must be positive)</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="404">User not found</response>
    [HttpPost("wallet/add")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Deduct funds from user's wallet
    /// </summary>
    /// <remarks>
    /// Deducts the specified amount from the authenticated user's wallet balance.
    /// User must have sufficient funds for the deduction to succeed.
    /// </remarks>
    /// <param name="request">Amount to deduct from wallet</param>
    /// <returns>Updated wallet balance</returns>
    /// <response code="200">Funds deducted successfully</response>
    /// <response code="400">Invalid amount or insufficient funds</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="404">User not found</response>
    [HttpPost("wallet/deduct")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeductFunds([FromBody] UpdateWalletRequest request)
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

        if (user.WalletBalance < request.Amount)
        {
            return BadRequest(new { message = "Insufficient funds" });
        }

        user.WalletBalance -= request.Amount;
        await _context.SaveChangesAsync();

        return Ok(new { balance = user.WalletBalance });
    }

    /// <summary>
    /// Get user's dietary preferences and allergies
    /// </summary>
    /// <remarks>
    /// Retrieves the dietary preferences and allergy information for the authenticated user.
    /// </remarks>
    /// <returns>User's dietary preferences and allergies</returns>
    /// <response code="200">Preferences retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="404">User not found</response>
    [HttpGet("preferences")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Update user's dietary preferences and allergies
    /// </summary>
    /// <remarks>
    /// Updates the dietary preferences and/or allergy information for the authenticated user.
    /// Only provided fields will be updated (partial updates supported).
    /// </remarks>
    /// <param name="request">Updated preferences and/or allergies</param>
    /// <returns>Updated preferences and allergies</returns>
    /// <response code="200">Preferences updated successfully</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="404">User not found</response>
    [HttpPut("preferences")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
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
        // First check for forwarded header from API Gateway
        if (Request.Headers.TryGetValue("X-User-Id", out var userIdValue) && int.TryParse(userIdValue, out var userIdFromHeader))
        {
            return userIdFromHeader;
        }

        // Fall back to validating token directly (for direct service access)
        var authHeader = Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return null;
        }

        var token = authHeader.Substring(7);
        return _jwtService.ValidateToken(token);
    }
}
