using System.IdentityModel.Tokens.Jwt;
using E_Commerce_BackendAPI.Authentication;
using E_Commerce_BackendAPI.DAL;
using E_Commerce_BackendAPI.Dtos;
using E_Commerce_BackendAPI.Model;
using E_Commerce_BackendAPI.Utilities;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_BackendAPI.Services
{
    public class AuthApplicationService : IAuthApplicationService
    {
        private readonly AppDbContext _context;
        private readonly AuthService _jwtService;
        private readonly ILogger<AuthApplicationService> _logger;

        public AuthApplicationService(AppDbContext context, AuthService jwtService, ILogger<AuthApplicationService> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task RegisterAsync(RegisterRequest request)
        {
            var existing = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existing != null)
                throw new ArgumentException("User with this email already exists.");

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

            _logger.LogInformation("User registered. Email: {Email}, UserId: {UserId}", user.Email, user.Id);
        }

        public async Task<AuthTokensResponseDto> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                _logger.LogWarning("Login failed for email: {Email}", request.Email);
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            _logger.LogInformation("User logged in. UserId: {UserId}, Email: {Email}", user.Id, user.Email);

            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return new AuthTokensResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<AuthTokensResponseDto> RefreshTokenAsync(string refreshToken)
        {
            var existingToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (existingToken == null || existingToken.IsRevoked || existingToken.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Refresh token rejected: invalid or expired.");
                throw new UnauthorizedAccessException("Invalid or expired refresh token");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == existingToken.UserId);
            if (user == null)
                throw new UnauthorizedAccessException("User not found");

            existingToken.IsRevoked = true;
            var newAccessToken = _jwtService.GenerateAccessToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            _context.RefreshTokens.Add(new RefreshToken
            {
                Token = newRefreshToken,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            });

            await _context.SaveChangesAsync();

            return new AuthTokensResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }

        public async Task LogoutAsync(string refreshToken, int userId)
        {
            var refreshTokenEntity = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.UserId == userId);

            if (refreshTokenEntity != null)
            {
                refreshTokenEntity.IsRevoked = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}

