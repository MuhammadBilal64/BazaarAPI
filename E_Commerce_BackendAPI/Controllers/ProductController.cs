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
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
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
            return Ok(await _productService.GetProductsAsync(
                page,
                pageSize,
                categoryId,
                minPrice,
                maxPrice,
                inStock,
                search,
                sortBy));
        }
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductById(int id)
        {
            return Ok(await _productService.GetProductByIdAsync(id));
        }
        [HttpGet("category/{categoryId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProductsByCategory(int categoryId, int page = 1, int pageSize = 10)
        {
            return Ok(await _productService.GetProductsByCategoryAsync(categoryId, page, pageSize));

        }

        /// <summary>Create product (Admin only).</summary>
        [HttpPost]
        [Authorize(Roles = nameof(Utilities.UserRole.Admin))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
        {
            var userId = GetCurrentUserId();
            var product = await _productService.CreateProductAsync(dto, userId);
            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
        }

        /// <summary>Update product (Admin only).</summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = nameof(Utilities.UserRole.Admin))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto dto)
        {
            var userId = GetCurrentUserId();
            return Ok(await _productService.UpdateProductAsync(id, dto, userId));
        }

        /// <summary>Soft-delete product (Admin only).</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = nameof(Utilities.UserRole.Admin))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            await _productService.DeleteProductAsync(id, GetCurrentUserId());
            return NoContent();
        }

        private int? GetCurrentUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return int.TryParse(sub, out var id) ? id : null;
        }
    }
}
