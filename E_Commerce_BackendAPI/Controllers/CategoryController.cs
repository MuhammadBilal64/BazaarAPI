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
    public class CategoryController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CategoryController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();

            return Ok(categories);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult>GetCategoryById(int id)
        {
            var category =await _context.Categories.Where(c => c.Id == id && c.IsActive)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .FirstOrDefaultAsync();

            if(category == null)
                return NotFound();
            return Ok(category);
        }

        /// <summary>Create category (Admin only).</summary>
        [HttpPost]
        [Authorize(Roles = nameof(Utilities.UserRole.Admin))]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            var userId = GetCurrentUserId();
            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description ?? "",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = userId ?? 0
            };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, new CategoryDto { Id = category.Id, Name = category.Name });
        }

        /// <summary>Update category (Admin only).</summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = nameof(Utilities.UserRole.Admin))]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();
            var userId = GetCurrentUserId();
            category.Name = dto.Name;
            category.Description = dto.Description ?? "";
            category.ModifiedDate = DateTime.UtcNow;
            category.ModifiedBy = userId;
            await _context.SaveChangesAsync();
            return Ok(new CategoryDto { Id = category.Id, Name = category.Name });
        }

        /// <summary>Soft-delete category (Admin only).</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = nameof(Utilities.UserRole.Admin))]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();
            category.IsActive = false;
            category.ModifiedDate = DateTime.UtcNow;
            category.ModifiedBy = GetCurrentUserId();
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
