using Microsoft.AspNetCore.Mvc;

namespace E_Commerce_BackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok(new { message = "Swagger is working!" });
    }
}