using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.Models;

namespace OrderService.Controllers;

/// <summary>
/// Order management controller for cafeteria order operations
/// </summary>
[ApiController]
[Route("api/orders")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly OrderDbContext _context;

    /// <summary>
    /// Initializes a new instance of the OrdersController
    /// </summary>
    /// <param name="context">Database context for order operations</param>
    public OrdersController(OrderDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all orders with optional filtering
    /// </summary>
    /// <remarks>
    /// Retrieves a list of orders. Admins and canteen staff see all orders,
    /// while regular users only see their own orders.
    /// Results are ordered by creation date (newest first).
    /// </remarks>
    /// <param name="status">Optional filter by status (e.g., "pending", "completed")</param>
    /// <returns>List of orders</returns>
    /// <response code="200">Orders retrieved successfully</response>
    /// <response code="401">User ID not found in request</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetOrders([FromQuery] string? status)
    {
        var query = _context.Orders
            .Include(o => o.Items)
            .AsQueryable();

        // Get user context from headers (passed by API Gateway)
        if (!Request.Headers.TryGetValue("X-User-Id", out var userIdValue) ||
            !int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(new { message = "User ID not found in request" });
        }

        // Check if user is admin
        Request.Headers.TryGetValue("X-User-Role", out var role);
        var isAdmin = role.ToString().ToLower() == "admin" || role.ToString().ToLower() == "canteen";

        // Non-admin users can only see their own orders
        if (!isAdmin)
        {
            query = query.Where(o => o.UserId == userId);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(o => o.Status == status.ToLower());
        }

        var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        return Ok(orders.Select(MapToDto));
    }

    /// <summary>
    /// Get a specific order by ID
    /// </summary>
    /// <remarks>
    /// Retrieves detailed information about a specific order including all items.
    /// </remarks>
    /// <param name="id">The order ID</param>
    /// <returns>Order details with items</returns>
    /// <response code="200">Order found</response>
    /// <response code="404">Order not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Create a new order
    /// </summary>
    /// <remarks>
    /// Creates a new order with the specified items and pickup time.
    /// Order must contain at least one item and pickup date cannot be in the past.
    /// Total amount is calculated automatically from item prices and quantities.
    /// </remarks>
    /// <param name="request">Order creation details including items and pickup time</param>
    /// <returns>Created order</returns>
    /// <response code="201">Order created successfully</response>
    /// <response code="400">Invalid input - empty items or past pickup date</response>
    /// <response code="401">User ID not found in request</response>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
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

    /// <summary>
    /// Update order status
    /// </summary>
    /// <remarks>
    /// Updates the status of an existing order.
    /// Valid statuses: pending, confirmed, preparing, ready, completed, cancelled.
    /// When status is set to "completed", payment status is automatically set to "paid".
    /// </remarks>
    /// <param name="id">The order ID</param>
    /// <param name="request">New status value</param>
    /// <returns>Updated order</returns>
    /// <response code="200">Order status updated successfully</response>
    /// <response code="400">Invalid status value</response>
    /// <response code="404">Order not found</response>
    [HttpPut("{id}/status")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Cancel an order
    /// </summary>
    /// <remarks>
    /// Cancels an existing order. Users can only cancel their own orders,
    /// while admins and canteen staff can cancel any order.
    /// Cannot cancel orders that are already completed or cancelled.
    /// </remarks>
    /// <param name="id">The order ID</param>
    /// <returns>Success message</returns>
    /// <response code="200">Order cancelled successfully</response>
    /// <response code="400">Order already completed or cancelled</response>
    /// <response code="401">User ID not found in request</response>
    /// <response code="403">User not authorized to cancel this order</response>
    /// <response code="404">Order not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Get order statistics
    /// </summary>
    /// <remarks>
    /// Returns aggregate statistics for all orders including counts by status
    /// and total revenue from completed orders.
    /// </remarks>
    /// <returns>Order statistics</returns>
    /// <response code="200">Statistics retrieved successfully</response>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(OrderStatsDto), StatusCodes.Status200OK)]
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

    /// <summary>
    /// Get current user's orders
    /// </summary>
    /// <remarks>
    /// Retrieves all orders for the authenticated user.
    /// Results are ordered by creation date (newest first).
    /// </remarks>
    /// <returns>List of user's orders</returns>
    /// <response code="200">Orders retrieved successfully</response>
    /// <response code="401">User ID not found in request</response>
    [HttpGet("my-orders")]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
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
