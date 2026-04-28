using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CafeteriaAPI.Models
{
    public class User
    {
        [Key]
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
        public string Role { get; set; } = "student"; // student, staff, admin, canteen

        public List<string> DietaryPreferences { get; set; } = new();
        public List<string> Allergies { get; set; } = new();

        [Column(TypeName = "decimal(18,2)")]
        public decimal WalletBalance { get; set; } = 500.00m;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [JsonIgnore]
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
