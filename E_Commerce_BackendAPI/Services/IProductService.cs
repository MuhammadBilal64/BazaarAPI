using E_Commerce_BackendAPI.Dtos;
using E_Commerce_BackendAPI.Model;

namespace E_Commerce_BackendAPI.Services
{
    public interface IProductService
    {
        Task<PaginationResponseDto<ProductDto>> GetProductsAsync(
            int page,
            int pageSize,
            int? categoryId,
            decimal? minPrice,
            decimal? maxPrice,
            bool? inStock,
            string? search,
            string? sortBy);

        Task<ProductDto> GetProductByIdAsync(int id);

        Task<PaginationResponseDto<ProductDto>> GetProductsByCategoryAsync(
            int categoryId,
            int page,
            int pageSize);

        Task<ProductDto> CreateProductAsync(CreateProductDto dto, int? userId);
        Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto dto, int? userId);
        Task DeleteProductAsync(int id, int? userId);
    }
}

