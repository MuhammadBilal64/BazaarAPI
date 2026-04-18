using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using E_Commerce_BackendAPI.Dtos;
using E_Commerce_BackendAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce_BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthApplicationService _authService;

        public AuthController(IAuthApplicationService authService)
        {
            _authService = authService;
        }
        [HttpPost("Register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            await _authService.RegisterAsync(request);
            return Ok("User registered successfully");
        }

        [HttpPost("Login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var tokens = await _authService.LoginAsync(loginRequest);
            return Ok(new
            {
                AccessToken = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken
            });
        }
        [HttpPost("Refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshRequest refreshrequest)
        {
            var tokens = await _authService.RefreshTokenAsync(refreshrequest.RefreshToken);

            return Ok(new
            {
                AccessToken = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken
            });
        }
        [HttpPost("Logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim);
            await _authService.LogoutAsync(request.RefreshToken, userId);

            return Ok("Logged out successfully");
        }





    }


   
   


}
