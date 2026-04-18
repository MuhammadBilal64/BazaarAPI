using System.Security.Claims;
using E_Commerce_BackendAPI.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using E_Commerce_BackendAPI.Services;

namespace E_Commerce_BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategories()
        {
            return Ok(await _categoryService.GetActiveCategoriesAsync());
        }
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult>GetCategoryById(int id)
        {
            return Ok(await _categoryService.GetCategoryByIdAsync(id));
        }

        /// <summary>Create category (Admin only).</summary>
        [HttpPost]
        [Authorize(Roles = nameof(Utilities.UserRole.Admin))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            var userId = GetCurrentUserId();
            var category = await _categoryService.CreateCategoryAsync(dto, userId);
            return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
        }

        /// <summary>Update category (Admin only).</summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = nameof(Utilities.UserRole.Admin))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
        {
            var userId = GetCurrentUserId();
            return Ok(await _categoryService.UpdateCategoryAsync(id, dto, userId));
        }

        /// <summary>Soft-delete category (Admin only).</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = nameof(Utilities.UserRole.Admin))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            await _categoryService.DeleteCategoryAsync(id, GetCurrentUserId());
            return NoContent();
        }

        private int? GetCurrentUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return int.TryParse(sub, out var id) ? id : null;
        }
    }
}
