using E_Commerce_BackendAPI.DAL;
using E_Commerce_BackendAPI.Dtos;
using Microsoft.AspNetCore.Http;
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
        [HttpGet("GetAll")]
        public async Task<IActionResult>GetProducts(int page=1 ,int pagesize=10)
        {
            var query = _context.Products.Include(p => p.Category).Where(p => p.IsActive);
            var totalproducts = await query.CountAsync();
            var products = await query.Skip((page - 1) * pagesize).Take(pagesize).Select(p => new ProductDto
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
                TotalCount = totalproducts,
                Page = page,
                PageSize = pagesize,
                Items = products

            });



        }
        [HttpGet("{id")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _context.Products.Include(u=>u.Category)
                .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
            if(product == null)
            {
                return NotFound();
            }
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


    }
}
