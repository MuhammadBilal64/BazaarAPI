using E_Commerce_BackendAPI.Dtos;

namespace E_Commerce_BackendAPI.Services
{
    public interface ICartService
    {
        Task<CartResponseDto> GetCartAsync(int userId);
        Task AddToCartAsync(int userId, AddToCartRequest request);
        Task UpdateCartItemAsync(int userId, int itemId, UpdateCartItemRequest request);
        Task RemoveFromCartAsync(int userId, int itemId);
    }
}

