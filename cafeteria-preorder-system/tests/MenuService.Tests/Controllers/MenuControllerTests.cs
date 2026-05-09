using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MenuService.Controllers;
using MenuService.Data;
using MenuService.DTOs;
using MenuService.Models;
using Moq;
using FluentAssertions;

namespace MenuService.Tests.Controllers;

public class MenuControllerTests : IDisposable
{
    private readonly MenuDbContext _context;
    private readonly MenuController _controller;

    public MenuControllerTests()
    {
        var options = new DbContextOptionsBuilder<MenuDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new MenuDbContext(options);
        _controller = new MenuController(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithNoItems_ReturnsEmptyList()
    {
        // Act
        var result = await _controller.GetAll(null, null, null);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = okResult.Value as IEnumerable<MenuItemDto>;
        items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_WithItems_ReturnsAllItems()
    {
        // Arrange
        await SeedTestData();

        // Act
        var result = await _controller.GetAll(null, null, null);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = okResult.Value as IEnumerable<MenuItemDto>;
        items.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAll_WithCategoryFilter_ReturnsFilteredItems()
    {
        // Arrange
        await SeedTestData();

        // Act
        var result = await _controller.GetAll("main", null, null);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = okResult.Value as IEnumerable<MenuItemDto>;
        items.Should().HaveCount(2);
        items!.All(i => i.Category == "main").Should().BeTrue();
    }

    [Fact]
    public async Task GetAll_WithSearchQuery_ReturnsMatchingItems()
    {
        // Arrange
        await SeedTestData();

        // Act
        var result = await _controller.GetAll(null, "burger", null);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = okResult.Value as IEnumerable<MenuItemDto>;
        items.Should().HaveCount(1);
        items!.First().Name.ToLower().Should().Contain("burger");
    }

    [Fact]
    public async Task GetAll_WithAvailableFilter_ReturnsOnlyAvailableItems()
    {
        // Arrange
        await SeedTestData();
        var unavailableItem = new MenuItem
        {
            Name = "Unavailable Item",
            Description = "Test",
            Price = 10m,
            Category = "test",
            Available = false
        };
        _context.MenuItems.Add(unavailableItem);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAll(null, null, true);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = okResult.Value as IEnumerable<MenuItemDto>;
        items.Should().HaveCount(3);
        items!.All(i => i.Available).Should().BeTrue();
    }

    [Fact]
    public async Task GetAll_WithUnavailableFilter_ReturnsOnlyUnavailableItems()
    {
        // Arrange
        await SeedTestData();
        var unavailableItem = new MenuItem
        {
            Name = "Unavailable Item",
            Description = "Test",
            Price = 10m,
            Category = "test",
            Available = false
        };
        _context.MenuItems.Add(unavailableItem);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAll(null, null, false);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = okResult.Value as IEnumerable<MenuItemDto>;
        items.Should().HaveCount(1);
        items!.First().Available.Should().BeFalse();
    }

    [Fact]
    public async Task GetAll_IsCaseInsensitiveForSearch()
    {
        // Arrange
        await SeedTestData();

        // Act
        var resultLower = await _controller.GetAll(null, "burger", null);
        var resultUpper = await _controller.GetAll(null, "BURGER", null);

        // Assert
        var lowerOk = resultLower.As<OkObjectResult>();
        var upperOk = resultUpper.As<OkObjectResult>();
        var lowerItems = lowerOk.Value as IEnumerable<MenuItemDto>;
        var upperItems = upperOk.Value as IEnumerable<MenuItemDto>;
        lowerItems!.Count().Should().Be(upperItems!.Count());
    }

    [Fact]
    public async Task GetAll_IsCaseInsensitiveForCategory()
    {
        // Arrange
        await SeedTestData();

        // Act
        var resultLower = await _controller.GetAll("main", null, null);
        var resultUpper = await _controller.GetAll("MAIN", null, null);

        // Assert
        var lowerOk = resultLower.As<OkObjectResult>();
        var upperOk = resultUpper.As<OkObjectResult>();
        var lowerItems = lowerOk.Value as IEnumerable<MenuItemDto>;
        var upperItems = upperOk.Value as IEnumerable<MenuItemDto>;
        lowerItems!.Count().Should().Be(upperItems!.Count());
    }

    [Fact]
    public async Task GetAll_SearchInDescription_Works()
    {
        // Arrange
        await SeedTestData();

        // Act
        var result = await _controller.GetAll(null, "delicious", null);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = okResult.Value as IEnumerable<MenuItemDto>;
        items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAll_CombinedFilters_Work()
    {
        // Arrange
        await SeedTestData();

        // Act
        var result = await _controller.GetAll("main", "burger", true);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var items = okResult.Value as IEnumerable<MenuItemDto>;
        items.Should().HaveCount(1);
        var item = items!.First();
        item.Category.Should().Be("main");
        item.Name.Should().Contain("Burger");
        item.Available.Should().BeTrue();
    }

    [Fact]
    public async Task GetAll_ResultsAreOrderedByCategoryThenName()
    {
        // Arrange
        var items = new List<MenuItem>
        {
            new() { Name = "Zebra", Description = "Test", Price = 1, Category = "dessert" },
            new() { Name = "Apple", Description = "Test", Price = 1, Category = "main" },
            new() { Name = "Banana", Description = "Test", Price = 1, Category = "main" }
        };
        _context.MenuItems.AddRange(items);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAll(null, null, null);

        // Assert
        var okResult = result.As<OkObjectResult>();
        var results = okResult.Value as IEnumerable<MenuItemDto>;
        var ordered = results!.ToList();
        // Ordered by Category (dessert < main), then by Name
        ordered[0].Name.Should().Be("Zebra");   // dessert category
        ordered[1].Name.Should().Be("Apple");   // main category
        ordered[2].Name.Should().Be("Banana"); // main category
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ReturnsItem()
    {
        // Arrange
        await SeedTestData();
        var item = await _context.MenuItems.FirstAsync();

        // Act
        var result = await _controller.GetById(item.Id);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = okResult.Value.Should().BeOfType<MenuItemDto>().Subject;
        dto.Id.Should().Be(item.Id);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetById_ReturnsCorrectDtoMapping()
    {
        // Arrange
        var item = new MenuItem
        {
            Name = "Test Item",
            Description = "Test Description",
            Price = 15.99m,
            Category = "test",
            DietaryTags = new[] { "vegan" },
            Allergens = new[] { "none" },
            Available = true,
            PreparationTime = 20,
            MaxOrdersPerDay = 50,
            OrdersToday = 10
        };
        _context.MenuItems.Add(item);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetById(item.Id);

        // Assert
        var okResult = result.As<OkObjectResult>();
        var dto = okResult.Value.As<MenuItemDto>();
        dto.Name.Should().Be("Test Item");
        dto.Description.Should().Be("Test Description");
        dto.Price.Should().Be(15.99m);
        dto.Category.Should().Be("test");
        dto.DietaryTags.Should().ContainSingle().Which.Should().Be("vegan");
        dto.Allergens.Should().ContainSingle().Which.Should().Be("none");
        dto.Available.Should().BeTrue();
        dto.PreparationTime.Should().Be(20);
        dto.MaxOrdersPerDay.Should().Be(50);
        dto.OrdersToday.Should().Be(10);
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = new CreateMenuItemRequest
        {
            Name = "New Item",
            Description = "A new menu item",
            Price = 12.99m,
            Category = "main",
            DietaryTags = new[] { "vegetarian" },
            Allergens = new[] { "dairy" },
            Available = true,
            PreparationTime = 15,
            MaxOrdersPerDay = 30
        };

        // Act
        var result = await _controller.Create(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result.As<CreatedAtActionResult>();
        createdResult.ActionName.Should().Be(nameof(MenuController.GetById));
        createdResult.RouteValues!["id"].Should().NotBeNull();
    }

    [Fact]
    public async Task Create_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateMenuItemRequest
        {
            Name = "",
            Price = 10m,
            Category = "main"
        };

        // Act
        var result = await _controller.Create(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public async Task Create_WithInvalidPrice_ReturnsBadRequest(decimal price)
    {
        // Arrange
        var request = new CreateMenuItemRequest
        {
            Name = "Test Item",
            Price = price,
            Category = "main"
        };

        // Act
        var result = await _controller.Create(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_WithWhitespaceName_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateMenuItemRequest
        {
            Name = "   ",
            Price = 10m,
            Category = "main"
        };

        // Act
        var result = await _controller.Create(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_TrimsWhitespaceFromName()
    {
        // Arrange
        var request = new CreateMenuItemRequest
        {
            Name = "  Test Item  ",
            Price = 10m,
            Category = "  MAIN  "
        };

        // Act
        await _controller.Create(request);

        // Assert
        var item = await _context.MenuItems.FirstAsync();
        item.Name.Should().Be("Test Item");
        item.Category.Should().Be("main");
    }

    [Fact]
    public async Task Create_WithNullDescription_SavesNull()
    {
        // Arrange
        var request = new CreateMenuItemRequest
        {
            Name = "Test Item",
            Description = null,
            Price = 10m,
            Category = "main"
        };

        // Act
        await _controller.Create(request);

        // Assert
        var item = await _context.MenuItems.FirstAsync();
        item.Description.Should().BeNull();
    }

    [Fact]
    public async Task Create_SavesToDatabase()
    {
        // Arrange
        var request = new CreateMenuItemRequest
        {
            Name = "New Item",
            Description = "Description",
            Price = 15m,
            Category = "dessert",
            DietaryTags = new[] { "vegan" },
            Allergens = new[] { "nuts" }
        };

        // Act
        await _controller.Create(request);

        // Assert
        var item = await _context.MenuItems.FirstOrDefaultAsync(i => i.Name == "New Item");
        item.Should().NotBeNull();
        item!.Price.Should().Be(15m);
        item.Category.Should().Be("dessert");
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidRequest_ReturnsOk()
    {
        // Arrange
        await SeedTestData();
        var item = await _context.MenuItems.FirstAsync();

        var request = new UpdateMenuItemRequest
        {
            Name = "Updated Name",
            Price = 20m
        };

        // Act
        var result = await _controller.Update(item.Id, request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var request = new UpdateMenuItemRequest { Name = "Updated" };

        // Act
        var result = await _controller.Update(999, request);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_OnlyUpdatesProvidedFields()
    {
        // Arrange
        var item = new MenuItem
        {
            Name = "Original",
            Description = "Original Description",
            Price = 10m,
            Category = "original"
        };
        _context.MenuItems.Add(item);
        await _context.SaveChangesAsync();

        var request = new UpdateMenuItemRequest { Name = "Updated" };

        // Act
        await _controller.Update(item.Id, request);

        // Assert
        var updatedItem = await _context.MenuItems.FindAsync(item.Id);
        updatedItem!.Name.Should().Be("Updated");
        updatedItem.Description.Should().Be("Original Description");
        updatedItem.Price.Should().Be(10m);
        updatedItem.Category.Should().Be("original");
    }

    [Fact]
    public async Task Update_TrimsWhitespaceFromNameAndCategory()
    {
        // Arrange
        var item = new MenuItem
        {
            Name = "Original",
            Price = 10m,
            Category = "original"
        };
        _context.MenuItems.Add(item);
        await _context.SaveChangesAsync();

        var request = new UpdateMenuItemRequest
        {
            Name = "  Updated  ",
            Category = "  NEW  "
        };

        // Act
        await _controller.Update(item.Id, request);

        // Assert
        var updatedItem = await _context.MenuItems.FindAsync(item.Id);
        updatedItem!.Name.Should().Be("Updated");
        updatedItem.Category.Should().Be("new");
    }

    [Fact]
    public async Task Update_WithNullDescription_KeepsOriginalValue()
    {
        // Arrange
        var item = new MenuItem
        {
            Name = "Original",
            Description = "Original Description",
            Price = 10m,
            Category = "main"
        };
        _context.MenuItems.Add(item);
        await _context.SaveChangesAsync();

        var request = new UpdateMenuItemRequest { Description = null };

        // Act
        await _controller.Update(item.Id, request);

        // Assert
        var updatedItem = await _context.MenuItems.FindAsync(item.Id);
        // Null values are not updated, so original description is preserved
        updatedItem!.Description.Should().Be("Original Description");
    }

    [Fact]
    public async Task Update_CanUpdateArrays()
    {
        // Arrange
        var item = new MenuItem
        {
            Name = "Original",
            Price = 10m,
            Category = "main",
            DietaryTags = new[] { "old" },
            Allergens = new[] { "old" }
        };
        _context.MenuItems.Add(item);
        await _context.SaveChangesAsync();

        var request = new UpdateMenuItemRequest
        {
            DietaryTags = new[] { "vegan", "new" },
            Allergens = new[] { "soy" }
        };

        // Act
        await _controller.Update(item.Id, request);

        // Assert
        var updatedItem = await _context.MenuItems.FindAsync(item.Id);
        updatedItem!.DietaryTags.Should().ContainInOrder("vegan", "new");
        updatedItem.Allergens.Should().ContainSingle().Which.Should().Be("soy");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Update_CanToggleAvailability(bool available)
    {
        // Arrange
        var item = new MenuItem
        {
            Name = "Test",
            Price = 10m,
            Category = "main",
            Available = !available
        };
        _context.MenuItems.Add(item);
        await _context.SaveChangesAsync();

        var request = new UpdateMenuItemRequest { Available = available };

        // Act
        await _controller.Update(item.Id, request);

        // Assert
        var updatedItem = await _context.MenuItems.FindAsync(item.Id);
        updatedItem!.Available.Should().Be(available);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithValidId_ReturnsNoContent()
    {
        // Arrange
        await SeedTestData();
        var item = await _context.MenuItems.FirstAsync();

        // Act
        var result = await _controller.Delete(item.Id);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.Delete(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_RemovesFromDatabase()
    {
        // Arrange
        await SeedTestData();
        var item = await _context.MenuItems.FirstAsync();
        var originalCount = await _context.MenuItems.CountAsync();

        // Act
        await _controller.Delete(item.Id);

        // Assert
        var newCount = await _context.MenuItems.CountAsync();
        newCount.Should().Be(originalCount - 1);
        var deletedItem = await _context.MenuItems.FindAsync(item.Id);
        deletedItem.Should().BeNull();
    }

    #endregion

    #region SeedData Tests

    [Fact]
    public async Task SeedData_WithEmptyDatabase_ReturnsOkWithCount()
    {
        // Act
        var result = await _controller.SeedData();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value as dynamic;
        ((int)value.GetType().GetProperty("count")!.GetValue(value)!).Should().Be(4);
    }

    [Fact]
    public async Task SeedData_WithExistingData_ReturnsBadRequest()
    {
        // Arrange
        await SeedTestData();

        // Act
        var result = await _controller.SeedData();

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task SeedData_AddsSampleData()
    {
        // Act
        await _controller.SeedData();

        // Assert
        var count = await _context.MenuItems.CountAsync();
        count.Should().Be(4);

        var categories = await _context.MenuItems.Select(i => i.Category).Distinct().ToListAsync();
        categories.Should().Contain("main");
        categories.Should().Contain("beverage");
    }

    #endregion

    #region GetCategories Tests

    [Fact]
    public async Task GetCategories_WithNoItems_ReturnsEmptyList()
    {
        // Act
        var result = await _controller.GetCategories();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var categories = okResult.Value as IEnumerable<string>;
        categories.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCategories_ReturnsDistinctCategories()
    {
        // Arrange
        await SeedTestData();

        // Act
        var result = await _controller.GetCategories();

        // Assert
        var okResult = result.As<OkObjectResult>();
        var categories = okResult.Value as IEnumerable<string>;
        categories.Should().HaveCount(2);
        categories.Should().Contain(new[] { "main", "beverage" });
    }

    [Fact]
    public async Task GetCategories_ReturnsOnlyDistinctValues()
    {
        // Arrange
        var items = new List<MenuItem>
        {
            new() { Name = "Item 1", Price = 1, Category = "same" },
            new() { Name = "Item 2", Price = 1, Category = "same" },
            new() { Name = "Item 3", Price = 1, Category = "same" }
        };
        _context.MenuItems.AddRange(items);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetCategories();

        // Assert
        var okResult = result.As<OkObjectResult>();
        var categories = okResult.Value as IEnumerable<string>;
        categories.Should().ContainSingle().Which.Should().Be("same");
    }

    #endregion

    private async Task SeedTestData()
    {
        var items = new List<MenuItem>
        {
            new()
            {
                Name = "Classic Burger",
                Description = "A delicious beef burger with cheese",
                Price = 12.99m,
                Category = "main",
                DietaryTags = new[] { "high-protein" },
                Allergens = new[] { "gluten", "dairy" },
                Available = true,
                PreparationTime = 15,
                MaxOrdersPerDay = 50
            },
            new()
            {
                Name = "Caesar Salad",
                Description = "Fresh romaine lettuce with caesar dressing",
                Price = 8.99m,
                Category = "main",
                DietaryTags = new[] { "vegetarian" },
                Allergens = new[] { "dairy", "eggs" },
                Available = true,
                PreparationTime = 10,
                MaxOrdersPerDay = 40
            },
            new()
            {
                Name = "Orange Juice",
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

        _context.MenuItems.AddRange(items);
        await _context.SaveChangesAsync();
    }
}
