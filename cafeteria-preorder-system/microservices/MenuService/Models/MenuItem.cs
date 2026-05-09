using System.ComponentModel.DataAnnotations;

namespace MenuService.Models;

public class MenuItem
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public decimal Price { get; set; }

    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Image { get; set; }

    public string[]? DietaryTags { get; set; }

    public string[]? Allergens { get; set; }

    public bool Available { get; set; } = true;

    public int PreparationTime { get; set; } // in minutes

    public int MaxOrdersPerDay { get; set; } = 100;

    public int OrdersToday { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
