using System.Security.Claims;
using E_Commerce_BackendAPI.DAL;
using E_Commerce_BackendAPI.Dtos;
using E_Commerce_BackendAPI.Model;
using E_Commerce_BackendAPI.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderController> _logger;

        public OrderController(AppDbContext context, ILogger<OrderController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>List current user's orders (customer). Pagination and optional status/date filters.</summary>
        [HttpGet]
        public async Task<IActionResult> GetMyOrders(
            int page = 1,
            int pageSize = 10,
            string? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var query = _context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId.Value);

            query = ApplyOrderFilters(query, status, fromDate, toDate);

            var totalCount = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new OrderListDto
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    Status = o.Status.ToString(),
                    TotalAmount = o.TotalAmount,
                    ItemCount = o.OrderItems.Count
                })
                .ToListAsync();

            return Ok(new
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Items = orders
            });
        }

        /// <summary>Get order by id. Customers can only view their own orders.</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var isAdmin = User.IsInRole(nameof(UserRole.Admin));
            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            if (!isAdmin && order.UserId != userId.Value)
                return Forbid();

            var detail = new OrderDetailDto
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                Items = order.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            };

            return Ok(detail);
        }

        /// <summary>Create order from current user's cart.</summary>
        [HttpPost]
        public async Task<IActionResult> CreateOrderFromCart()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId.Value && c.Status == CartStatus.Active && c.IsActive)
                .ToListAsync();
            if (cartItems.Count == 0)
                return BadRequest("Cart is empty.");

            foreach (var item in cartItems)
            {
                if (item.Product.Stock < item.Quantity)
                    return BadRequest($"Insufficient stock for product '{item.Product.Name}'. Available: {item.Product.Stock}, requested: {item.Quantity}.");
            }

            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();
            foreach (var item in cartItems)
            {
                var lineTotal = item.Product.Price * item.Quantity;
                totalAmount += lineTotal;
                orderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = userId.Value
                });
            }

            var order = new Order
            {
                UserId = userId.Value,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Created,
                TotalAmount = totalAmount,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = userId.Value
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var oi in orderItems)
            {
                oi.OrderId = order.Id;
                _context.OrderItems.Add(oi);
            }
            foreach (var item in cartItems)
            {
                item.Status = CartStatus.Ordered;
                item.ModifiedDate = DateTime.UtcNow;
                item.ModifiedBy = userId;
            }
            foreach (var item in cartItems)
            {
                item.Product.Stock -= item.Quantity;
            }
            await _context.SaveChangesAsync();

            _logger.LogInformation("Order created. OrderId: {OrderId}, UserId: {UserId}, TotalAmount: {TotalAmount}", order.Id, userId.Value, order.TotalAmount);
            return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, new { OrderId = order.Id, TotalAmount = order.TotalAmount, Message = "Order created." });
        }

        /// <summary>Update order status (Admin only).</summary>
        [HttpPut("{id:int}/status")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
        {
            if (!Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
                return BadRequest("Invalid status. Use: Created, Paid, Shipped, Cancelled.");

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();
            order.Status = newStatus;
            order.ModifiedDate = DateTime.UtcNow;
            order.ModifiedBy = GetCurrentUserId();
            await _context.SaveChangesAsync();
            return Ok(new { OrderId = order.Id, Status = order.Status.ToString() });
        }

        /// <summary>Simulate payment for an order (mock). On success, marks order as Paid. Replace with real gateway later.</summary>
        [HttpPost("{id:int}/pay")]
        public async Task<IActionResult> SimulatePayment(int id, [FromBody] SimulatePaymentRequest? request = null)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound("Order not found.");

            if (!User.IsInRole(nameof(UserRole.Admin)) && order.UserId != userId.Value)
                return Forbid("You can only pay for your own order.");

            if (order.Status != OrderStatus.Created)
                return BadRequest($"Order cannot be paid. Current status: {order.Status}. Only orders in 'Created' status can be paid.");

            bool simulateSuccess = request?.SimulateSuccess ?? true;

            if (simulateSuccess)
            {
                order.Status = OrderStatus.Paid;
                order.ModifiedDate = DateTime.UtcNow;
                order.ModifiedBy = userId;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Payment simulated success. OrderId: {OrderId}, UserId: {UserId}", order.Id, userId.Value);
                return Ok(new { Success = true, OrderId = order.Id, Status = order.Status.ToString(), Message = "Payment successful." });
            }

            _logger.LogInformation("Payment simulated failure. OrderId: {OrderId}, UserId: {UserId}", order.Id, userId.Value);
            return Ok(new { Success = false, OrderId = order.Id, Message = "Payment failed (simulated)." });
        }

        /// <summary>List all orders (Admin only). Pagination and optional status/date filters.</summary>
        [HttpGet("admin")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> GetAllOrders(
            int page = 1,
            int pageSize = 10,
            string? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var query = _context.Orders.AsNoTracking();

            query = ApplyOrderFilters(query, status, fromDate, toDate);

            var totalCount = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new OrderListDto
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    Status = o.Status.ToString(),
                    TotalAmount = o.TotalAmount,
                    ItemCount = o.OrderItems.Count
                })
                .ToListAsync();

            return Ok(new
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Items = orders
            });
        }

        private static IQueryable<Order> ApplyOrderFilters(
            IQueryable<Order> query,
            string? status,
            DateTime? fromDate,
            DateTime? toDate)
        {
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
                query = query.Where(o => o.Status == orderStatus);
            if (fromDate.HasValue)
                query = query.Where(o => o.OrderDate >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(o => o.OrderDate <= toDate.Value);
            return query;
        }

        private int? GetCurrentUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return int.TryParse(sub, out var id) ? id : null;
        }
    }
}
