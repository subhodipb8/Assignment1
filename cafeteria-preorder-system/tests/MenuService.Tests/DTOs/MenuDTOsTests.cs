using Xunit;
using MenuService.DTOs;
using FluentAssertions;

namespace MenuService.Tests.DTOs;

public class MenuDTOsTests
{
    #region CreateMenuItemRequest Tests

    [Fact]
    public void CreateMenuItemRequest_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var request = new CreateMenuItemRequest();

        // Assert
        request.Name.Should().BeEmpty();
        request.Description.Should().BeNull();
        request.Price.Should().Be(0);
        request.Category.Should().BeEmpty();
        request.DietaryTags.Should().BeNull();
        request.Allergens.Should().BeNull();
        request.Available.Should().BeTrue();
        request.PreparationTime.Should().Be(15);
        request.MaxOrdersPerDay.Should().Be(100);
    }

    [Fact]
    public void CreateMenuItemRequest_CanBeFullyPopulated()
    {
        // Arrange & Act
        var request = new CreateMenuItemRequest
        {
            Name = "Test Item",
            Description = "Test Description",
            Price = 15.99m,
            Category = "main",
            DietaryTags = new[] { "vegan", "gluten-free" },
            Allergens = new[] { "soy" },
            Available = false,
            PreparationTime = 25,
            MaxOrdersPerDay = 50
        };

        // Assert
        request.Name.Should().Be("Test Item");
        request.Description.Should().Be("Test Description");
        request.Price.Should().Be(15.99m);
        request.Category.Should().Be("main");
        request.DietaryTags.Should().ContainInOrder("vegan", "gluten-free");
        request.Allergens.Should().ContainSingle().Which.Should().Be("soy");
        request.Available.Should().BeFalse();
        request.PreparationTime.Should().Be(25);
        request.MaxOrdersPerDay.Should().Be(50);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(999)]
    public void CreateMenuItemRequest_PreparationTime_AcceptsVariousValues(int minutes)
    {
        // Arrange & Act
        var request = new CreateMenuItemRequest { PreparationTime = minutes };

        // Assert
        request.PreparationTime.Should().Be(minutes);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(0)]
    public void CreateMenuItemRequest_MaxOrdersPerDay_AcceptsVariousValues(int max)
    {
        // Arrange & Act
        var request = new CreateMenuItemRequest { MaxOrdersPerDay = max };

        // Assert
        request.MaxOrdersPerDay.Should().Be(max);
    }

    #endregion

    #region UpdateMenuItemRequest Tests

    [Fact]
    public void UpdateMenuItemRequest_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var request = new UpdateMenuItemRequest();

        // Assert
        request.Name.Should().BeNull();
        request.Description.Should().BeNull();
        request.Price.Should().BeNull();
        request.Category.Should().BeNull();
        request.DietaryTags.Should().BeNull();
        request.Allergens.Should().BeNull();
        request.Available.Should().BeNull();
        request.PreparationTime.Should().BeNull();
        request.MaxOrdersPerDay.Should().BeNull();
    }

    [Fact]
    public void UpdateMenuItemRequest_CanBeFullyPopulated()
    {
        // Arrange & Act
        var request = new UpdateMenuItemRequest
        {
            Name = "Updated Name",
            Description = "Updated Description",
            Price = 20.99m,
            Category = "dessert",
            DietaryTags = new[] { "vegetarian" },
            Allergens = new[] { "nuts" },
            Available = false,
            PreparationTime = 30,
            MaxOrdersPerDay = 75
        };

        // Assert
        request.Name.Should().Be("Updated Name");
        request.Description.Should().Be("Updated Description");
        request.Price.Should().Be(20.99m);
        request.Category.Should().Be("dessert");
        request.DietaryTags.Should().ContainSingle().Which.Should().Be("vegetarian");
        request.Allergens.Should().ContainSingle().Which.Should().Be("nuts");
        request.Available.Should().BeFalse();
        request.PreparationTime.Should().Be(30);
        request.MaxOrdersPerDay.Should().Be(75);
    }

    [Fact]
    public void UpdateMenuItemRequest_AllowsPartialUpdates()
    {
        // Arrange & Act
        var request = new UpdateMenuItemRequest { Name = "Only Name" };

        // Assert
        request.Name.Should().Be("Only Name");
        request.Description.Should().BeNull();
        request.Price.Should().BeNull();
        request.Available.Should().BeNull();
    }

    [Fact]
    public void UpdateMenuItemRequest_CanUpdateSingleProperty()
    {
        // Arrange & Act
        var request = new UpdateMenuItemRequest { Available = true };

        // Assert
        request.Available.Should().BeTrue();
        request.Name.Should().BeNull();
        request.Price.Should().BeNull();
    }

    #endregion

    #region MenuItemDto Tests

    [Fact]
    public void MenuItemDto_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var dto = new MenuItemDto();

        // Assert
        dto.Id.Should().Be(0);
        dto.Name.Should().BeEmpty();
        dto.Description.Should().BeNull();
        dto.Price.Should().Be(0);
        dto.Category.Should().BeEmpty();
        dto.DietaryTags.Should().BeNull();
        dto.Allergens.Should().BeNull();
        dto.Available.Should().BeFalse();
        dto.PreparationTime.Should().Be(0);
        dto.MaxOrdersPerDay.Should().Be(0);
        dto.OrdersToday.Should().Be(0);
        dto.CreatedAt.Should().Be(default);
    }

    [Fact]
    public void MenuItemDto_CanBeFullyPopulated()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddDays(-1);

        // Act
        var dto = new MenuItemDto
        {
            Id = 1,
            Name = "Test Item",
            Description = "Test Description",
            Price = 15.99m,
            Category = "main",
            DietaryTags = new[] { "vegan" },
            Allergens = new[] { "soy" },
            Available = true,
            PreparationTime = 20,
            MaxOrdersPerDay = 50,
            OrdersToday = 10,
            CreatedAt = createdAt
        };

        // Assert
        dto.Id.Should().Be(1);
        dto.Name.Should().Be("Test Item");
        dto.Description.Should().Be("Test Description");
        dto.Price.Should().Be(15.99m);
        dto.Category.Should().Be("main");
        dto.DietaryTags.Should().ContainSingle().Which.Should().Be("vegan");
        dto.Allergens.Should().ContainSingle().Which.Should().Be("soy");
        dto.Available.Should().BeTrue();
        dto.PreparationTime.Should().Be(20);
        dto.MaxOrdersPerDay.Should().Be(50);
        dto.OrdersToday.Should().Be(10);
        dto.CreatedAt.Should().Be(createdAt);
    }

    #endregion

    #region MenuFilterRequest Tests

    [Fact]
    public void MenuFilterRequest_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var request = new MenuFilterRequest();

        // Assert
        request.Category.Should().BeNull();
        request.Search.Should().BeNull();
        request.Available.Should().BeNull();
        request.DietaryTags.Should().BeNull();
    }

    [Fact]
    public void MenuFilterRequest_CanBeFullyPopulated()
    {
        // Arrange & Act
        var request = new MenuFilterRequest
        {
            Category = "main",
            Search = "burger",
            Available = true,
            DietaryTags = new[] { "vegan", "gluten-free" }
        };

        // Assert
        request.Category.Should().Be("main");
        request.Search.Should().Be("burger");
        request.Available.Should().BeTrue();
        request.DietaryTags.Should().ContainInOrder("vegan", "gluten-free");
    }

    [Fact]
    public void MenuFilterRequest_CanFilterBySingleDietaryTag()
    {
        // Arrange & Act
        var request = new MenuFilterRequest
        {
            DietaryTags = new[] { "vegetarian" }
        };

        // Assert
        request.DietaryTags.Should().ContainSingle().Which.Should().Be("vegetarian");
    }

    [Fact]
    public void MenuFilterRequest_WithEmptyDietaryTags_WorksCorrectly()
    {
        // Arrange & Act
        var request = new MenuFilterRequest { DietaryTags = Array.Empty<string>() };

        // Assert
        request.DietaryTags.Should().BeEmpty();
    }

    #endregion
}
