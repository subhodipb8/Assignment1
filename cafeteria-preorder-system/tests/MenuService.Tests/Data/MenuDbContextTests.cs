using Xunit;
using Microsoft.EntityFrameworkCore;
using MenuService.Data;
using MenuService.Models;
using FluentAssertions;

namespace MenuService.Tests.Data;

public class MenuDbContextTests : IDisposable
{
    private readonly MenuDbContext _context;

    public MenuDbContextTests()
    {
        var options = new DbContextOptionsBuilder<MenuDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new MenuDbContext(options);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public void MenuDbContext_CanBeConstructed()
    {
        // Assert
        _context.Should().NotBeNull();
        _context.MenuItems.Should().NotBeNull();
    }

    [Fact]
    public async Task MenuDbContext_CanAddAndRetrieveMenuItem()
    {
        // Arrange
        var item = new MenuItem
        {
            Name = "Test Item",
            Description = "Test Description",
            Price = 12.99m,
            Category = "main"
        };

        // Act
        _context.MenuItems.Add(item);
        await _context.SaveChangesAsync();

        // Assert
        var retrievedItem = await _context.MenuItems.FirstOrDefaultAsync(i => i.Name == "Test Item");
        retrievedItem.Should().NotBeNull();
        retrievedItem!.Price.Should().Be(12.99m);
        retrievedItem.Category.Should().Be("main");
    }

    [Fact]
    public async Task MenuDbContext_CanUpdateMenuItem()
    {
        // Arrange
        var item = new MenuItem
        {
            Name = "Original Name",
            Price = 10m,
            Category = "main"
        };

        _context.MenuItems.Add(item);
        await _context.SaveChangesAsync();

        // Act
        item.Name = "Updated Name";
        item.Price = 15m;
        _context.MenuItems.Update(item);
        await _context.SaveChangesAsync();

        // Assert
        var updatedItem = await _context.MenuItems.FindAsync(item.Id);
        updatedItem!.Name.Should().Be("Updated Name");
        updatedItem.Price.Should().Be(15m);
    }

    [Fact]
    public async Task MenuDbContext_CanDeleteMenuItem()
    {
        // Arrange
        var item = new MenuItem
        {
            Name = "To Delete",
            Price = 10m,
            Category = "main"
        };

        _context.MenuItems.Add(item);
        await _context.SaveChangesAsync();

        // Act
        _context.MenuItems.Remove(item);
        await _context.SaveChangesAsync();

        // Assert
        var deletedItem = await _context.MenuItems.FirstOrDefaultAsync(i => i.Name == "To Delete");
        deletedItem.Should().BeNull();
    }

    [Fact]
    public async Task MenuDbContext_CanQueryWithFilters()
    {
        // Arrange
        var items = new List<MenuItem>
        {
            new() { Name = "Burger", Price = 12m, Category = "main", Available = true },
            new() { Name = "Salad", Price = 8m, Category = "main", Available = true },
            new() { Name = "Soda", Price = 3m, Category = "beverage", Available = false }
        };

        _context.MenuItems.AddRange(items);
        await _context.SaveChangesAsync();

        // Act
        var mainItems = await _context.MenuItems.Where(i => i.Category == "main").ToListAsync();
        var availableItems = await _context.MenuItems.Where(i => i.Available).ToListAsync();

        // Assert
        mainItems.Should().HaveCount(2);
        availableItems.Should().HaveCount(2);
    }

    [Fact]
    public async Task MenuDbContext_CanQueryWithPriceRange()
    {
        // Arrange
        var items = new List<MenuItem>
        {
            new() { Name = "Cheap", Price = 5m, Category = "snack" },
            new() { Name = "Mid", Price = 12m, Category = "main" },
            new() { Name = "Expensive", Price = 25m, Category = "premium" }
        };

        _context.MenuItems.AddRange(items);
        await _context.SaveChangesAsync();

        // Act
        var affordableItems = await _context.MenuItems
            .Where(i => i.Price >= 5m && i.Price <= 15m)
            .ToListAsync();

        // Assert
        affordableItems.Should().HaveCount(2);
    }

    [Fact]
    public async Task MenuDbContext_FindAsync_ReturnsNullForNonexistentId()
    {
        // Act
        var result = await _context.MenuItems.FindAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task MenuDbContext_CanAddItemsWithArrays()
    {
        // Arrange
        var item = new MenuItem
        {
            Name = "Vegan Burger",
            Price = 15m,
            Category = "main",
            DietaryTags = new[] { "vegan", "gluten-free" },
            Allergens = new[] { "soy", "tree-nuts" }
        };

        // Act
        _context.MenuItems.Add(item);
        await _context.SaveChangesAsync();

        // Assert
        var retrievedItem = await _context.MenuItems.FirstAsync();
        retrievedItem.DietaryTags.Should().ContainInOrder("vegan", "gluten-free");
        retrievedItem.Allergens.Should().ContainInOrder("soy", "tree-nuts");
    }

    [Fact]
    public async Task MenuDbContext_CanQueryWithSearchPattern()
    {
        // Arrange
        var items = new List<MenuItem>
        {
            new() { Name = "Chicken Burger", Description = "Tasty chicken", Price = 10m, Category = "main" },
            new() { Name = "Beef Burger", Description = "Juicy beef", Price = 12m, Category = "main" },
            new() { Name = "Fish and Chips", Description = "Crispy fish", Price = 14m, Category = "main" }
        };

        _context.MenuItems.AddRange(items);
        await _context.SaveChangesAsync();

        // Act
        var burgerItems = await _context.MenuItems
            .Where(i => i.Name.ToLower().Contains("burger"))
            .ToListAsync();

        // Assert
        burgerItems.Should().HaveCount(2);
    }

    [Fact]
    public async Task MenuDbContext_CanOrderByCategoryThenName()
    {
        // Arrange
        var items = new List<MenuItem>
        {
            new() { Name = "Zebra", Price = 1m, Category = "dessert" },
            new() { Name = "Apple", Price = 1m, Category = "main" },
            new() { Name = "Banana", Price = 1m, Category = "main" }
        };

        _context.MenuItems.AddRange(items);
        await _context.SaveChangesAsync();

        // Act
        var orderedItems = await _context.MenuItems
            .OrderBy(i => i.Category)
            .ThenBy(i => i.Name)
            .Select(i => i.Name)
            .ToListAsync();

        // Assert
        // dessert comes before main alphabetically
        orderedItems.Should().ContainInOrder("Zebra", "Apple", "Banana");
    }

    [Fact]
    public async Task MenuDbContext_CanGetDistinctCategories()
    {
        // Arrange
        var items = new List<MenuItem>
        {
            new() { Name = "Item 1", Price = 1m, Category = "main" },
            new() { Name = "Item 2", Price = 1m, Category = "main" },
            new() { Name = "Item 3", Price = 1m, Category = "beverage" },
            new() { Name = "Item 4", Price = 1m, Category = "dessert" }
        };

        _context.MenuItems.AddRange(items);
        await _context.SaveChangesAsync();

        // Act
        var categories = await _context.MenuItems
            .Select(i => i.Category)
            .Distinct()
            .ToListAsync();

        // Assert
        categories.Should().HaveCount(3);
        categories.Should().Contain(new[] { "main", "beverage", "dessert" });
    }

    [Fact]
    public async Task MenuDbContext_CanFilterByDietaryTags()
    {
        // Arrange
        var items = new List<MenuItem>
        {
            new() { Name = "Vegan Burger", Price = 12m, Category = "main", DietaryTags = new[] { "vegan" } },
            new() { Name = "Vegan Salad", Price = 10m, Category = "main", DietaryTags = new[] { "vegan", "gluten-free" } },
            new() { Name = "Regular Burger", Price = 12m, Category = "main", DietaryTags = new[] { "high-protein" } }
        };

        _context.MenuItems.AddRange(items);
        await _context.SaveChangesAsync();

        // Act
        // Note: This is a simplified test; actual array contains queries depend on EF Core provider
        var veganItems = await _context.MenuItems
            .Where(i => i.DietaryTags != null && i.DietaryTags.Contains("vegan"))
            .ToListAsync();

        // Assert - in-memory database supports array contains
        veganItems.Should().HaveCount(2);
    }

    [Fact]
    public async Task MenuDbContext_CanCountItems()
    {
        // Arrange
        var items = new List<MenuItem>
        {
            new() { Name = "Item 1", Price = 1m, Category = "main", Available = true },
            new() { Name = "Item 2", Price = 1m, Category = "main", Available = true },
            new() { Name = "Item 3", Price = 1m, Category = "main", Available = false }
        };

        _context.MenuItems.AddRange(items);
        await _context.SaveChangesAsync();

        // Act
        var totalCount = await _context.MenuItems.CountAsync();
        var availableCount = await _context.MenuItems.CountAsync(i => i.Available);

        // Assert
        totalCount.Should().Be(3);
        availableCount.Should().Be(2);
    }

    [Fact]
    public async Task MenuDbContext_CanSumPrices()
    {
        // Arrange
        var items = new List<MenuItem>
        {
            new() { Name = "Item 1", Price = 10m, Category = "main" },
            new() { Name = "Item 2", Price = 15m, Category = "main" },
            new() { Name = "Item 3", Price = 25m, Category = "main" }
        };

        _context.MenuItems.AddRange(items);
        await _context.SaveChangesAsync();

        // Act
        var totalValue = await _context.MenuItems.SumAsync(i => i.Price);

        // Assert
        totalValue.Should().Be(50m);
    }
}
