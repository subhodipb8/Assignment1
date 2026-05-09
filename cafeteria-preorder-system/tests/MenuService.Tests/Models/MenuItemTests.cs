using Xunit;
using System.ComponentModel.DataAnnotations;
using MenuService.Models;
using FluentAssertions;

namespace MenuService.Tests.Models;

public class MenuItemTests
{
    [Fact]
    public void MenuItem_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var item = new MenuItem();

        // Assert
        item.Id.Should().Be(0);
        item.Name.Should().BeEmpty();
        item.Description.Should().BeNull();
        item.Price.Should().Be(0);
        item.Category.Should().BeEmpty();
        item.DietaryTags.Should().BeNull();
        item.Allergens.Should().BeNull();
        item.Available.Should().BeTrue();
        item.PreparationTime.Should().Be(0);
        item.MaxOrdersPerDay.Should().Be(100);
        item.OrdersToday.Should().Be(0);
        item.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MenuItem_CanBeFullyPopulated()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddDays(-1);

        // Act
        var item = new MenuItem
        {
            Id = 1,
            Name = "Test Item",
            Description = "Test Description",
            Price = 15.99m,
            Category = "main",
            DietaryTags = new[] { "vegan", "gluten-free" },
            Allergens = new[] { "soy" },
            Available = false,
            PreparationTime = 25,
            MaxOrdersPerDay = 50,
            OrdersToday = 10,
            CreatedAt = createdAt
        };

        // Assert
        item.Id.Should().Be(1);
        item.Name.Should().Be("Test Item");
        item.Description.Should().Be("Test Description");
        item.Price.Should().Be(15.99m);
        item.Category.Should().Be("main");
        item.DietaryTags.Should().ContainInOrder("vegan", "gluten-free");
        item.Allergens.Should().ContainSingle().Which.Should().Be("soy");
        item.Available.Should().BeFalse();
        item.PreparationTime.Should().Be(25);
        item.MaxOrdersPerDay.Should().Be(50);
        item.OrdersToday.Should().Be(10);
        item.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void MenuItem_Name_HasRequiredAndMaxLengthAttributes()
    {
        // Arrange
        var nameProperty = typeof(MenuItem).GetProperty("Name")!;
        var attributes = nameProperty.GetCustomAttributes(false);

        // Assert
        attributes.Should().ContainSingle(a => a is RequiredAttribute);
        attributes.Should().ContainSingle(a => a is MaxLengthAttribute);
        var maxLength = attributes.OfType<MaxLengthAttribute>().First();
        maxLength.Length.Should().Be(100);
    }

    [Fact]
    public void MenuItem_Price_HasRequiredAttribute()
    {
        // Arrange
        var priceProperty = typeof(MenuItem).GetProperty("Price")!;
        var attributes = priceProperty.GetCustomAttributes(false);

        // Assert
        attributes.Should().ContainSingle(a => a is RequiredAttribute);
    }

    [Fact]
    public void MenuItem_Category_HasMaxLength50()
    {
        // Arrange
        var categoryProperty = typeof(MenuItem).GetProperty("Category")!;
        var maxLength = categoryProperty.GetCustomAttributes(typeof(MaxLengthAttribute), false)
            .Cast<MaxLengthAttribute>()
            .First();

        // Assert
        maxLength.Length.Should().Be(50);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.01)]
    [InlineData(9999.99)]
    [InlineData(1000000)]
    public void MenuItem_Price_AcceptsVariousValues(decimal price)
    {
        // Arrange & Act
        var item = new MenuItem { Price = price };

        // Assert
        item.Price.Should().Be(price);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(60)]
    [InlineData(1440)]
    public void MenuItem_PreparationTime_AcceptsVariousValues(int minutes)
    {
        // Arrange & Act
        var item = new MenuItem { PreparationTime = minutes };

        // Assert
        item.PreparationTime.Should().Be(minutes);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(0)]
    public void MenuItem_MaxOrdersPerDay_AcceptsVariousValues(int max)
    {
        // Arrange & Act
        var item = new MenuItem { MaxOrdersPerDay = max };

        // Assert
        item.MaxOrdersPerDay.Should().Be(max);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public void MenuItem_OrdersToday_AcceptsVariousValues(int orders)
    {
        // Arrange & Act
        var item = new MenuItem { OrdersToday = orders };

        // Assert
        item.OrdersToday.Should().Be(orders);
    }

    [Fact]
    public void MenuItem_WithEmptyArrays_WorksCorrectly()
    {
        // Arrange & Act
        var item = new MenuItem
        {
            DietaryTags = Array.Empty<string>(),
            Allergens = Array.Empty<string>()
        };

        // Assert
        item.DietaryTags.Should().BeEmpty();
        item.Allergens.Should().BeEmpty();
    }

    [Fact]
    public void MenuItem_WithNullDescription_WorksCorrectly()
    {
        // Arrange & Act
        var item = new MenuItem { Description = null };

        // Assert
        item.Description.Should().BeNull();
    }

    [Fact]
    public void MenuItem_WithEmptyDescription_WorksCorrectly()
    {
        // Arrange & Act
        var item = new MenuItem { Description = "" };

        // Assert
        item.Description.Should().BeEmpty();
    }

    [Fact]
    public void MenuItem_Available_CanBeToggled()
    {
        // Arrange
        var item = new MenuItem { Available = true };

        // Act
        item.Available = false;

        // Assert
        item.Available.Should().BeFalse();
    }
}
