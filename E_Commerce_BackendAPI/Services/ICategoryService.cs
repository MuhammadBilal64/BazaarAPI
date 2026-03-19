using E_Commerce_BackendAPI.Dtos;

namespace E_Commerce_BackendAPI.Services
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetActiveCategoriesAsync();
        Task<CategoryDto> GetCategoryByIdAsync(int id);
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto, int? userId);
        Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto dto, int? userId);
        Task DeleteCategoryAsync(int id, int? userId);
    }
}

