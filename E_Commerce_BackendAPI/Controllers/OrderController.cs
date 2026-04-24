using System.Security.Claims;
using E_Commerce_BackendAPI.Dtos;
using E_Commerce_BackendAPI.Utilities;
using E_Commerce_BackendAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce_BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        private string? GetIdempotencyKey()
        {
            if (Request.Headers.TryGetValue("Idempotency-Key", out var key))
            {
                return key.ToString();
            }
            return null;
        }
  

        /// <summary>List current user's orders (customer). Pagination and optional status/date filters.</summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
            return Ok(await _orderService.GetMyOrdersAsync(userId.Value, page, pageSize, status, fromDate, toDate));
        }

        /// <summary>Get order by id. Customers can only view their own orders.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var isAdmin = User.IsInRole(nameof(UserRole.Admin));
            return Ok(await _orderService.GetOrderByIdAsync(id, userId.Value, isAdmin));
        }

        /// <summary>Create order from current user's cart.</summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateOrderFromCart()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var idempotencyKey = GetIdempotencyKey();

            var result = await _orderService.CreateOrderFromCartAsync(userId.Value, idempotencyKey);
            return CreatedAtAction(nameof(GetOrderById), new { id = result.OrderId }, new { result.OrderId, result.TotalAmount, result.Message });
        }

        /// <summary>Update order status (Admin only).</summary>
        [HttpPut("{id:int}/status")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
        {
            var modifiedBy = GetCurrentUserId();
            var newStatus = await _orderService.UpdateOrderStatusAsync(id, request.Status, modifiedBy);
            return Ok(new { OrderId = id, Status = newStatus.ToString() });
        }

        /// <summary>Simulate payment for an order (mock). On success, marks order as Paid. Replace with real gateway later.</summary>
        [HttpPost("{id:int}/pay")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SimulatePayment(int id, [FromBody] SimulatePaymentRequest? request = null)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var isAdmin = User.IsInRole(nameof(UserRole.Admin));
            var simulateSuccess = request?.SimulateSuccess ?? true;
            var result = await _orderService.SimulatePaymentAsync(id, userId.Value, isAdmin, simulateSuccess);

            if (result.Success)
                return Ok(new { Success = true, OrderId = result.OrderId, Status = result.Status.ToString(), Message = result.Message });

            return Ok(new { Success = false, OrderId = result.OrderId, Message = result.Message });
        }

        /// <summary>List all orders (Admin only). Pagination and optional status/date filters.</summary>
        [HttpGet("admin")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllOrders(
            int page = 1,
            int pageSize = 10,
            string? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            return Ok(await _orderService.GetAllOrdersAsync(page, pageSize, status, fromDate, toDate));
        }

        private int? GetCurrentUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return int.TryParse(sub, out var id) ? id : null;
        }
    }
}
