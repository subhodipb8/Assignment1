using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CafeteriaAPI.Data;
using CafeteriaAPI.Models;

namespace CafeteriaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly CafeteriaDbContext _context;

        public OrdersController(CafeteriaDbContext context)
        {
            _context = context;
        }

        // GET: api/orders
        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] string? status = null)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            IQueryable<Order> query = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.User);

            // Filter by role
            if (user.Role == "student" || user.Role == "staff")
            {
                query = query.Where(o => o.UserId == userId);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status.ToLower());
            }

            var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();

            // Map to anonymous objects to avoid circular references
            var result = orders.Select(o => new
            {
                o.Id,
                o.UserId,
                user = new { o.User.Id, o.User.Name, o.User.Email, o.User.Role },
                o.TotalAmount,
                o.PickupTime,
                o.PickupDate,
                o.Status,
                o.PaymentStatus,
                o.OrderDate,
                o.SpecialInstructions,
                orderItems = o.OrderItems.Select(oi => new
                {
                    oi.Id,
                    oi.OrderId,
                    oi.MenuItemId,
                    menuItem = new
                    {
                        oi.MenuItem.Id,
                        oi.MenuItem.Name,
                        oi.MenuItem.Description,
                        oi.MenuItem.Price,
                        oi.MenuItem.Category,
                        oi.MenuItem.Image,
                        oi.MenuItem.DietaryTags,
                        oi.MenuItem.Allergens,
                        oi.MenuItem.NutritionInfo,
                        oi.MenuItem.Available,
                        oi.MenuItem.PreparationTime
                    },
                    oi.Quantity,
                    oi.Price
                })
            });

            return Ok(result);
        }

        // GET: api/orders/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound(new { message = "Order not found" });

            // Check authorization
            if (user.Role == "student" && order.UserId != userId)
            {
                return Forbid();
            }

            // Map to anonymous object to avoid circular references
            var result = new
            {
                order.Id,
                order.UserId,
                user = new { order.User.Id, order.User.Name, order.User.Email, order.User.Role },
                order.TotalAmount,
                order.PickupTime,
                order.PickupDate,
                order.Status,
                order.PaymentStatus,
                order.OrderDate,
                order.SpecialInstructions,
                orderItems = order.OrderItems.Select(oi => new
                {
                    oi.Id,
                    oi.OrderId,
                    oi.MenuItemId,
                    menuItem = new
                    {
                        oi.MenuItem.Id,
                        oi.MenuItem.Name,
                        oi.MenuItem.Description,
                        oi.MenuItem.Price,
                        oi.MenuItem.Category,
                        oi.MenuItem.Image,
                        oi.MenuItem.DietaryTags,
                        oi.MenuItem.Allergens,
                        oi.MenuItem.NutritionInfo,
                        oi.MenuItem.Available,
                        oi.MenuItem.PreparationTime
                    },
                    oi.Quantity,
                    oi.Price
                })
            };

            return Ok(result);
        }

        // POST: api/orders
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            // Validate pickup date is not in the past
            if (request.PickupDate.Date < DateTime.UtcNow.Date)
            {
                return BadRequest(new { message = "Pickup date cannot be in the past" });
            }

            // Calculate total and validate items
            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in request.Items)
            {
                var menuItem = await _context.MenuItems.FindAsync(item.MenuItemId);
                if (menuItem == null || !menuItem.Available)
                {
                    return BadRequest(new { message = $"Menu item {item.MenuItemId} is not available" });
                }

                // Check if item is already maxed out for today
                if (menuItem.OrdersToday >= menuItem.MaxOrderPerDay)
                {
                    return BadRequest(new { message = $"{menuItem.Name} is sold out for today" });
                }

                // Check for allergens
                if (menuItem.Allergens.Any(a => user.Allergies.Contains(a)))
                {
                    return BadRequest(new { message = $"Warning: {menuItem.Name} contains allergens you are sensitive to" });
                }

                totalAmount += menuItem.Price * item.Quantity;
                orderItems.Add(new OrderItem
                {
                    MenuItemId = item.MenuItemId,
                    Quantity = item.Quantity,
                    Price = menuItem.Price
                });

                // Increment today's orders
                menuItem.OrdersToday++;
            }

            // Check wallet balance
            if (user.WalletBalance < totalAmount)
            {
                return BadRequest(new { message = "Insufficient wallet balance. Please add funds to your wallet." });
            }

            // Create order
            var order = new Order
            {
                UserId = userId.Value,
                TotalAmount = totalAmount,
                PickupTime = request.PickupTime,
                PickupDate = request.PickupDate,
                Status = "pending",
                PaymentStatus = "completed", // Simulated payment
                SpecialInstructions = request.SpecialInstructions,
                OrderItems = orderItems
            };

            // Deduct from wallet
            user.WalletBalance -= totalAmount;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        // PUT: api/orders/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            var userRole = user?.Role?.ToLower();
            if (user == null || (userRole != "canteen" && userRole != "admin"))
            {
                return Forbid();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound(new { message = "Order not found" });

            var validStatuses = new[] { "pending", "confirmed", "preparing", "ready", "completed", "cancelled" };
            if (!validStatuses.Contains(request.Status.ToLower()))
            {
                return BadRequest(new { message = "Invalid status" });
            }

            order.Status = request.Status.ToLower();
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { id = order.Id, status = order.Status, message = "Status updated successfully" });
        }

        // DELETE: api/orders/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound(new { message = "Order not found" });

            // Only allow cancellation if pending and owned by user
            if (order.UserId != userId && !User.IsInRole("admin"))
            {
                return Forbid();
            }

            if (order.Status != "pending")
            {
                return BadRequest(new { message = "Can only cancel pending orders" });
            }

            // Refund to wallet
            var user = await _context.Users.FindAsync(order.UserId);
            if (user != null)
            {
                user.WalletBalance += order.TotalAmount;
            }

            // Restore menu item counts
            foreach (var orderItem in order.OrderItems)
            {
                var menuItem = await _context.MenuItems.FindAsync(orderItem.MenuItemId);
                if (menuItem != null)
                {
                    menuItem.OrdersToday = Math.Max(0, menuItem.OrdersToday - orderItem.Quantity);
                }
            }

            order.Status = "cancelled";
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Order cancelled successfully" });
        }

        // GET: api/orders/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetOrderStats()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            var userRole = user?.Role?.ToLower();
            if (user == null || (userRole != "canteen" && userRole != "admin"))
            {
                return Forbid();
            }

            var today = DateTime.UtcNow.Date;
            var stats = new
            {
                TotalOrders = await _context.Orders.CountAsync(),
                TodayOrders = await _context.Orders.CountAsync(o => o.OrderDate.Date == today),
                PendingOrders = await _context.Orders.CountAsync(o => o.Status == "pending"),
                Revenue = await _context.Orders.Where(o => o.PaymentStatus == "completed").SumAsync(o => o.TotalAmount),
                TodayRevenue = await _context.Orders.Where(o => o.OrderDate.Date == today && o.PaymentStatus == "completed").SumAsync(o => o.TotalAmount)
            };

            return Ok(stats);
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return null;
            return int.Parse(userIdClaim);
        }
    }

    public class CreateOrderRequest
    {
        public List<OrderItemRequest> Items { get; set; } = new();
        public string PickupTime { get; set; } = "12:00";
        public DateTime PickupDate { get; set; } = DateTime.UtcNow.AddDays(1);
        public string? SpecialInstructions { get; set; }
    }

    public class OrderItemRequest
    {
        public int MenuItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}
