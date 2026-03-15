using System.Security.Claims;
using E_Commerce_BackendAPI.DAL;
using E_Commerce_BackendAPI.Dtos;
using E_Commerce_BackendAPI.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetProducts(
    int page = 1,
    int pageSize = 10,
    int? categoryId = null,
    decimal? minPrice = null,
    decimal? maxPrice = null,
    bool? inStock = null,
    string search = null,
    string sortBy = null)
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

           
            return Ok(new
            {
                TotalCount = totalProducts,
                Page = page,
                PageSize = pageSize,
                Items = products
            });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null)
                return NotFound();

            var productDto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                CategoryName = product.Category.Name
            };

            return Ok(productDto);
        }
        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetProductsByCategory(int categoryId, int page = 1, int pageSize = 10)
        {
            var query = _context.Products.Include(p => p.Category).Where(p => p.CategoryId == categoryId && p.IsActive);
            var totalProducts = await query.CountAsync();
            var products = await query.Skip(((page - 1)) * pageSize).Take(pageSize).Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                CategoryName = p.Category.Name
            }).ToListAsync();
          
            return Ok(new
            {
                TotalCount = totalProducts,
                Page = page,
                PageSize = pageSize,
                Items = products
            });

        }

        /// <summary>Create product (Admin only).</summary>
        [HttpPost]
        [Authorize(Roles = nameof(Utilities.UserRole.Admin))]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
        {
            var userId = GetCurrentUserId();
            var exists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId && c.IsActive);
            if (!exists)
                return BadRequest("Invalid or inactive category.");

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
            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                CategoryName = (await _context.Categories.FindAsync(product.CategoryId))!.Name
            });
        }

        /// <summary>Update product (Admin only).</summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = nameof(Utilities.UserRole.Admin))]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId && c.IsActive);
            if (!categoryExists)
                return BadRequest("Invalid or inactive category.");

            var userId = GetCurrentUserId();
            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.Stock = dto.Stock;
            product.CategoryId = dto.CategoryId;
            product.ModifiedDate = DateTime.UtcNow;
            product.ModifiedBy = userId;
            await _context.SaveChangesAsync();
            var category = await _context.Categories.FindAsync(product.CategoryId);
            return Ok(new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                CategoryName = category?.Name ?? ""
            });
        }

        /// <summary>Soft-delete product (Admin only).</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = nameof(Utilities.UserRole.Admin))]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();
            product.IsActive = false;
            product.ModifiedDate = DateTime.UtcNow;
            product.ModifiedBy = GetCurrentUserId();
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
