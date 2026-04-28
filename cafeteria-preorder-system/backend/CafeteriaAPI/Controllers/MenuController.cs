using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CafeteriaAPI.Data;
using CafeteriaAPI.Models;

namespace CafeteriaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : ControllerBase
    {
        private readonly CafeteriaDbContext _context;

        public MenuController(CafeteriaDbContext context)
        {
            _context = context;
        }

        // GET: api/menu
        [HttpGet]
        public async Task<IActionResult> GetMenuItems(
            [FromQuery] string? category = null,
            [FromQuery] bool? available = null,
            [FromQuery] string? search = null)
        {
            var query = _context.MenuItems.AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(m => m.Category == category.ToLower());
            }

            if (available.HasValue)
            {
                query = query.Where(m => m.Available == available.Value);
            }

            if (!string.IsNullOrEmpty(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(m =>
                    EF.Functions.Like(m.Name.ToLower(), $"%{lowerSearch}%") ||
                    EF.Functions.Like(m.Description.ToLower(), $"%{lowerSearch}%"));
            }

            var menuItems = await query.OrderBy(m => m.Category).ThenBy(m => m.Name).ToListAsync();
            return Ok(menuItems);
        }

        // GET: api/menu/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMenuItem(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
            {
                return NotFound(new { message = "Menu item not found" });
            }
            return Ok(menuItem);
        }

        // POST: api/menu
        [HttpPost]
        public async Task<IActionResult> CreateMenuItem([FromBody] MenuItem menuItem)
        {
            menuItem.CreatedAt = DateTime.UtcNow;
            _context.MenuItems.Add(menuItem);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetMenuItem), new { id = menuItem.Id }, menuItem);
        }

        // PUT: api/menu/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMenuItem(int id, [FromBody] MenuItem menuItem)
        {
            if (id != menuItem.Id)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            var existingItem = await _context.MenuItems.FindAsync(id);
            if (existingItem == null)
            {
                return NotFound(new { message = "Menu item not found" });
            }

            existingItem.Name = menuItem.Name;
            existingItem.Description = menuItem.Description;
            existingItem.Price = menuItem.Price;
            existingItem.Category = menuItem.Category;
            existingItem.Image = menuItem.Image;
            existingItem.DietaryTags = menuItem.DietaryTags;
            existingItem.Allergens = menuItem.Allergens;
            existingItem.NutritionInfo = menuItem.NutritionInfo;
            existingItem.Available = menuItem.Available;
            existingItem.PreparationTime = menuItem.PreparationTime;
            existingItem.MaxOrderPerDay = menuItem.MaxOrderPerDay;
            existingItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(existingItem);
        }

        // DELETE: api/menu/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
            {
                return NotFound(new { message = "Menu item not found" });
            }

            _context.MenuItems.Remove(menuItem);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Menu item deleted successfully" });
        }

        // GET: api/menu/categories
        [HttpGet("categories")]
        public IActionResult GetCategories()
        {
            var categories = new[] { "breakfast", "lunch", "dinner", "snacks", "beverages" };
            return Ok(categories);
        }

        // POST: api/menu/seed
        [HttpPost("seed")]
        public async Task<IActionResult> SeedMenuItems()
        {
            // Clear existing menu items
            var existingItems = await _context.MenuItems.ToListAsync();
            if (existingItems.Any())
            {
                _context.MenuItems.RemoveRange(existingItems);
                await _context.SaveChangesAsync();
            }

            var menuItems = new List<MenuItem>
            {
                new MenuItem
                {
                    Name = "Masala Dosa",
                    Description = "Crispy rice crepes filled with spiced potato filling",
                    Price = 60,
                    Category = "breakfast",
                    Image = "https://images.unsplash.com/photo-1582237555184-6d9cf1247502?w=400&h=300&fit=crop",
                    DietaryTags = new List<string> { "vegetarian", "vegan" },
                    Allergens = new List<string>(),
                    NutritionInfo = new NutritionInfo { Calories = 250, Protein = 6, Carbs = 40, Fat = 8 },
                    Available = true,
                    PreparationTime = 15
                },
                new MenuItem
                {
                    Name = "Idli Sambar",
                    Description = "Steamed rice cakes served with lentil soup",
                    Price = 40,
                    Category = "breakfast",
                    Image = "https://images.unsplash.com/photo-1565557623262-b51c2513a641?w=400&h=300&fit=crop",
                    DietaryTags = new List<string> { "vegetarian", "vegan", "gluten-free" },
                    Allergens = new List<string>(),
                    NutritionInfo = new NutritionInfo { Calories = 180, Protein = 8, Carbs = 35, Fat = 2 },
                    Available = true,
                    PreparationTime = 10
                },
                new MenuItem
                {
                    Name = "Paneer Tikka Rice Bowl",
                    Description = "Grilled cottage cheese cubes with aromatic rice and vegetables",
                    Price = 120,
                    Category = "lunch",
                    Image = "https://images.unsplash.com/photo-1585937421612-70a008356fbe?w=400&h=300&fit=crop",
                    DietaryTags = new List<string> { "vegetarian" },
                    Allergens = new List<string> { "dairy" },
                    NutritionInfo = new NutritionInfo { Calories = 450, Protein = 18, Carbs = 55, Fat = 15 },
                    Available = true,
                    PreparationTime = 20
                },
                new MenuItem
                {
                    Name = "Chicken Biryani",
                    Description = "Fragrant basmati rice cooked with tender chicken and spices",
                    Price = 150,
                    Category = "lunch",
                    Image = "https://images.unsplash.com/photo-1589302168068-9e7630de16f1?w=400&h=300&fit=crop",
                    DietaryTags = new List<string>(),
                    Allergens = new List<string>(),
                    NutritionInfo = new NutritionInfo { Calories = 550, Protein = 25, Carbs = 65, Fat = 20 },
                    Available = true,
                    PreparationTime = 25
                },
                new MenuItem
                {
                    Name = "Vegetable Curry with Rice",
                    Description = "Mixed vegetable curry served with steamed rice",
                    Price = 80,
                    Category = "dinner",
                    Image = "https://images.unsplash.com/photo-1606471191009-63994c66933b?w=400&h=300&fit=crop",
                    DietaryTags = new List<string> { "vegetarian", "vegan" },
                    Allergens = new List<string>(),
                    NutritionInfo = new NutritionInfo { Calories = 380, Protein = 10, Carbs = 60, Fat = 12 },
                    Available = true,
                    PreparationTime = 20
                },
                new MenuItem
                {
                    Name = "Samosa",
                    Description = "Crispy pastry filled with spiced potatoes and peas",
                    Price = 20,
                    Category = "snacks",
                    Image = "https://images.unsplash.com/photo-1601050690597-df0568f75450?w=400&h=300&fit=crop",
                    DietaryTags = new List<string> { "vegetarian" },
                    Allergens = new List<string> { "gluten" },
                    NutritionInfo = new NutritionInfo { Calories = 150, Protein = 3, Carbs = 20, Fat = 7 },
                    Available = true,
                    PreparationTime = 5
                },
                new MenuItem
                {
                    Name = "Mango Lassi",
                    Description = "Refreshing yogurt drink with mango pulp",
                    Price = 50,
                    Category = "beverages",
                    Image = "https://images.unsplash.com/photo-1623065422902-30a2d299bbe4?w=400&h=300&fit=crop",
                    DietaryTags = new List<string> { "vegetarian" },
                    Allergens = new List<string> { "dairy" },
                    NutritionInfo = new NutritionInfo { Calories = 120, Protein = 4, Carbs = 25, Fat = 2 },
                    Available = true,
                    PreparationTime = 5
                },
                new MenuItem
                {
                    Name = "Masala Chai",
                    Description = "Spiced Indian tea with milk",
                    Price = 25,
                    Category = "beverages",
                    Image = "https://images.unsplash.com/photo-1561336313-0bd5e0b27ec8?w=400&h=300&fit=crop",
                    DietaryTags = new List<string> { "vegetarian" },
                    Allergens = new List<string> { "dairy" },
                    NutritionInfo = new NutritionInfo { Calories = 80, Protein = 3, Carbs = 10, Fat = 3 },
                    Available = true,
                    PreparationTime = 5
                }
            };

            foreach (var item in menuItems)
            {
                item.CreatedAt = DateTime.UtcNow;
                _context.MenuItems.Add(item);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Menu items seeded successfully", count = menuItems.Count });
        }
    }
}
