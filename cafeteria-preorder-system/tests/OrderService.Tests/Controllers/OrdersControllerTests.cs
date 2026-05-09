using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Controllers;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace OrderService.Tests.Controllers;

public class OrdersControllerTests : IDisposable
{
    private readonly OrderDbContext _context;
    private readonly OrdersController _controller;

    public OrdersControllerTests()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new OrderDbContext(options);
        _controller = new OrdersController(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private void SetupUserContext(string userId, string? role = null)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Add("X-User-Id", userId);
        if (role != null)
        {
            httpContext.Request.Headers.Add("X-User-Role", role);
        }
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    private void ClearUserContext()
    {
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
    }

    #region GetOrders Tests

    [Fact]
    public async Task GetOrders_WithoutUserId_ReturnsUnauthorized()
    {
        // Arrange
        ClearUserContext();

        // Act
        var result = await _controller.GetOrders(null);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task GetOrders_AsRegularUser_ReturnsOnlyOwnOrders()
    {
        // Arrange
        await SeedTestOrders();
        SetupUserContext("1", "student");

        // Act
        var result = await _controller.GetOrders(null);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var orders = okResult.Value as IEnumerable<OrderDto>;
        orders.Should().HaveCount(2);
        orders!.All(o => o.UserId == 1).Should().BeTrue();
    }

    [Theory]
    [InlineData("admin")]
    [InlineData("canteen")]
    public async Task GetOrders_AsAdmin_ReturnsAllOrders(string role)
    {
        // Arrange
        await SeedTestOrders();
        SetupUserContext("2", role);

        // Act
        var result = await _controller.GetOrders(null);

        // Assert
        var okResult = result.As<OkObjectResult>();
        var orders = okResult.Value as IEnumerable<OrderDto>;
        orders.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetOrders_WithStatusFilter_ReturnsFilteredOrders()
    {
        // Arrange
        await SeedTestOrders();
        SetupUserContext("1", "student");

        // Act
        var result = await _controller.GetOrders("pending");

        // Assert
        var okResult = result.As<OkObjectResult>();
        var orders = okResult.Value as IEnumerable<OrderDto>;
        orders.Should().AllSatisfy(o => o.Status.Should().Be("pending"));
    }

    [Fact]
    public async Task GetOrders_WithInvalidStatus_ReturnsEmpty()
    {
        // Arrange
        await SeedTestOrders();
        SetupUserContext("1", "student");

        // Act
        var result = await _controller.GetOrders("nonexistent");

        // Assert
        var okResult = result.As<OkObjectResult>();
        var orders = okResult.Value as IEnumerable<OrderDto>;
        orders.Should().BeEmpty();
    }

    [Fact]
    public async Task GetOrders_ResultsAreOrderedByCreatedAtDescending()
    {
        // Arrange
        var orders = new List<Order>
        {
            new()
            {
                UserId = 1,
                TotalAmount = 10m,
                Status = "pending",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 10m, MenuItemName = "Item" } }
            },
            new()
            {
                UserId = 1,
                TotalAmount = 20m,
                Status = "pending",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 20m, MenuItemName = "Item" } }
            },
            new()
            {
                UserId = 1,
                TotalAmount = 30m,
                Status = "pending",
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 30m, MenuItemName = "Item" } }
            }
        };

        _context.Orders.AddRange(orders);
        await _context.SaveChangesAsync();

        SetupUserContext("1", "student");

        // Act
        var result = await _controller.GetOrders(null);

        // Assert
        var okResult = result.As<OkObjectResult>();
        var resultOrders = okResult.Value as IEnumerable<OrderDto>;
        var amounts = resultOrders!.Select(o => o.TotalAmount).ToList();
        amounts.Should().ContainInOrder(30m, 20m, 10m);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ReturnsOrder()
    {
        // Arrange
        await SeedTestOrders();
        var order = await _context.Orders.FirstAsync();

        // Act
        var result = await _controller.GetById(order.Id);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = okResult.Value.Should().BeOfType<OrderDto>().Subject;
        dto.Id.Should().Be(order.Id);
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
    public async Task GetById_IncludesOrderItems()
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
                new() { MenuItemId = 2, Quantity = 1, Price = 15m, MenuItemName = "Fries" }
            }
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetById(order.Id);

        // Assert
        var okResult = result.As<OkObjectResult>();
        var dto = okResult.Value.As<OrderDto>();
        dto.Items.Should().HaveCount(2);
        dto.Items.Should().Contain(i => i.MenuItemName == "Burger" && i.Quantity == 2);
        dto.Items.Should().Contain(i => i.MenuItemName == "Fries" && i.Quantity == 1);
    }

    [Fact]
    public async Task GetById_CalculatesSubtotalsCorrectly()
    {
        // Arrange
        var order = new Order
        {
            UserId = 1,
            TotalAmount = 35m,
            Status = "pending",
            Items = new List<OrderItem>
            {
                new() { MenuItemId = 1, Quantity = 3, Price = 10m, MenuItemName = "Item" }
            }
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetById(order.Id);

        // Assert
        var okResult = result.As<OkObjectResult>();
        var dto = okResult.Value.As<OrderDto>();
        dto.Items.First().Subtotal.Should().Be(30m);
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreated()
    {
        // Arrange
        SetupUserContext("1");

        var request = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest>
            {
                new() { MenuItemId = 1, Quantity = 2, Price = 10m, MenuItemName = "Burger" }
            },
            PickupTime = DateTime.UtcNow.AddHours(2),
            PickupDate = DateTime.UtcNow.Date
        };

        // Act
        var result = await _controller.Create(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Create_WithoutUserId_ReturnsUnauthorized()
    {
        // Arrange
        ClearUserContext();

        var request = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest> { new() { MenuItemId = 1, Quantity = 1, Price = 10m } },
            PickupTime = DateTime.UtcNow.AddHours(2),
            PickupDate = DateTime.UtcNow.Date
        };

        // Act
        var result = await _controller.Create(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Create_WithEmptyItems_ReturnsBadRequest()
    {
        // Arrange
        SetupUserContext("1");

        var request = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest>(),
            PickupTime = DateTime.UtcNow.AddHours(2),
            PickupDate = DateTime.UtcNow.Date
        };

        // Act
        var result = await _controller.Create(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_WithNullItems_ReturnsBadRequest()
    {
        // Arrange
        SetupUserContext("1");

        var request = new CreateOrderRequest
        {
            Items = null!,
            PickupTime = DateTime.UtcNow.AddHours(2),
            PickupDate = DateTime.UtcNow.Date
        };

        // Act
        var result = await _controller.Create(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_WithPastPickupDate_ReturnsBadRequest()
    {
        // Arrange
        SetupUserContext("1");

        var request = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest> { new() { MenuItemId = 1, Quantity = 1, Price = 10m } },
            PickupTime = DateTime.UtcNow.AddHours(2),
            PickupDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = await _controller.Create(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CalculatesTotalAmountCorrectly()
    {
        // Arrange
        SetupUserContext("1");

        var request = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest>
            {
                new() { MenuItemId = 1, Quantity = 2, Price = 10m, MenuItemName = "Burger" },
                new() { MenuItemId = 2, Quantity = 1, Price = 15m, MenuItemName = "Fries" },
                new() { MenuItemId = 3, Quantity = 3, Price = 5m, MenuItemName = "Soda" }
            },
            PickupTime = DateTime.UtcNow.AddHours(2),
            PickupDate = DateTime.UtcNow.Date
        };

        // Act
        var result = await _controller.Create(request);

        // Assert
        var createdResult = result.As<CreatedAtActionResult>();
        var dto = createdResult.Value.As<OrderDto>();
        dto.TotalAmount.Should().Be(50m); // (2*10) + (1*15) + (3*5) = 20 + 15 + 15 = 50
    }

    [Fact]
    public async Task Create_SetsStatusToPending()
    {
        // Arrange
        SetupUserContext("1");

        var request = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest> { new() { MenuItemId = 1, Quantity = 1, Price = 10m } },
            PickupTime = DateTime.UtcNow.AddHours(2),
            PickupDate = DateTime.UtcNow.Date
        };

        // Act
        var result = await _controller.Create(request);

        // Assert
        var createdResult = result.As<CreatedAtActionResult>();
        var dto = createdResult.Value.As<OrderDto>();
        dto.Status.Should().Be("pending");
        dto.PaymentStatus.Should().Be("unpaid");
    }

    [Fact]
    public async Task Create_SavesToDatabase()
    {
        // Arrange
        SetupUserContext("1");

        var request = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest> { new() { MenuItemId = 1, Quantity = 2, Price = 10m, MenuItemName = "Burger" } },
            PickupTime = DateTime.UtcNow.AddHours(2),
            PickupDate = DateTime.UtcNow.Date
        };

        // Act
        await _controller.Create(request);

        // Assert
        var order = await _context.Orders.FirstOrDefaultAsync();
        order.Should().NotBeNull();
        order!.UserId.Should().Be(1);
        order.TotalAmount.Should().Be(20m);
        order.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Create_WithSpecialInstructions_SavesInstructions()
    {
        // Arrange
        SetupUserContext("1");

        var request = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest> { new() { MenuItemId = 1, Quantity = 1, Price = 10m } },
            PickupTime = DateTime.UtcNow.AddHours(2),
            PickupDate = DateTime.UtcNow.Date,
            SpecialInstructions = "Extra sauce, no onions"
        };

        // Act
        var result = await _controller.Create(request);

        // Assert
        var createdResult = result.As<CreatedAtActionResult>();
        var dto = createdResult.Value.As<OrderDto>();
        dto.SpecialInstructions.Should().Be("Extra sauce, no onions");
    }

    [Fact]
    public async Task Create_NormalizesUtcTime()
    {
        // Arrange
        SetupUserContext("1");

        var localTime = DateTime.Now; // Not UTC
        var localDate = DateTime.Now.Date;

        var request = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest> { new() { MenuItemId = 1, Quantity = 1, Price = 10m } },
            PickupTime = localTime,
            PickupDate = localDate
        };

        // Act
        var result = await _controller.Create(request);

        // Assert
        var createdResult = result.As<CreatedAtActionResult>();
        var dto = createdResult.Value.As<OrderDto>();
        dto.PickupTime.Kind.Should().Be(DateTimeKind.Utc);
        dto.PickupDate.Kind.Should().Be(DateTimeKind.Utc);
    }

    #endregion

    #region UpdateStatus Tests

    [Fact]
    public async Task UpdateStatus_WithValidStatus_ReturnsOk()
    {
        // Arrange
        await SeedTestOrders();
        var order = await _context.Orders.FirstAsync();

        var request = new UpdateStatusRequest { Status = "confirmed" };

        // Act
        var result = await _controller.UpdateStatus(order.Id, request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateStatus_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var request = new UpdateStatusRequest { Status = "confirmed" };

        // Act
        var result = await _controller.UpdateStatus(999, request);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Theory]
    [InlineData("pending")]
    [InlineData("confirmed")]
    [InlineData("preparing")]
    [InlineData("ready")]
    [InlineData("completed")]
    [InlineData("cancelled")]
    public async Task UpdateStatus_WithValidStatuses_Succeeds(string status)
    {
        // Arrange
        await SeedTestOrders();
        var order = await _context.Orders.FirstAsync();

        var request = new UpdateStatusRequest { Status = status };

        // Act
        var result = await _controller.UpdateStatus(order.Id, request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("done")]
    [InlineData("")]
    public async Task UpdateStatus_WithInvalidStatus_ReturnsBadRequest(string status)
    {
        // Arrange
        await SeedTestOrders();
        var order = await _context.Orders.FirstAsync();

        var request = new UpdateStatusRequest { Status = status };

        // Act
        var result = await _controller.UpdateStatus(order.Id, request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateStatus_WhenCompleted_SetsPaymentStatusToPaid()
    {
        // Arrange
        await SeedTestOrders();
        var order = await _context.Orders.FirstAsync();

        var request = new UpdateStatusRequest { Status = "completed" };

        // Act
        var result = await _controller.UpdateStatus(order.Id, request);

        // Assert
        var okResult = result.As<OkObjectResult>();
        var dto = okResult.Value.As<OrderDto>();
        dto.Status.Should().Be("completed");
        dto.PaymentStatus.Should().Be("paid");
    }

    [Fact]
    public async Task UpdateStatus_IsCaseInsensitive()
    {
        // Arrange
        await SeedTestOrders();
        var order = await _context.Orders.FirstAsync();

        var request = new UpdateStatusRequest { Status = "CONFIRMED" };

        // Act
        var result = await _controller.UpdateStatus(order.Id, request);

        // Assert
        var okResult = result.As<OkObjectResult>();
        var dto = okResult.Value.As<OrderDto>();
        dto.Status.Should().Be("confirmed");
    }

    [Fact]
    public async Task UpdateStatus_UpdatesUpdatedAt()
    {
        // Arrange
        var order = new Order
        {
            UserId = 1,
            TotalAmount = 10m,
            Status = "pending",
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 10m } }
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var request = new UpdateStatusRequest { Status = "confirmed" };

        // Act
        await _controller.UpdateStatus(order.Id, request);

        // Assert
        var updatedOrder = await _context.Orders.FindAsync(order.Id);
        updatedOrder!.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    #endregion

    #region CancelOrder Tests

    [Fact]
    public async Task CancelOrder_WithValidOrder_ReturnsOk()
    {
        // Arrange
        await SeedTestOrders();
        var order = await _context.Orders.FirstAsync(o => o.Status == "pending");

        SetupUserContext(order.UserId.ToString());

        // Act
        var result = await _controller.CancelOrder(order.Id);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task CancelOrder_WithoutUserId_ReturnsUnauthorized()
    {
        // Arrange
        ClearUserContext();

        // Act
        var result = await _controller.CancelOrder(1);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task CancelOrder_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        SetupUserContext("1");

        // Act
        var result = await _controller.CancelOrder(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CancelOrder_ByOwner_Succeeds()
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

        SetupUserContext("1");

        // Act
        var result = await _controller.CancelOrder(order.Id);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Theory]
    [InlineData("admin")]
    [InlineData("canteen")]
    public async Task CancelOrder_ByAdmin_Succeeds(string role)
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

        SetupUserContext("2", role);

        // Act
        var result = await _controller.CancelOrder(order.Id);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Theory]
    [InlineData("student")]
    [InlineData("staff")]
    public async Task CancelOrder_ByOtherUser_ReturnsForbidden(string role)
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

        SetupUserContext("2", role);

        // Act
        var result = await _controller.CancelOrder(order.Id);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task CancelOrder_WhenCompleted_ReturnsBadRequest()
    {
        // Arrange
        var order = new Order
        {
            UserId = 1,
            TotalAmount = 10m,
            Status = "completed",
            Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 10m } }
        };
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        SetupUserContext("1");

        // Act
        var result = await _controller.CancelOrder(order.Id);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CancelOrder_WhenAlreadyCancelled_ReturnsBadRequest()
    {
        // Arrange
        var order = new Order
        {
            UserId = 1,
            TotalAmount = 10m,
            Status = "cancelled",
            Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 10m } }
        };
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        SetupUserContext("1");

        // Act
        var result = await _controller.CancelOrder(order.Id);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CancelOrder_SetsStatusAndPaymentStatus()
    {
        // Arrange
        var order = new Order
        {
            UserId = 1,
            TotalAmount = 10m,
            Status = "pending",
            PaymentStatus = "unpaid",
            Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 10m } }
        };
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        SetupUserContext("1");

        // Act
        await _controller.CancelOrder(order.Id);

        // Assert
        var cancelledOrder = await _context.Orders.FindAsync(order.Id);
        cancelledOrder!.Status.Should().Be("cancelled");
        cancelledOrder.PaymentStatus.Should().Be("refunded");
    }

    #endregion

    #region GetStats Tests

    [Fact]
    public async Task GetStats_WithNoOrders_ReturnsZeroStats()
    {
        // Act
        var result = await _controller.GetStats();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var stats = okResult.Value.Should().BeOfType<OrderStatsDto>().Subject;
        stats.TotalOrders.Should().Be(0);
        stats.TotalRevenue.Should().Be(0);
        stats.PendingOrders.Should().Be(0);
    }

    [Fact]
    public async Task GetStats_CalculatesCorrectly()
    {
        // Arrange
        var orders = new List<Order>
        {
            new() { UserId = 1, TotalAmount = 10m, Status = "pending", Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 10m } } },
            new() { UserId = 1, TotalAmount = 20m, Status = "completed", Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 20m } } },
            new() { UserId = 1, TotalAmount = 30m, Status = "completed", Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 30m } } },
            new() { UserId = 1, TotalAmount = 15m, Status = "cancelled", Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 15m } } }
        };

        _context.Orders.AddRange(orders);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetStats();

        // Assert
        var okResult = result.As<OkObjectResult>();
        var stats = okResult.Value.As<OrderStatsDto>();
        stats.TotalOrders.Should().Be(4);
        stats.PendingOrders.Should().Be(1);
        stats.CompletedOrders.Should().Be(2);
        stats.CancelledOrders.Should().Be(1);
        stats.TotalRevenue.Should().Be(50m); // Only completed orders
    }

    [Fact]
    public async Task GetStats_OnlyIncludesCompletedInRevenue()
    {
        // Arrange
        var orders = new List<Order>
        {
            new() { UserId = 1, TotalAmount = 100m, Status = "pending", Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 100m } } },
            new() { UserId = 1, TotalAmount = 50m, Status = "completed", Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 50m } } },
            new() { UserId = 1, TotalAmount = 75m, Status = "cancelled", Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 75m } } }
        };

        _context.Orders.AddRange(orders);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetStats();

        // Assert
        var okResult = result.As<OkObjectResult>();
        var stats = okResult.Value.As<OrderStatsDto>();
        stats.TotalRevenue.Should().Be(50m);
    }

    #endregion

    #region GetMyOrders Tests

    [Fact]
    public async Task GetMyOrders_WithoutUserId_ReturnsUnauthorized()
    {
        // Arrange
        ClearUserContext();

        // Act
        var result = await _controller.GetMyOrders();

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task GetMyOrders_ReturnsOnlyCurrentUserOrders()
    {
        // Arrange
        await SeedTestOrders();
        SetupUserContext("1");

        // Act
        var result = await _controller.GetMyOrders();

        // Assert
        var okResult = result.As<OkObjectResult>();
        var orders = okResult.Value as IEnumerable<OrderDto>;
        orders.Should().HaveCount(2);
        orders!.All(o => o.UserId == 1).Should().BeTrue();
    }

    [Fact]
    public async Task GetMyOrders_ResultsAreOrderedByCreatedAtDescending()
    {
        // Arrange
        var orders = new List<Order>
        {
            new()
            {
                UserId = 1,
                TotalAmount = 10m,
                Status = "pending",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 10m } }
            },
            new()
            {
                UserId = 1,
                TotalAmount = 20m,
                Status = "pending",
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItem> { new() { MenuItemId = 1, Quantity = 1, Price = 20m } }
            }
        };

        _context.Orders.AddRange(orders);
        await _context.SaveChangesAsync();

        SetupUserContext("1");

        // Act
        var result = await _controller.GetMyOrders();

        // Assert
        var okResult = result.As<OkObjectResult>();
        var resultOrders = okResult.Value as IEnumerable<OrderDto>;
        var amounts = resultOrders!.Select(o => o.TotalAmount).ToList();
        amounts.Should().ContainInOrder(20m, 10m);
    }

    [Fact]
    public async Task GetMyOrders_IncludesOrderItems()
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
                new() { MenuItemId = 2, Quantity = 1, Price = 15m, MenuItemName = "Fries" }
            }
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        SetupUserContext("1");

        // Act
        var result = await _controller.GetMyOrders();

        // Assert
        var okResult = result.As<OkObjectResult>();
        var orders = okResult.Value as IEnumerable<OrderDto>;
        orders!.First().Items.Should().HaveCount(2);
    }

    #endregion

    private async Task SeedTestOrders()
    {
        var orders = new List<Order>
        {
            new()
            {
                UserId = 1,
                TotalAmount = 25.99m,
                Status = "pending",
                PaymentStatus = "unpaid",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                Items = new List<OrderItem>
                {
                    new() { MenuItemId = 1, Quantity = 1, Price = 15.99m, MenuItemName = "Burger" },
                    new() { MenuItemId = 2, Quantity = 1, Price = 10m, MenuItemName = "Fries" }
                }
            },
            new()
            {
                UserId = 1,
                TotalAmount = 15m,
                Status = "completed",
                PaymentStatus = "paid",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                Items = new List<OrderItem> { new() { MenuItemId = 3, Quantity = 1, Price = 15m, MenuItemName = "Salad" } }
            },
            new()
            {
                UserId = 2,
                TotalAmount = 30m,
                Status = "pending",
                PaymentStatus = "unpaid",
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItem> { new() { MenuItemId = 4, Quantity = 2, Price = 15m, MenuItemName = "Pizza" } }
            }
        };

        _context.Orders.AddRange(orders);
        await _context.SaveChangesAsync();
    }
}
