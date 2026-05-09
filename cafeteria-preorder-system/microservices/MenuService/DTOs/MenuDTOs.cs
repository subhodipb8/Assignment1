namespace MenuService.DTOs;

public class CreateMenuItemRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Image { get; set; }
    public string[]? DietaryTags { get; set; }
    public string[]? Allergens { get; set; }
    public bool Available { get; set; } = true;
    public int PreparationTime { get; set; } = 15;
    public int MaxOrdersPerDay { get; set; } = 100;
}

public class UpdateMenuItemRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public string? Category { get; set; }
    public string? Image { get; set; }
    public string[]? DietaryTags { get; set; }
    public string[]? Allergens { get; set; }
    public bool? Available { get; set; }
    public int? PreparationTime { get; set; }
    public int? MaxOrdersPerDay { get; set; }
}

public class MenuItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Image { get; set; }
    public string[]? DietaryTags { get; set; }
    public string[]? Allergens { get; set; }
    public bool Available { get; set; }
    public int PreparationTime { get; set; }
    public int MaxOrdersPerDay { get; set; }
    public int OrdersToday { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MenuFilterRequest
{
    public string? Category { get; set; }
    public string? Search { get; set; }
    public bool? Available { get; set; }
    public string[]? DietaryTags { get; set; }
}
