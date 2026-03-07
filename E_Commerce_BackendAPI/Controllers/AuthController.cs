using Azure.Core;
using E_Commerce_BackendAPI.Authentication;
using E_Commerce_BackendAPI.DAL;
using E_Commerce_BackendAPI.Dtos;
using E_Commerce_BackendAPI.Model;
using E_Commerce_BackendAPI.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuthService _jwtService;
        public AuthController(AppDbContext context, AuthService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (result != null)
            {
                return BadRequest("User with this email already exists.");
            }

            var user = new User
            {
                Email = request.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Name = request.Name,
                Role = UserRole.Customer,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully");
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var user =await _context.Users.FirstOrDefaultAsync(u=>u.Email==loginRequest.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.Password))
                return Unauthorized("Invalid credentials");

            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            var RefreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };
            _context.RefreshTokens.Add(RefreshTokenEntity);
           await _context.SaveChangesAsync();
            return Ok(new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }
        [HttpPost("Refresh")]


        public async Task<IActionResult> RefreshToken([FromBody] RefreshRequest refreshrequest)
        {
            var existingToken =await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshrequest.RefreshToken);
            if (existingToken == null || existingToken.IsRevoked || existingToken.ExpiresAt < DateTime.UtcNow)
            {
                return Unauthorized("Invalid or expired refresh token");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == existingToken.UserId);
            if (user == null)
            {
                return Unauthorized("User not found");
            }
            existingToken.IsRevoked = true;
            var newAccessToken = _jwtService.GenerateAccessToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            var newRefreshTokenEntity = new RefreshToken
            {
                Token = newRefreshToken,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            _context.RefreshTokens.Add(newRefreshTokenEntity);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            var refreshTokenEntity = await _context.RefreshTokens
    .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);
            if (refreshTokenEntity != null)
            {
                refreshTokenEntity.IsRevoked = true;
                await _context.SaveChangesAsync();
            }

            return Ok("Logged out successfully");
        }





    }


   
   


}
