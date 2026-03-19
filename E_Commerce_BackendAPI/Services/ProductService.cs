using E_Commerce_BackendAPI.DAL;
using E_Commerce_BackendAPI.Dtos;
using E_Commerce_BackendAPI.Exceptions;
using E_Commerce_BackendAPI.Model;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_BackendAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PaginationResponseDto<ProductDto>> GetProductsAsync(
            int page,
            int pageSize,
            int? categoryId,
            decimal? minPrice,
            decimal? maxPrice,
            bool? inStock,
            string? search,
            string? sortBy)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive);

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            if (inStock.HasValue && inStock.Value)
                query = query.Where(p => p.Stock > 0);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    (p.Description != null && p.Description.Contains(search)));

            switch (sortBy?.ToLower())
            {
                case "priceasc": query = query.OrderBy(p => p.Price); break;
                case "pricedesc": query = query.OrderByDescending(p => p.Price); break;
                case "nameasc": query = query.OrderBy(p => p.Name); break;
                case "namedesc": query = query.OrderByDescending(p => p.Name); break;
                case "newest": query = query.OrderByDescending(p => p.CreatedDate); break;
                default: query = query.OrderBy(p => p.Id); break;
            }

            var totalProducts = await query.CountAsync();

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    CategoryName = p.Category.Name
                })
                .ToListAsync();

            return new PaginationResponseDto<ProductDto>
            {
                TotalCount = totalProducts,
                Page = page,
                PageSize = pageSize,
                Items = products
            };
        }

        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null)
                throw new NotFoundException("Product not found.");

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                CategoryName = product.Category.Name
            };
        }

        public async Task<PaginationResponseDto<ProductDto>> GetProductsByCategoryAsync(
            int categoryId,
            int page,
            int pageSize)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId && p.IsActive);

            var totalProducts = await query.CountAsync();

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    CategoryName = p.Category.Name
                })
                .ToListAsync();

            return new PaginationResponseDto<ProductDto>
            {
                TotalCount = totalProducts,
                Page = page,
                PageSize = pageSize,
                Items = products
            };
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto dto, int? userId)
        {
            var exists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId && c.IsActive);
            if (!exists)
                throw new ArgumentException("Invalid or inactive category.");

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Stock = dto.Stock,
                CategoryId = dto.CategoryId,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = userId ?? 0
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var categoryName = await _context.Categories
                .Where(c => c.Id == product.CategoryId && c.IsActive)
                .Select(c => c.Name)
                .FirstAsync();

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                CategoryName = categoryName
            };
        }

        public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto dto, int? userId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
            if (product == null)
                throw new NotFoundException("Product not found.");

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId && c.IsActive);
            if (!categoryExists)
                throw new ArgumentException("Invalid or inactive category.");

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.Stock = dto.Stock;
            product.CategoryId = dto.CategoryId;
            product.ModifiedDate = DateTime.UtcNow;
            product.ModifiedBy = userId;

            await _context.SaveChangesAsync();

            var categoryName = await _context.Categories
                .Where(c => c.Id == product.CategoryId && c.IsActive)
                .Select(c => c.Name)
                .FirstAsync();

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                CategoryName = categoryName
            };
        }

        public async Task DeleteProductAsync(int id, int? userId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
            if (product == null)
                throw new NotFoundException("Product not found.");

            product.IsActive = false;
            product.ModifiedDate = DateTime.UtcNow;
            product.ModifiedBy = userId;

            await _context.SaveChangesAsync();
        }
    }
}

