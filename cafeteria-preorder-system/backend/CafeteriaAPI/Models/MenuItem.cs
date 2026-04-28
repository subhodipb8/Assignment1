using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CafeteriaAPI.Models
{
    public class MenuItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        [MaxLength(20)]
        public string Category { get; set; } = "lunch"; // breakfast, lunch, dinner, snacks, beverages

        [MaxLength(255)]
        public string Image { get; set; } = "/placeholder-food.jpg";

        public List<string> DietaryTags { get; set; } = new();
        public List<string> Allergens { get; set; } = new();

        public NutritionInfo? NutritionInfo { get; set; }

        public bool Available { get; set; } = true;

        public int PreparationTime { get; set; } = 15; // in minutes

        public int MaxOrderPerDay { get; set; } = 100;
        public int OrdersToday { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [JsonIgnore]
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    [Owned]
    public class NutritionInfo
    {
        public int Calories { get; set; }
        public int Protein { get; set; }
        public int Carbs { get; set; }
        public int Fat { get; set; }
    }
}
