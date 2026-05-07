using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.Models;

namespace OrderService.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly OrderDbContext _context;

    public OrdersController(OrderDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders([FromQuery] int? userId, [FromQuery] string? status)
    {
        var query = _context.Orders
            .Include(o => o.Items)
            .AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(o => o.UserId == userId.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(o => o.Status == status.ToLower());
        }

        var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        return Ok(orders.Select(MapToDto));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound(new { message = "Order not found" });
        }

        return Ok(MapToDto(order));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        // Get UserId from header (passed by API Gateway)
        if (!Request.Headers.TryGetValue("X-User-Id", out var userIdValue) ||
            !int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(new { message = "User ID not found in request" });
        }

        if (request.Items == null || !request.Items.Any())
        {
            return BadRequest(new { message = "Order must contain at least one item" });
        }

        // Validate pickup time
        if (request.PickupDate.Date < DateTime.UtcNow.Date)
        {
            return BadRequest(new { message = "Pickup date cannot be in the past" });
        }

        // Calculate total
        var totalAmount = request.Items.Sum(i => i.Price * i.Quantity);

        var order = new Order
        {
            UserId = userId,
            TotalAmount = totalAmount,
            PickupTime = request.PickupTime.Kind == DateTimeKind.Utc ? request.PickupTime : request.PickupTime.ToUniversalTime(),
            PickupDate = request.PickupDate.Kind == DateTimeKind.Utc ? request.PickupDate : request.PickupDate.ToUniversalTime(),
            Status = "pending",
            PaymentStatus = "unpaid",
            SpecialInstructions = request.SpecialInstructions,
            Items = request.Items.Select(i => new OrderItem
            {
                MenuItemId = i.MenuItemId,
                Quantity = i.Quantity,
                Price = i.Price,
                MenuItemName = i.MenuItemName
            }).ToList()
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, MapToDto(order));
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return NotFound(new { message = "Order not found" });
        }

        var validStatuses = new[] { "pending", "confirmed", "preparing", "ready", "completed", "cancelled" };
        if (!validStatuses.Contains(request.Status.ToLower()))
        {
            return BadRequest(new { message = "Invalid status" });
        }

        order.Status = request.Status.ToLower();
        order.UpdatedAt = DateTime.UtcNow;

        if (order.Status == "completed")
        {
            order.PaymentStatus = "paid";
        }

        await _context.SaveChangesAsync();

        return Ok(MapToDto(order));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        // Get UserId from header
        if (!Request.Headers.TryGetValue("X-User-Id", out var userIdValue) ||
            !int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(new { message = "User ID not found in request" });
        }

        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return NotFound(new { message = "Order not found" });
        }

        // Only allow cancellation if user owns the order or is admin
        if (order.UserId != userId)
        {
            // Check if user is admin or canteen staff
            Request.Headers.TryGetValue("X-User-Role", out var role);
            var roleString = role.ToString();
            if (roleString != "admin" && roleString != "canteen")
            {
                return Forbid();
            }
        }

        if (order.Status is "completed" or "cancelled")
        {
            return BadRequest(new { message = "Cannot cancel completed or already cancelled orders" });
        }

        order.Status = "cancelled";
        order.PaymentStatus = "refunded";
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Order cancelled successfully" });
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var orders = await _context.Orders.ToListAsync();

        var stats = new OrderStatsDto
        {
            TotalOrders = orders.Count,
            PendingOrders = orders.Count(o => o.Status == "pending"),
            ConfirmedOrders = orders.Count(o => o.Status == "confirmed"),
            PreparingOrders = orders.Count(o => o.Status == "preparing"),
            ReadyOrders = orders.Count(o => o.Status == "ready"),
            CompletedOrders = orders.Count(o => o.Status == "completed"),
            CancelledOrders = orders.Count(o => o.Status == "cancelled"),
            TotalRevenue = orders.Where(o => o.Status == "completed").Sum(o => o.TotalAmount)
        };

        return Ok(stats);
    }

    [HttpGet("my-orders")]
    public async Task<IActionResult> GetMyOrders()
    {
        if (!Request.Headers.TryGetValue("X-User-Id", out var userIdValue) ||
            !int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(new { message = "User ID not found in request" });
        }

        var orders = await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(orders.Select(MapToDto));
    }

    private static OrderDto MapToDto(Order order) => new()
    {
        Id = order.Id,
        UserId = order.UserId,
        TotalAmount = order.TotalAmount,
        PickupTime = order.PickupTime,
        PickupDate = order.PickupDate,
        Status = order.Status,
        PaymentStatus = order.PaymentStatus,
        SpecialInstructions = order.SpecialInstructions,
        CreatedAt = order.CreatedAt,
        UpdatedAt = order.UpdatedAt,
        Items = order.Items.Select(i => new OrderItemDto
        {
            Id = i.Id,
            MenuItemId = i.MenuItemId,
            MenuItemName = i.MenuItemName,
            Quantity = i.Quantity,
            Price = i.Price
        }).ToList()
    };
}
