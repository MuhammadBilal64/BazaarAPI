using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using E_Commerce_BackendAPI.Model;
using Microsoft.IdentityModel.Tokens;

namespace E_Commerce_BackendAPI.Authentication
{
    public class AuthService
    {
        private readonly IConfiguration _config;
        public AuthService(IConfiguration config)
        {
            _config = config;
        }
        public string GenerateAccessToken(User user)
        {
            var jwtSettings =_config.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expirationMinutes = int.Parse(jwtSettings["AccessTokenExpirationMinutes"]);

            var claims = new[]
            {
                new Claim (JwtRegisteredClaimNames.Sub,user.Id.ToString()),
                new Claim (ClaimTypes.Role,user.Role.ToString()),
                new Claim (JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
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
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }

    }
}
