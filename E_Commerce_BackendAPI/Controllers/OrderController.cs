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

        public OrderController(AppDbContext context)
        {
            _context = context;
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
