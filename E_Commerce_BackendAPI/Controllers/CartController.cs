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
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        /// <summary>Get current user's active cart items.</summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCart()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            return Ok(await _cartService.GetCartAsync(userId.Value));
        }

        /// <summary>Add product to cart or update quantity if already in cart.</summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            await _cartService.AddToCartAsync(userId.Value, request);
            return Ok("Cart updated.");
        }

        /// <summary>Update cart item quantity.</summary>
        [HttpPut("{itemId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCartItem(int itemId, [FromBody] UpdateCartItemRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            await _cartService.UpdateCartItemAsync(userId.Value, itemId, request);
            return Ok("Cart item updated.");
        }

        /// <summary>Remove item from cart (soft delete: sets IsActive = false).</summary>
        [HttpDelete("{itemId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveFromCart(int itemId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            await _cartService.RemoveFromCartAsync(userId.Value, itemId);
            return NoContent();
        }

        private int? GetCurrentUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return int.TryParse(sub, out var id) ? id : null;
        }
    }
}
