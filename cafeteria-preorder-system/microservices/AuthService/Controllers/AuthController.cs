using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthService.Data;
using AuthService.DTOs;
using AuthService.Models;
using AuthService.Services;

namespace AuthService.Controllers;

/// <summary>
/// Authentication controller for user registration, login, and profile management
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly AuthDbContext _context;
    private readonly IJwtService _jwtService;

    /// <summary>
    /// Initializes a new instance of the AuthController
    /// </summary>
    /// <param name="context">Database context for user operations</param>
    /// <param name="jwtService">Service for JWT token generation</param>
    public AuthController(AuthDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <remarks>
    /// Creates a new user with the specified details. Valid roles are: student, staff, admin, canteen.
    /// The email must be unique and will be normalized to lowercase.
    /// </remarks>
    /// <param name="request">User registration details including name, email, password, and role</param>
    /// <returns>Newly created user with JWT token</returns>
    /// <response code="200">User registered successfully</response>
    /// <response code="400">Invalid input - missing required fields or invalid role</response>
    /// <response code="409">User with this email already exists</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
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

    /// <summary>
    /// Authenticate user and generate JWT token
    /// </summary>
    /// <remarks>
    /// Validates user credentials and returns a JWT token for authenticated requests.
    /// Email matching is case-insensitive.
    /// </remarks>
    /// <param name="request">Login credentials containing email and password</param>
    /// <returns>Authenticated user with JWT token</returns>
    /// <response code="200">Login successful</response>
    /// <response code="400">Missing email or password</response>
    /// <response code="401">Invalid email or password</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
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

    /// <summary>
    /// Get current authenticated user details
    /// </summary>
    /// <remarks>
    /// Retrieves the profile information of the currently authenticated user.
    /// Requires authentication via JWT token or X-User-Id header.
    /// </remarks>
    /// <returns>Current user profile</returns>
    /// <response code="200">User profile retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="404">User not found</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
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
