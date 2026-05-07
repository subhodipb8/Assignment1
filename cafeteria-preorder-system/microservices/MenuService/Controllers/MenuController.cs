using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MenuService.Data;
using MenuService.DTOs;
using MenuService.Models;

namespace MenuService.Controllers;

[ApiController]
[Route("api/menu")]
public class MenuController : ControllerBase
{
    private readonly MenuDbContext _context;

    public MenuController(MenuDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category, [FromQuery] string? search, [FromQuery] bool? available)
    {
        var query = _context.MenuItems.AsQueryable();

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(m => m.Category.ToLower() == category.ToLower());
        }

        if (!string.IsNullOrEmpty(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(m => m.Name.ToLower().Contains(searchLower) ||
                                    (m.Description != null && m.Description.ToLower().Contains(searchLower)));
        }

        if (available.HasValue)
        {
            query = query.Where(m => m.Available == available.Value);
        }

        var items = await query.OrderBy(m => m.Category).ThenBy(m => m.Name).ToListAsync();

        return Ok(items.Select(MapToDto));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _context.MenuItems.FindAsync(id);
        if (item == null)
        {
            return NotFound(new { message = "Menu item not found" });
        }

        return Ok(MapToDto(item));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMenuItemRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Name is required" });
        }

        if (request.Price <= 0)
        {
            return BadRequest(new { message = "Price must be greater than 0" });
        }

        var menuItem = new MenuItem
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Price = request.Price,
            Category = request.Category.Trim().ToLower(),
            DietaryTags = request.DietaryTags,
            Allergens = request.Allergens,
            Available = request.Available,
            PreparationTime = request.PreparationTime,
            MaxOrdersPerDay = request.MaxOrdersPerDay
        };

        _context.MenuItems.Add(menuItem);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = menuItem.Id }, MapToDto(menuItem));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMenuItemRequest request)
    {
        var item = await _context.MenuItems.FindAsync(id);
        if (item == null)
        {
            return NotFound(new { message = "Menu item not found" });
        }

        if (request.Name != null) item.Name = request.Name.Trim();
        if (request.Description != null) item.Description = request.Description.Trim();
        if (request.Price.HasValue) item.Price = request.Price.Value;
        if (request.Category != null) item.Category = request.Category.Trim().ToLower();
        if (request.DietaryTags != null) item.DietaryTags = request.DietaryTags;
        if (request.Allergens != null) item.Allergens = request.Allergens;
        if (request.Available.HasValue) item.Available = request.Available.Value;
        if (request.PreparationTime.HasValue) item.PreparationTime = request.PreparationTime.Value;
        if (request.MaxOrdersPerDay.HasValue) item.MaxOrdersPerDay = request.MaxOrdersPerDay.Value;

        await _context.SaveChangesAsync();

        return Ok(MapToDto(item));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.MenuItems.FindAsync(id);
        if (item == null)
        {
            return NotFound(new { message = "Menu item not found" });
        }

        _context.MenuItems.Remove(item);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("seed")]
    public async Task<IActionResult> SeedData()
    {
        if (await _context.MenuItems.AnyAsync())
        {
            return BadRequest(new { message = "Database already contains data" });
        }

        var menuItems = new List<MenuItem>
        {
            new()
            {
                Name = "Margherita Pizza",
                Description = "Classic tomato and mozzarella pizza",
                Price = 8.99m,
                Category = "main",
                DietaryTags = new[] { "vegetarian" },
                Allergens = new[] { "gluten", "dairy" },
                Available = true,
                PreparationTime = 20,
                MaxOrdersPerDay = 50
            },
            new()
            {
                Name = "Chicken Caesar Salad",
                Description = "Fresh romaine lettuce with grilled chicken and caesar dressing",
                Price = 10.99m,
                Category = "main",
                DietaryTags = new[] { "high-protein" },
                Allergens = new[] { "dairy", "eggs" },
                Available = true,
                PreparationTime = 10,
                MaxOrdersPerDay = 30
            },
            new()
            {
                Name = "Vegan Buddha Bowl",
                Description = "Quinoa, avocado, chickpeas, and seasonal vegetables",
                Price = 9.99m,
                Category = "main",
                DietaryTags = new[] { "vegan", "gluten-free" },
                Allergens = Array.Empty<string>(),
                Available = true,
                PreparationTime = 15,
                MaxOrdersPerDay = 25
            },
            new()
            {
                Name = "Fresh Orange Juice",
                Description = "Freshly squeezed orange juice",
                Price = 3.99m,
                Category = "beverage",
                DietaryTags = new[] { "vegan", "gluten-free" },
                Allergens = Array.Empty<string>(),
                Available = true,
                PreparationTime = 5,
                MaxOrdersPerDay = 100
            }
        };

        _context.MenuItems.AddRange(menuItems);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Sample data added successfully", count = menuItems.Count });
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _context.MenuItems
            .Select(m => m.Category)
            .Distinct()
            .ToListAsync();

        return Ok(categories);
    }

    private static MenuItemDto MapToDto(MenuItem item) => new()
    {
        Id = item.Id,
        Name = item.Name,
        Description = item.Description,
        Price = item.Price,
        Category = item.Category,
        DietaryTags = item.DietaryTags,
        Allergens = item.Allergens,
        Available = item.Available,
        PreparationTime = item.PreparationTime,
        MaxOrdersPerDay = item.MaxOrdersPerDay,
        OrdersToday = item.OrdersToday,
        CreatedAt = item.CreatedAt
    };
}
