namespace E_Commerce_BackendAPI.Dtos
{
    public class AuthTokensResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}

