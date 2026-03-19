using E_Commerce_BackendAPI.DAL;
using E_Commerce_BackendAPI.Dtos;
using E_Commerce_BackendAPI.Exceptions;
using E_Commerce_BackendAPI.Model;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_BackendAPI.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryDto>> GetActiveCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.IsActive)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();
        }

        public async Task<CategoryDto> GetCategoryByIdAsync(int id)
        {
            var category = await _context.Categories
                .Where(c => c.Id == id && c.IsActive)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .FirstOrDefaultAsync();

            return category ?? throw new NotFoundException("Category not found.");
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto, int? userId)
        {
            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description ?? string.Empty,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = userId ?? 0
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name
            };
        }

        public async Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto dto, int? userId)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

            if (category == null)
                throw new NotFoundException("Category not found.");

            category.Name = dto.Name;
            category.Description = dto.Description ?? string.Empty;
            category.ModifiedDate = DateTime.UtcNow;
            category.ModifiedBy = userId;

            await _context.SaveChangesAsync();

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name
            };
        }

        public async Task DeleteCategoryAsync(int id, int? userId)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

            if (category == null)
                throw new NotFoundException("Category not found.");

            category.IsActive = false;
            category.ModifiedDate = DateTime.UtcNow;
            category.ModifiedBy = userId;
            await _context.SaveChangesAsync();
        }
    }
}

