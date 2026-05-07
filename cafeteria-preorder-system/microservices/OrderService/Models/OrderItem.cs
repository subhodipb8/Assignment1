using System.ComponentModel.DataAnnotations;

namespace OrderService.Models;

public class OrderItem
{
    public int Id { get; set; }

    [Required]
    public int OrderId { get; set; }

    public Order Order { get; set; } = null!;

    [Required]
    public int MenuItemId { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    public decimal Price { get; set; } // Price at time of order

    public string? MenuItemName { get; set; } // Snapshot of name
}
