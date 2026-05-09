using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MenuService.Data;
using MenuService.DTOs;
using MenuService.Models;

namespace MenuService.Controllers;

/// <summary>
/// Menu management controller for cafeteria menu items
/// </summary>
[ApiController]
[Route("api/menu")]
[Produces("application/json")]
public class MenuController : ControllerBase
{
    private readonly MenuDbContext _context;

    /// <summary>
    /// Initializes a new instance of the MenuController
    /// </summary>
    /// <param name="context">Database context for menu operations</param>
    public MenuController(MenuDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all menu items with optional filtering
    /// </summary>
    /// <remarks>
    /// Retrieves a list of menu items that can be filtered by category, search term, or availability.
    /// Results are ordered by category then name.
    /// </remarks>
    /// <param name="category">Optional filter by category (e.g., "main", "beverage", "dessert")</param>
    /// <param name="search">Optional search term for name or description</param>
    /// <param name="available">Optional filter by availability (true/false)</param>
    /// <returns>List of menu items</returns>
    /// <response code="200">Menu items retrieved successfully</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MenuItemDto>), StatusCodes.Status200OK)]
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

    /// <summary>
    /// Get a specific menu item by ID
    /// </summary>
    /// <remarks>
    /// Retrieves detailed information about a specific menu item.
    /// </remarks>
    /// <param name="id">The menu item ID</param>
    /// <returns>Menu item details</returns>
    /// <response code="200">Menu item found</response>
    /// <response code="404">Menu item not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MenuItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _context.MenuItems.FindAsync(id);
        if (item == null)
        {
            return NotFound(new { message = "Menu item not found" });
        }

        return Ok(MapToDto(item));
    }

    /// <summary>
    /// Create a new menu item
    /// </summary>
    /// <remarks>
    /// Creates a new menu item with the specified details.
    /// Name is required and price must be greater than 0.
    /// </remarks>
    /// <param name="request">Menu item creation details</param>
    /// <returns>Created menu item</returns>
    /// <response code="201">Menu item created successfully</response>
    /// <response code="400">Invalid input - missing name or invalid price</response>
    [HttpPost]
    [ProducesResponseType(typeof(MenuItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
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

    /// <summary>
    /// Update an existing menu item
    /// </summary>
    /// <remarks>
    /// Updates a menu item with the specified details.
    /// Only provided fields will be updated (partial updates supported).
    /// </remarks>
    /// <param name="id">The menu item ID</param>
    /// <param name="request">Updated menu item details</param>
    /// <returns>Updated menu item</returns>
    /// <response code="200">Menu item updated successfully</response>
    /// <response code="404">Menu item not found</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MenuItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Delete a menu item
    /// </summary>
    /// <remarks>
    /// Permanently removes a menu item from the database.
    /// </remarks>
    /// <param name="id">The menu item ID</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Menu item deleted successfully</response>
    /// <response code="404">Menu item not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Seed sample menu data
    /// </summary>
    /// <remarks>
    /// Populates the database with sample menu items for testing.
    /// Can only be used on an empty database.
    /// </remarks>
    /// <returns>Success message with count of items added</returns>
    /// <response code="200">Sample data added successfully</response>
    /// <response code="400">Database already contains data</response>
    [HttpPost("seed")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
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

    /// <summary>
    /// Get all unique categories
    /// </summary>
    /// <remarks>
    /// Returns a list of distinct categories currently used by menu items.
    /// </remarks>
    /// <returns>List of category names</returns>
    /// <response code="200">Categories retrieved successfully</response>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
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
