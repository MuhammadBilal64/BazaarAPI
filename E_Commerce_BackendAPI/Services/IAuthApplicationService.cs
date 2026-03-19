using E_Commerce_BackendAPI.Dtos;

namespace E_Commerce_BackendAPI.Services
{
    public interface IAuthApplicationService
    {
        Task RegisterAsync(RegisterRequest request);
        Task<AuthTokensResponseDto> LoginAsync(LoginRequest request);
        Task<AuthTokensResponseDto> RefreshTokenAsync(string refreshToken);
        Task LogoutAsync(string refreshToken, int userId);
    }
}

