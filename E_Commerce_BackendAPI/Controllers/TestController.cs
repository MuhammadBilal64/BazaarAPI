using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace E_Commerce_BackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Get() => Ok(new { message = "Swagger is working!" });
    }
}