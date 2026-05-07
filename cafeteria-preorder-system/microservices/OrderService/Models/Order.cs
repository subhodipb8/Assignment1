using System.ComponentModel.DataAnnotations;

namespace OrderService.Models;

public class Order
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public decimal TotalAmount { get; set; }

    public DateTime PickupTime { get; set; }

    public DateTime PickupDate { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "pending"; // pending, confirmed, preparing, ready, completed, cancelled

    [MaxLength(20)]
    public string PaymentStatus { get; set; } = "unpaid"; // unpaid, paid, refunded

    public string? SpecialInstructions { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public List<OrderItem> Items { get; set; } = new();
}
