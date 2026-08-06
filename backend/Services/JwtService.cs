using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Service_Request_Management_System.Services
{
    public class JwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(int userId, string email, string roleName)
        {
            var key = Environment.GetEnvironmentVariable("SESSION_SECRET")
                      ?? _config["Jwt:Key"]
                      ?? throw new InvalidOperationException("JWT signing key is not configured.");

            var issuer = _config["Jwt:Issuer"] ?? "SRMS";
            var audience = _config["Jwt:Audience"] ?? "SRMS";
            var expiryMinutes = int.TryParse(_config["Jwt:ExpiryMinutes"], out var m) ? m : 480;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimTypes.Role, roleName),
                new Claim("userId", userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
