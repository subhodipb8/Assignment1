using System.ComponentModel.DataAnnotations;

namespace AuthService.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = "student";

    public string[]? DietaryPreferences { get; set; }

    public string[]? Allergies { get; set; }

    public decimal WalletBalance { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
