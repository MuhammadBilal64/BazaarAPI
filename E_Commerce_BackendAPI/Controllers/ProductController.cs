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



    }
}
