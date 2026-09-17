using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ServiceRequestManagementSystem.API.DTOs.Auth;
using ServiceRequestManagementSystem.API.Models;

namespace ServiceRequestManagementSystem.API.Services
{
    public class TokenService
    {
        private readonly JwtSettings _settings;

        public TokenService(IOptions<JwtSettings> settings)
        {
            _settings = settings.Value;
        }

        public AuthResponse GenerateToken(
            string username,
            string? role = null,
            string? email = null,
            int? userId = null,
            string? fullName = null,
            string? employeeId = null,
            int? departmentId = null,
            string? departmentName = null)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, username),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (!string.IsNullOrWhiteSpace(role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
                claims.Add(new Claim("role", role));
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Email, email));
                claims.Add(new Claim(ClaimTypes.Email, email));
            }

            if (userId.HasValue)
            {
                claims.Add(new Claim("userId", userId.Value.ToString()));
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
            }

            if (!string.IsNullOrWhiteSpace(fullName))
            {
                claims.Add(new Claim(ClaimTypes.Name, fullName));
                claims.Add(new Claim("fullName", fullName));
            }

            if (!string.IsNullOrWhiteSpace(employeeId))
            {
                claims.Add(new Claim("employeeId", employeeId));
            }

            if (departmentId.HasValue)
            {
                claims.Add(new Claim("departmentId", departmentId.Value.ToString()));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes > 0 ? _settings.ExpiryMinutes : 120);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new AuthResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAt = expires,
                Role = role ?? string.Empty,
                FullName = fullName ?? string.Empty,
                Email = email ?? string.Empty,
                UserId = userId ?? 0,
                EmployeeId = employeeId ?? string.Empty,
                DepartmentId = departmentId,
                DepartmentName = departmentName
            };
        }
    }
}
