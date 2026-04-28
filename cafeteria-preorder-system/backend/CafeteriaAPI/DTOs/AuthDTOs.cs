using System.ComponentModel.DataAnnotations;

namespace CafeteriaAPI.DTOs
{
    public class RegisterRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Role { get; set; } = "student";

        public List<string> DietaryPreferences { get; set; } = new();
        public List<string> Allergies { get; set; } = new();
    }

    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public UserDTO User { get; set; } = null!;
    }

    public class UserDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public decimal WalletBalance { get; set; }
        public List<string> DietaryPreferences { get; set; } = new();
        public List<string> Allergies { get; set; } = new();
    }
}
