using Xunit;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;
using FluentAssertions;

namespace OrderService.Tests.Data;

public class OrderDbContextTests : IDisposable
{
    private readonly OrderDbContext _context;

    public OrderDbContextTests()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new OrderDbContext(options);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public void OrderDbContext_CanBeConstructed()
    {
        // Assert
        _context.Should().NotBeNull();
        _context.Orders.Should().NotBeNull();
    }

    [Fact]
    public async Task OrderDbContext_CanAddAndRetrieveOrder()
    {
        // Arrange
        var order = new Order
        {
            UserId = 1,
            TotalAmount = 25.99m,
            Status = "pending",
            Items = new List<OrderItem>
            {
                new() { MenuItemId = 1, Quantity = 1, Price = 15.99m, MenuItemName = "Burger" },
                new() { MenuItemId = 2, Quantity = 1, Price = 10m, MenuItemName = "Fries" }
            }
        };

        // Act
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Assert
        var retrievedOrder = await _context.Orders.FindAsync(order.Id);
        retrievedOrder.Should().NotBeNull();
        retrievedOrder!.UserId.Should().Be(1);
        retrievedOrder.TotalAmount.Should().Be(25.99m);
    }

    [Fact]
    public async Task OrderDbContext_CanAddOrderWithItems()
    {
        // Arrange
        var order = new Order
        {
            UserId = 1,
            TotalAmount = 35m,
            Status = "pending",
            Items = new List<OrderItem>
            {
                new() { MenuItemId = 1, Quantity = 2, Price = 10m, MenuItemName = "Burger" },
                new() { MenuItemId = 2, Quantity = 1, Price = 15m, MenuItemName = "Salad" }
            }
        };

        // Act
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Assert
        var retrievedOrder = await _context.Orders
            .Include(o => o.Items)
            .FirstAsync();

        retrievedOrder.Items.Should().HaveCount(2);
        retrievedOrder.Items.Should().Contain(i => i.MenuItemName == "Burger" && i.Quantity == 2);
    }

    [Fact]
    public async Task OrderDbContext_CanUpdateOrder()
    {
        // Arrange
        var order = new Order
        {
            UserId = 1,
            TotalAmount = 10m,
            Status = "pending"
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Act
        order.Status = "confirmed";
        order.TotalAmount = 20m;
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();

        // Assert
        var updatedOrder = await _context.Orders.FindAsync(order.Id);
        updatedOrder!.Status.Should().Be("confirmed");
        updatedOrder.TotalAmount.Should().Be(20m);
    }

    [Fact]
    public async Task OrderDbContext_CanDeleteOrder()
    {
        // Arrange
        var order = new Order
        {
            UserId = 1,
            TotalAmount = 10m,
            Status = "pending",
            Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 10m } }
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Act
        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        // Assert
        var deletedOrder = await _context.Orders.FindAsync(order.Id);
        deletedOrder.Should().BeNull();
    }

    [Fact]
    public async Task OrderDbContext_CascadeDelete_RemovesItemsWithOrder()
    {
        // Arrange
        var order = new Order
        {
            UserId = 1,
            TotalAmount = 25m,
            Status = "pending",
            Items = new List<OrderItem>
            {
                new() { MenuItemId = 1, Quantity = 1, Price = 15m, MenuItemName = "Burger" },
                new() { MenuItemId = 2, Quantity = 1, Price = 10m, MenuItemName = "Fries" }
            }
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Act
        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        // Assert - items should also be removed (cascade delete)
        var items = await _context.Set<OrderItem>().Where(i => i.OrderId == order.Id).ToListAsync();
        items.Should().BeEmpty();
    }

    [Fact]
    public async Task OrderDbContext_CanQueryWithStatusFilter()
    {
        // Arrange
        var orders = new List<Order>
        {
            new() { UserId = 1, TotalAmount = 10m, Status = "pending", Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 10m } } },
            new() { UserId = 1, TotalAmount = 15m, Status = "completed", Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 15m } } },
            new() { UserId = 2, TotalAmount = 20m, Status = "pending", Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 20m } } }
        };

        _context.Orders.AddRange(orders);
        await _context.SaveChangesAsync();

        // Act
        var pendingOrders = await _context.Orders.Where(o => o.Status == "pending").ToListAsync();
        var completedOrders = await _context.Orders.Where(o => o.Status == "completed").ToListAsync();

        // Assert
        pendingOrders.Should().HaveCount(2);
        completedOrders.Should().HaveCount(1);
    }

    [Fact]
    public async Task OrderDbContext_CanQueryByUserId()
    {
        // Arrange
        var orders = new List<Order>
        {
            new() { UserId = 1, TotalAmount = 10m, Status = "pending", Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 10m } } },
            new() { UserId = 1, TotalAmount = 15m, Status = "completed", Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 15m } } },
            new() { UserId = 2, TotalAmount = 20m, Status = "pending", Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 20m } } }
        };

        _context.Orders.AddRange(orders);
        await _context.SaveChangesAsync();

        // Act
        var user1Orders = await _context.Orders.Where(o => o.UserId == 1).ToListAsync();

        // Assert
        user1Orders.Should().HaveCount(2);
        user1Orders.All(o => o.UserId == 1).Should().BeTrue();
    }

    [Fact]
    public async Task OrderDbContext_CanSumByStatus()
    {
        // Arrange
        var orders = new List<Order>
        {
            new() { UserId = 1, TotalAmount = 10m, Status = "completed", Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 10m } } },
            new() { UserId = 1, TotalAmount = 20m, Status = "completed", Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 20m } } },
            new() { UserId = 1, TotalAmount = 30m, Status = "pending", Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 30m } } }
        };

        _context.Orders.AddRange(orders);
        await _context.SaveChangesAsync();

        // Act
        var completedRevenue = await _context.Orders
            .Where(o => o.Status == "completed")
            .SumAsync(o => o.TotalAmount);

        // Assert
        completedRevenue.Should().Be(30m);
    }

    [Fact]
    public async Task OrderDbContext_CanCountByStatus()
    {
        // Arrange
        var orders = new List<Order>
        {
            new() { UserId = 1, TotalAmount = 10m, Status = "pending", Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 10m } } },
            new() { UserId = 1, TotalAmount = 15m, Status = "pending", Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 15m } } },
            new() { UserId = 2, TotalAmount = 20m, Status = "completed", Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 20m } } }
        };

        _context.Orders.AddRange(orders);
        await _context.SaveChangesAsync();

        // Act
        var pendingCount = await _context.Orders.CountAsync(o => o.Status == "pending");
        var completedCount = await _context.Orders.CountAsync(o => o.Status == "completed");
        var totalCount = await _context.Orders.CountAsync();

        // Assert
        pendingCount.Should().Be(2);
        completedCount.Should().Be(1);
        totalCount.Should().Be(3);
    }

    [Fact]
    public async Task OrderDbContext_FindAsync_ReturnsNullForNonexistentId()
    {
        // Act
        var result = await _context.Orders.FindAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task OrderDbContext_IncludeItems_RetrievesRelatedData()
    {
        // Arrange
        var order = new Order
        {
            UserId = 1,
            TotalAmount = 35m,
            Status = "pending",
            Items = new List<OrderItem>
            {
                new() { MenuItemId = 1, Quantity = 2, Price = 10m, MenuItemName = "Burger" },
                new() { MenuItemId = 2, Quantity = 1, Price = 15m, MenuItemName = "Salad" }
            }
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Act
        var retrievedOrder = await _context.Orders
            .Include(o => o.Items)
            .FirstAsync();

        // Assert
        retrievedOrder.Items.Should().HaveCount(2);
        retrievedOrder.Items.Sum(i => i.Price * i.Quantity).Should().Be(35m);
    }

    [Fact]
    public async Task OrderDbContext_CanOrderByCreatedAt()
    {
        // Arrange
        var orders = new List<Order>
        {
            new() { UserId = 1, TotalAmount = 10m, Status = "pending", CreatedAt = DateTime.UtcNow.AddDays(-2), Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 10m } } },
            new() { UserId = 1, TotalAmount = 20m, Status = "pending", CreatedAt = DateTime.UtcNow, Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 20m } } },
            new() { UserId = 1, TotalAmount = 15m, Status = "pending", CreatedAt = DateTime.UtcNow.AddDays(-1), Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 15m } } }
        };

        _context.Orders.AddRange(orders);
        await _context.SaveChangesAsync();

        // Act
        var orderedOrders = await _context.Orders
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => o.TotalAmount)
            .ToListAsync();

        // Assert
        orderedOrders.Should().ContainInOrder(20m, 15m, 10m);
    }

    [Fact]
    public async Task OrderDbContext_CanFilterByPaymentStatus()
    {
        // Arrange
        var orders = new List<Order>
        {
            new() { UserId = 1, TotalAmount = 10m, Status = "completed", PaymentStatus = "paid", Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 10m } } },
            new() { UserId = 1, TotalAmount = 15m, Status = "pending", PaymentStatus = "unpaid", Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 15m } } },
            new() { UserId = 2, TotalAmount = 20m, Status = "cancelled", PaymentStatus = "refunded", Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 20m } } }
        };

        _context.Orders.AddRange(orders);
        await _context.SaveChangesAsync();

        // Act
        var paidOrders = await _context.Orders.Where(o => o.PaymentStatus == "paid").ToListAsync();
        var unpaidOrders = await _context.Orders.Where(o => o.PaymentStatus == "unpaid").ToListAsync();

        // Assert
        paidOrders.Should().HaveCount(1);
        unpaidOrders.Should().HaveCount(1);
    }
}
