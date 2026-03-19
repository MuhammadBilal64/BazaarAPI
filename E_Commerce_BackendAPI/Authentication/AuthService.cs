using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using E_Commerce_BackendAPI.Model;
using Microsoft.IdentityModel.Tokens;

namespace E_Commerce_BackendAPI.Authentication
{
    public class AuthService
    {
        private readonly string secretKey;
        private readonly string issuer;
        private readonly string audience;
        private readonly int expirationMinutes;

        public AuthService(IConfiguration config)
        {
            var jwtSettings = config.GetSection("JwtSettings");
             secretKey = jwtSettings["SecretKey"]!;
             issuer = jwtSettings["Issuer"]!;
             audience = jwtSettings["Audience"]!;
             expirationMinutes = int.Parse(jwtSettings["AccessTokenExpirationMinutes"]);

        }
        public string GenerateAccessToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim (JwtRegisteredClaimNames.Sub,user.Id.ToString()),
                new Claim (ClaimTypes.Role,user.Role.ToString()),
                new Claim (JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
            };
          
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
               expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                               signingCredentials: creds);

                                           return new JwtSecurityTokenHandler().WriteToken(token);


        }
        public string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }

    }
}
