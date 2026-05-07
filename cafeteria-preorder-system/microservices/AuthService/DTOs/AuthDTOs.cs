namespace AuthService.DTOs;

public class RegisterRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "student";
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = new();
}

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string[]? DietaryPreferences { get; set; }
    public string[]? Allergies { get; set; }
    public decimal WalletBalance { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateWalletRequest
{
    public decimal Amount { get; set; }
}

public class UpdatePreferencesRequest
{
    public string[]? DietaryPreferences { get; set; }
    public string[]? Allergies { get; set; }
}
