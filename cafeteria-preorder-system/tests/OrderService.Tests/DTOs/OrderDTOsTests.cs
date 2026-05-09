using Xunit;
using OrderService.DTOs;
using FluentAssertions;

namespace OrderService.Tests.DTOs;

public class OrderDTOsTests
{
    #region CreateOrderRequest Tests

    [Fact]
    public void CreateOrderRequest_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var request = new CreateOrderRequest();

        // Assert
        request.Items.Should().BeEmpty();
        request.PickupTime.Should().Be(default);
        request.PickupDate.Should().Be(default);
        request.SpecialInstructions.Should().BeNull();
    }

    [Fact]
    public void CreateOrderRequest_CanBeFullyPopulated()
    {
        // Arrange
        var pickupTime = DateTime.UtcNow.AddHours(2);
        var pickupDate = DateTime.UtcNow.Date;

        // Act
        var request = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest>
            {
                new() { MenuItemId = 1, Quantity = 2, Price = 10m, MenuItemName = "Burger" },
                new() { MenuItemId = 2, Quantity = 1, Price = 5m, MenuItemName = "Soda" }
            },
            PickupTime = pickupTime,
            PickupDate = pickupDate,
            SpecialInstructions = "Extra sauce"
        };

        // Assert
        request.Items.Should().HaveCount(2);
        request.PickupTime.Should().Be(pickupTime);
        request.PickupDate.Should().Be(pickupDate);
        request.SpecialInstructions.Should().Be("Extra sauce");
    }

    [Fact]
    public void CreateOrderRequest_WithNullInstructions_WorksCorrectly()
    {
        // Arrange & Act
        var request = new CreateOrderRequest { SpecialInstructions = null };

        // Assert
        request.SpecialInstructions.Should().BeNull();
    }

    #endregion

    #region OrderItemRequest Tests

    [Fact]
    public void OrderItemRequest_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var item = new OrderItemRequest();

        // Assert
        item.MenuItemId.Should().Be(0);
        item.Quantity.Should().Be(0);
        item.Price.Should().Be(0);
        item.MenuItemName.Should().BeNull();
    }

    [Fact]
    public void OrderItemRequest_CanBeFullyPopulated()
    {
        // Arrange & Act
        var item = new OrderItemRequest
        {
            MenuItemId = 42,
            Quantity = 3,
            Price = 15.99m,
            MenuItemName = "Gourmet Burger"
        };

        // Assert
        item.MenuItemId.Should().Be(42);
        item.Quantity.Should().Be(3);
        item.Price.Should().Be(15.99m);
        item.MenuItemName.Should().Be("Gourmet Burger");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public void OrderItemRequest_Quantity_AcceptsVariousValues(int quantity)
    {
        // Arrange & Act
        var item = new OrderItemRequest { Quantity = quantity };

        // Assert
        item.Quantity.Should().Be(quantity);
    }

    #endregion

    #region UpdateStatusRequest Tests

    [Fact]
    public void UpdateStatusRequest_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var request = new UpdateStatusRequest();

        // Assert
        request.Status.Should().BeEmpty();
    }

    [Theory]
    [InlineData("pending")]
    [InlineData("confirmed")]
    [InlineData("completed")]
    [InlineData("CANCELLED")]
    public void UpdateStatusRequest_AcceptsVariousStatuses(string status)
    {
        // Arrange & Act
        var request = new UpdateStatusRequest { Status = status };

        // Assert
        request.Status.Should().Be(status);
    }

    #endregion

    #region OrderDto Tests

    [Fact]
    public void OrderDto_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var dto = new OrderDto();

        // Assert
        dto.Id.Should().Be(0);
        dto.UserId.Should().Be(0);
        dto.TotalAmount.Should().Be(0);
        dto.PickupTime.Should().Be(default);
        dto.PickupDate.Should().Be(default);
        dto.Status.Should().BeEmpty();
        dto.PaymentStatus.Should().BeEmpty();
        dto.SpecialInstructions.Should().BeNull();
        dto.CreatedAt.Should().Be(default);
        dto.UpdatedAt.Should().BeNull();
        dto.Items.Should().BeEmpty();
    }

    [Fact]
    public void OrderDto_CanBeFullyPopulated()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var updatedAt = DateTime.UtcNow;
        var pickupTime = DateTime.UtcNow.AddHours(2);
        var pickupDate = DateTime.UtcNow.Date;

        // Act
        var dto = new OrderDto
        {
            Id = 1,
            UserId = 42,
            TotalAmount = 55.99m,
            PickupTime = pickupTime,
            PickupDate = pickupDate,
            Status = "confirmed",
            PaymentStatus = "paid",
            SpecialInstructions = "Extra napkins",
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            Items = new List<OrderItemDto>
            {
                new() { Id = 1, MenuItemId = 1, Quantity = 2, Price = 15m, MenuItemName = "Burger" }
            }
        };

        // Assert
        dto.Id.Should().Be(1);
        dto.UserId.Should().Be(42);
        dto.TotalAmount.Should().Be(55.99m);
        dto.Status.Should().Be("confirmed");
        dto.PaymentStatus.Should().Be("paid");
        dto.SpecialInstructions.Should().Be("Extra napkins");
        dto.Items.Should().HaveCount(1);
    }

    #endregion

    #region OrderItemDto Tests

    [Fact]
    public void OrderItemDto_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var item = new OrderItemDto();

        // Assert
        item.Id.Should().Be(0);
        item.MenuItemId.Should().Be(0);
        item.MenuItemName.Should().BeNull();
        item.Quantity.Should().Be(0);
        item.Price.Should().Be(0);
    }

    [Fact]
    public void OrderItemDto_CanBeFullyPopulated()
    {
        // Arrange & Act
        var item = new OrderItemDto
        {
            Id = 1,
            MenuItemId = 42,
            MenuItemName = "Gourmet Burger",
            Quantity = 3,
            Price = 15m
        };

        // Assert
        item.Id.Should().Be(1);
        item.MenuItemId.Should().Be(42);
        item.MenuItemName.Should().Be("Gourmet Burger");
        item.Quantity.Should().Be(3);
        item.Price.Should().Be(15m);
    }

    [Theory]
    [InlineData(1, 10, 10)]
    [InlineData(2, 15, 30)]
    [InlineData(3, 10, 30)]
    [InlineData(1, 0.01, 0.01)]
    public void OrderItemDto_Subtotal_CalculatesCorrectly(int quantity, decimal price, decimal expected)
    {
        // Arrange
        var item = new OrderItemDto
        {
            Quantity = quantity,
            Price = price
        };

        // Act & Assert
        item.Subtotal.Should().Be(expected);
    }

    #endregion

    #region OrderStatsDto Tests

    [Fact]
    public void OrderStatsDto_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var stats = new OrderStatsDto();

        // Assert
        stats.TotalOrders.Should().Be(0);
        stats.PendingOrders.Should().Be(0);
        stats.ConfirmedOrders.Should().Be(0);
        stats.PreparingOrders.Should().Be(0);
        stats.ReadyOrders.Should().Be(0);
        stats.CompletedOrders.Should().Be(0);
        stats.CancelledOrders.Should().Be(0);
        stats.TotalRevenue.Should().Be(0);
    }

    [Fact]
    public void OrderStatsDto_CanBeFullyPopulated()
    {
        // Arrange & Act
        var stats = new OrderStatsDto
        {
            TotalOrders = 100,
            PendingOrders = 10,
            ConfirmedOrders = 15,
            PreparingOrders = 20,
            ReadyOrders = 5,
            CompletedOrders = 45,
            CancelledOrders = 5,
            TotalRevenue = 2500.50m
        };

        // Assert
        stats.TotalOrders.Should().Be(100);
        stats.PendingOrders.Should().Be(10);
        stats.ConfirmedOrders.Should().Be(15);
        stats.PreparingOrders.Should().Be(20);
        stats.ReadyOrders.Should().Be(5);
        stats.CompletedOrders.Should().Be(45);
        stats.CancelledOrders.Should().Be(5);
        stats.TotalRevenue.Should().Be(2500.50m);
    }

    [Fact]
    public void OrderStatsDto_IndividualCounts_CanExceedTotal()
    {
        // This tests that the DTO doesn't enforce business logic
        // Arrange & Act
        var stats = new OrderStatsDto
        {
            TotalOrders = 10,
            PendingOrders = 20, // More than total
            CompletedOrders = 30  // More than total
        };

        // Assert
        stats.TotalOrders.Should().Be(10);
        stats.PendingOrders.Should().Be(20);
        stats.CompletedOrders.Should().Be(30);
    }

    #endregion
}
