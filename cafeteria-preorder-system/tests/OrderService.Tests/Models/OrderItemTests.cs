using Xunit;
using System.ComponentModel.DataAnnotations;
using OrderService.Models;
using FluentAssertions;

namespace OrderService.Tests.Models;

public class OrderItemTests
{
    [Fact]
    public void OrderItem_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var item = new OrderItem();

        // Assert
        item.Id.Should().Be(0);
        item.OrderId.Should().Be(0);
        item.Order.Should().BeNull();
        item.MenuItemId.Should().Be(0);
        item.Quantity.Should().Be(0);
        item.Price.Should().Be(0);
        item.MenuItemName.Should().BeNull();
    }

    [Fact]
    public void OrderItem_CanBeFullyPopulated()
    {
        // Arrange
        var order = new Order { Id = 1 };

        // Act
        var item = new OrderItem
        {
            Id = 1,
            OrderId = 1,
            Order = order,
            MenuItemId = 42,
            Quantity = 3,
            Price = 15.99m,
            MenuItemName = "Gourmet Burger"
        };

        // Assert
        item.Id.Should().Be(1);
        item.OrderId.Should().Be(1);
        item.Order.Should().Be(order);
        item.MenuItemId.Should().Be(42);
        item.Quantity.Should().Be(3);
        item.Price.Should().Be(15.99m);
        item.MenuItemName.Should().Be("Gourmet Burger");
    }

    [Fact]
    public void OrderItem_OrderId_IsRequired()
    {
        // Arrange
        var orderIdProperty = typeof(OrderItem).GetProperty("OrderId")!;
        var attributes = orderIdProperty.GetCustomAttributes(false);

        // Assert
        attributes.Should().ContainSingle(a => a is RequiredAttribute);
    }

    [Fact]
    public void OrderItem_MenuItemId_IsRequired()
    {
        // Arrange
        var menuItemIdProperty = typeof(OrderItem).GetProperty("MenuItemId")!;
        var attributes = menuItemIdProperty.GetCustomAttributes(false);

        // Assert
        attributes.Should().ContainSingle(a => a is RequiredAttribute);
    }

    [Fact]
    public void OrderItem_Quantity_IsRequired()
    {
        // Arrange
        var quantityProperty = typeof(OrderItem).GetProperty("Quantity")!;
        var attributes = quantityProperty.GetCustomAttributes(false);

        // Assert
        attributes.Should().ContainSingle(a => a is RequiredAttribute);
    }

    [Fact]
    public void OrderItem_Price_IsRequired()
    {
        // Arrange
        var priceProperty = typeof(OrderItem).GetProperty("Price")!;
        var attributes = priceProperty.GetCustomAttributes(false);

        // Assert
        attributes.Should().ContainSingle(a => a is RequiredAttribute);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public void OrderItem_Quantity_AcceptsVariousValues(int quantity)
    {
        // Arrange & Act
        var item = new OrderItem { Quantity = quantity };

        // Assert
        item.Quantity.Should().Be(quantity);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1)]
    [InlineData(15.99)]
    [InlineData(999.99)]
    public void OrderItem_Price_AcceptsVariousValues(decimal price)
    {
        // Arrange & Act
        var item = new OrderItem { Price = price };

        // Assert
        item.Price.Should().Be(price);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(9999)]
    public void OrderItem_MenuItemId_AcceptsVariousValues(int menuItemId)
    {
        // Arrange & Act
        var item = new OrderItem { MenuItemId = menuItemId };

        // Assert
        item.MenuItemId.Should().Be(menuItemId);
    }

    [Fact]
    public void OrderItem_WithNullMenuItemName_WorksCorrectly()
    {
        // Arrange & Act
        var item = new OrderItem { MenuItemName = null };

        // Assert
        item.MenuItemName.Should().BeNull();
    }

    [Fact]
    public void OrderItem_MenuItemName_CanBeEmpty()
    {
        // Arrange & Act
        var item = new OrderItem { MenuItemName = "" };

        // Assert
        item.MenuItemName.Should().BeEmpty();
    }

    [Fact]
    public void OrderItem_CanHaveNullOrderReference()
    {
        // Arrange & Act
        var item = new OrderItem { Order = null };

        // Assert
        item.Order.Should().BeNull();
    }
}
