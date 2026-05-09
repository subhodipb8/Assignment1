using Xunit;
using System.ComponentModel.DataAnnotations;
using OrderService.Models;
using FluentAssertions;

namespace OrderService.Tests.Models;

public class OrderTests
{
    [Fact]
    public void Order_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var order = new Order();

        // Assert
        order.Id.Should().Be(0);
        order.UserId.Should().Be(0);
        order.TotalAmount.Should().Be(0);
        order.PickupTime.Should().Be(default);
        order.PickupDate.Should().Be(default);
        order.Status.Should().Be("pending");
        order.PaymentStatus.Should().Be("unpaid");
        order.SpecialInstructions.Should().BeNull();
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        order.UpdatedAt.Should().BeNull();
        order.Items.Should().BeEmpty();
    }

    [Fact]
    public void Order_CanBeFullyPopulated()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var pickupTime = DateTime.UtcNow.AddHours(2);
        var pickupDate = DateTime.UtcNow.Date;

        // Act
        var order = new Order
        {
            Id = 1,
            UserId = 42,
            TotalAmount = 55.99m,
            PickupTime = pickupTime,
            PickupDate = pickupDate,
            Status = "confirmed",
            PaymentStatus = "paid",
            SpecialInstructions = "Extra napkins please",
            CreatedAt = createdAt,
            UpdatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>
            {
                new() { Id = 1, MenuItemId = 1, Quantity = 2, Price = 15.99m, MenuItemName = "Burger" },
                new() { Id = 2, MenuItemId = 2, Quantity = 1, Price = 24.01m, MenuItemName = "Salad" }
            }
        };

        // Assert
        order.Id.Should().Be(1);
        order.UserId.Should().Be(42);
        order.TotalAmount.Should().Be(55.99m);
        order.PickupTime.Should().Be(pickupTime);
        order.PickupDate.Should().Be(pickupDate);
        order.Status.Should().Be("confirmed");
        order.PaymentStatus.Should().Be("paid");
        order.SpecialInstructions.Should().Be("Extra napkins please");
        order.CreatedAt.Should().Be(createdAt);
        order.UpdatedAt.Should().NotBeNull();
        order.Items.Should().HaveCount(2);
    }

    [Fact]
    public void Order_UserId_IsRequired()
    {
        // Arrange
        var userIdProperty = typeof(Order).GetProperty("UserId")!;
        var attributes = userIdProperty.GetCustomAttributes(false);

        // Assert
        attributes.Should().ContainSingle(a => a is RequiredAttribute);
    }

    [Fact]
    public void Order_TotalAmount_IsRequired()
    {
        // Arrange
        var totalAmountProperty = typeof(Order).GetProperty("TotalAmount")!;
        var attributes = totalAmountProperty.GetCustomAttributes(false);

        // Assert
        attributes.Should().ContainSingle(a => a is RequiredAttribute);
    }

    [Fact]
    public void Order_Status_HasMaxLength20()
    {
        // Arrange
        var statusProperty = typeof(Order).GetProperty("Status")!;
        var maxLength = statusProperty.GetCustomAttributes(typeof(MaxLengthAttribute), false)
            .Cast<MaxLengthAttribute>()
            .First();

        // Assert
        maxLength.Length.Should().Be(20);
    }

    [Fact]
    public void Order_PaymentStatus_HasMaxLength20()
    {
        // Arrange
        var paymentStatusProperty = typeof(Order).GetProperty("PaymentStatus")!;
        var maxLength = paymentStatusProperty.GetCustomAttributes(typeof(MaxLengthAttribute), false)
            .Cast<MaxLengthAttribute>()
            .First();

        // Assert
        maxLength.Length.Should().Be(20);
    }

    [Theory]
    [InlineData("pending")]
    [InlineData("confirmed")]
    [InlineData("preparing")]
    [InlineData("ready")]
    [InlineData("completed")]
    [InlineData("cancelled")]
    public void Order_Status_AcceptsValidStatuses(string status)
    {
        // Arrange & Act
        var order = new Order { Status = status };

        // Assert
        order.Status.Should().Be(status);
    }

    [Theory]
    [InlineData("unpaid")]
    [InlineData("paid")]
    [InlineData("refunded")]
    public void Order_PaymentStatus_AcceptsValidStatuses(string status)
    {
        // Arrange & Act
        var order = new Order { PaymentStatus = status };

        // Assert
        order.PaymentStatus.Should().Be(status);
    }

    [Fact]
    public void Order_Items_CanBeEmpty()
    {
        // Arrange & Act
        var order = new Order { Items = new List<OrderItem>() };

        // Assert
        order.Items.Should().BeEmpty();
    }

    [Fact]
    public void Order_WithNullSpecialInstructions_WorksCorrectly()
    {
        // Arrange & Act
        var order = new Order { SpecialInstructions = null };

        // Assert
        order.SpecialInstructions.Should().BeNull();
    }

    [Fact]
    public void Order_UpdatedAt_CanBeNull()
    {
        // Arrange & Act
        var order = new Order { UpdatedAt = null };

        // Assert
        order.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Order_UpdatedAt_CanBeSet()
    {
        // Arrange
        var updatedAt = DateTime.UtcNow;

        // Act
        var order = new Order { UpdatedAt = updatedAt };

        // Assert
        order.UpdatedAt.Should().Be(updatedAt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.01)]
    [InlineData(9999.99)]
    [InlineData(1000000)]
    public void Order_TotalAmount_AcceptsVariousValues(decimal amount)
    {
        // Arrange & Act
        var order = new Order { TotalAmount = amount };

        // Assert
        order.TotalAmount.Should().Be(amount);
    }
}
