using E_Commerce_BackendAPI.DAL;
using E_Commerce_BackendAPI.Dtos;
using E_Commerce_BackendAPI.Exceptions;
using E_Commerce_BackendAPI.Model;
using E_Commerce_BackendAPI.Utilities;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_BackendAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderService> _logger;

        public OrderService(AppDbContext context, ILogger<OrderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PaginationResponseDto<OrderListDto>> GetMyOrdersAsync(
            int userId,
            int page,
            int pageSize,
            string? status,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = _context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId);

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

            return new PaginationResponseDto<OrderListDto>
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Items = orders
            };
        }

        public async Task<OrderDetailDto> GetOrderByIdAsync(int id, int userId, bool isAdmin)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                throw new NotFoundException("Order not found.");

            if (!isAdmin && order.UserId != userId)
                throw new ForbiddenException("You can only view your own orders.");

            return new OrderDetailDto
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
        }

        public async Task<OrderCreateResponseDto> CreateOrderFromCartAsync(int userId)
        {
            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId && c.Status == CartStatus.Active && c.IsActive)
                .ToListAsync();

            if (cartItems.Count == 0)
                throw new ArgumentException("Cart is empty.");

            foreach (var item in cartItems)
            {
                if (item.Product.Stock < item.Quantity)
                    throw new ArgumentException(
                        $"Insufficient stock for product '{item.Product.Name}'. Available: {item.Product.Stock}, requested: {item.Quantity}.");
            }

            Order? order = null;
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                decimal totalAmount = 0;
                var orderItems = new List<OrderItem>();

                order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Created,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = userId
                };

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
                        CreatedBy = userId,

                        // Important: link the OrderItem to the Order so EF can insert
                        // both in one SaveChanges() call (no "missing items" on crash).
                        Order = order
                    });
                }

                order.TotalAmount = totalAmount;
                order.OrderItems = orderItems;

                _context.Orders.Add(order);

                foreach (var item in cartItems)
                {
                    item.Status = CartStatus.Ordered;
                    item.ModifiedDate = DateTime.UtcNow;
                    item.ModifiedBy = userId;
                }

                // Decrease stock for the successful order creation.
                foreach (var item in cartItems)
                {
                    item.Product.Stock -= item.Quantity;
                }

                // One SaveChanges() ensures either everything commits or nothing does.
                await _context.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }

            if (order == null)
                throw new InvalidOperationException("Order creation failed unexpectedly.");

            _logger.LogInformation(
                "Order created. OrderId: {OrderId}, UserId: {UserId}, TotalAmount: {TotalAmount}",
                order.Id, userId, order.TotalAmount);

            return new OrderCreateResponseDto
            {
                OrderId = order.Id,
                TotalAmount = order.TotalAmount,
                Message = "Order created."
            };
        }

        public async Task<OrderStatus> UpdateOrderStatusAsync(int id, string status, int? modifiedBy)
        {
            if (!Enum.TryParse<OrderStatus>(status, true, out var newStatus))
                throw new ArgumentException("Invalid status. Use: Created, Paid, Shipped, Cancelled.");

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                throw new NotFoundException("Order not found.");

            order.Status = newStatus;
            order.ModifiedDate = DateTime.UtcNow;
            order.ModifiedBy = modifiedBy;
            await _context.SaveChangesAsync();

            return order.Status;
        }

        public async Task<PaymentSimulationResultDto> SimulatePaymentAsync(
            int id,
            int userId,
            bool isAdmin,
            bool simulateSuccess)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                throw new NotFoundException("Order not found.");

            if (!isAdmin && order.UserId != userId)
                throw new ForbiddenException("You can only pay for your own order.");

            if (order.Status != OrderStatus.Created)
                throw new ArgumentException(
                    $"Order cannot be paid. Current status: {order.Status}. Only orders in 'Created' status can be paid.");

            if (simulateSuccess)
            {
                order.Status = OrderStatus.Paid;
                order.ModifiedDate = DateTime.UtcNow;
                order.ModifiedBy = userId;
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Payment simulated success. OrderId: {OrderId}, UserId: {UserId}",
                    order.Id, userId);

                return new PaymentSimulationResultDto
                {
                    Success = true,
                    OrderId = order.Id,
                    Status = order.Status,
                    Message = "Payment successful."
                };
            }

            _logger.LogInformation(
                "Payment simulated failure. OrderId: {OrderId}, UserId: {UserId}",
                order.Id, userId);

            return new PaymentSimulationResultDto
            {
                Success = false,
                OrderId = order.Id,
                Status = order.Status,
                Message = "Payment failed (simulated)."
            };
        }

        public async Task<PaginationResponseDto<OrderListDto>> GetAllOrdersAsync(
            int page,
            int pageSize,
            string? status,
            DateTime? fromDate,
            DateTime? toDate)
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

            return new PaginationResponseDto<OrderListDto>
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Items = orders
            };
        }

        private static IQueryable<Order> ApplyOrderFilters(
            IQueryable<Order> query,
            string? status,
            DateTime? fromDate,
            DateTime? toDate)
        {
            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
            {
                query = query.Where(o => o.Status == orderStatus);
            }

            if (fromDate.HasValue)
                query = query.Where(o => o.OrderDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(o => o.OrderDate <= toDate.Value);

            return query;
        }
    }
}

