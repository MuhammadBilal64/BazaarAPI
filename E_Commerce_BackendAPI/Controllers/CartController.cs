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
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>Get current user's active cart items.</summary>
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId.Value && c.Status == CartStatus.Active && c.IsActive)
                .Select(c => new CartItemDto
                {
                    Id = c.Id,
                    ProductId = c.ProductId,
                    ProductName = c.Product.Name,
                    Quantity = c.Quantity,
                    UnitPrice = c.Product.Price
                })
                .ToListAsync();

            return Ok(new
            {
                Items = cartItems,
                TotalItemCount = cartItems.Sum(i => i.Quantity),
                CartTotal = cartItems.Sum(i => i.LineTotal)
            });
        }

        /// <summary>Add product to cart or update quantity if already in cart.</summary>
        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId && p.IsActive);
            if (product == null)
                return NotFound("Product not found or inactive.");
            if (product.Stock < request.Quantity)
                return BadRequest($"Insufficient stock. Available: {product.Stock}.");

            var existing = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId.Value && c.ProductId == request.ProductId && c.Status == CartStatus.Active && c.IsActive);
            if (existing != null)
            {
                existing.Quantity += request.Quantity;
                if (product.Stock < existing.Quantity)
                    return BadRequest($"Insufficient stock. Available: {product.Stock}. Already in cart: {existing.Quantity - request.Quantity}.");
                existing.ModifiedDate = DateTime.UtcNow;
                existing.ModifiedBy = userId;
            }
            else
            {
                _context.CartItems.Add(new CartItem
                {
                    UserId = userId.Value,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    Status = CartStatus.Active,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = userId.Value
                });
            }
            await _context.SaveChangesAsync();
            return Ok("Cart updated.");
        }

        /// <summary>Update cart item quantity.</summary>
        [HttpPut("{itemId:int}")]
        public async Task<IActionResult> UpdateCartItem(int itemId, [FromBody] UpdateCartItemRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var item = await _context.CartItems.Include(c => c.Product).FirstOrDefaultAsync(c => c.Id == itemId && c.UserId == userId.Value && c.Status == CartStatus.Active && c.IsActive);
            if (item == null)
                return NotFound("Cart item not found.");
            if (item.Product.Stock < request.Quantity)
                return BadRequest($"Insufficient stock. Available: {item.Product.Stock}.");

            item.Quantity = request.Quantity;
            item.ModifiedDate = DateTime.UtcNow;
            item.ModifiedBy = userId;
            await _context.SaveChangesAsync();
            return Ok("Cart item updated.");
        }

        /// <summary>Remove item from cart (soft delete: sets IsActive = false).</summary>
        [HttpDelete("{itemId:int}")]
        public async Task<IActionResult> RemoveFromCart(int itemId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var item = await _context.CartItems.FirstOrDefaultAsync(c => c.Id == itemId && c.UserId == userId.Value && c.Status == CartStatus.Active && c.IsActive);
            if (item == null)
                return NotFound("Cart item not found.");
            item.IsActive = false;
            item.ModifiedDate = DateTime.UtcNow;
            item.ModifiedBy = userId;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private int? GetCurrentUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return int.TryParse(sub, out var id) ? id : null;
        }
    }
}
