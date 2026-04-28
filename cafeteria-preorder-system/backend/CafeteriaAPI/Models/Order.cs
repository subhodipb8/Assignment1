using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CafeteriaAPI.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [MaxLength(10)]
        public string PickupTime { get; set; } = "12:00"; // 08:00, 08:30, etc.

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "pending"; // pending, confirmed, preparing, ready, completed, cancelled

        [Required]
        [MaxLength(20)]
        public string PaymentStatus { get; set; } = "pending"; // pending, completed, failed

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime PickupDate { get; set; }

        [MaxLength(500)]
        public string? SpecialInstructions { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        [JsonIgnore]
        public Order Order { get; set; } = null!;

        [Required]
        public int MenuItemId { get; set; }

        [ForeignKey("MenuItemId")]
        public MenuItem MenuItem { get; set; } = null!;

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
    }
}
