using System.Linq;
using E_Commerce_BackendAPI.DAL;
using E_Commerce_BackendAPI.Dtos;
using E_Commerce_BackendAPI.Exceptions;
using E_Commerce_BackendAPI.Model;
using E_Commerce_BackendAPI.Utilities;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_BackendAPI.Services
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _context;

        public CartService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CartResponseDto> GetCartAsync(int userId)
        {
            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId && c.Status == CartStatus.Active && c.IsActive)
                .Select(c => new CartItemDto
                {
                    Id = c.Id,
                    ProductId = c.ProductId,
                    ProductName = c.Product.Name,
                    Quantity = c.Quantity,
                    UnitPrice = c.Product.Price
                })
                .ToListAsync();

            return new CartResponseDto
            {
                Items = cartItems,
                TotalItemCount = cartItems.Sum(i => i.Quantity),
                CartTotal = cartItems.Sum(i => i.LineTotal)
            };
        }

        public async Task AddToCartAsync(int userId, AddToCartRequest request)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId && p.IsActive);
            if (product == null)
                throw new NotFoundException("Product not found or inactive.");

            if (product.Stock < request.Quantity)
                throw new ArgumentException($"Insufficient stock. Available: {product.Stock}.");

            var existing = await _context.CartItems.FirstOrDefaultAsync(c =>
                c.UserId == userId &&
                c.ProductId == request.ProductId &&
                c.Status == CartStatus.Active &&
                c.IsActive);

            if (existing != null)
            {
                existing.Quantity += request.Quantity;
                if (product.Stock < existing.Quantity)
                    throw new ArgumentException($"Insufficient stock. Available: {product.Stock}. Already in cart: {existing.Quantity - request.Quantity}.");

                existing.ModifiedDate = DateTime.UtcNow;
                existing.ModifiedBy = userId;
            }
            else
            {
                _context.CartItems.Add(new CartItem
                {
                    UserId = userId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    Status = CartStatus.Active,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = userId
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateCartItemAsync(int userId, int itemId, UpdateCartItemRequest request)
        {
            var item = await _context.CartItems
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == itemId && c.UserId == userId && c.Status == CartStatus.Active && c.IsActive);

            if (item == null)
                throw new NotFoundException("Cart item not found.");

            if (item.Product.Stock < request.Quantity)
                throw new ArgumentException($"Insufficient stock. Available: {item.Product.Stock}.");

            item.Quantity = request.Quantity;
            item.ModifiedDate = DateTime.UtcNow;
            item.ModifiedBy = userId;
            await _context.SaveChangesAsync();
        }

        public async Task RemoveFromCartAsync(int userId, int itemId)
        {
            var item = await _context.CartItems.FirstOrDefaultAsync(c =>
                c.Id == itemId &&
                c.UserId == userId &&
                c.Status == CartStatus.Active &&
                c.IsActive);

            if (item == null)
                throw new NotFoundException("Cart item not found.");

            item.IsActive = false;
            item.ModifiedDate = DateTime.UtcNow;
            item.ModifiedBy = userId;
            await _context.SaveChangesAsync();
        }
    }
}

