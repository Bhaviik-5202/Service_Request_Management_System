namespace ServiceRequestManagementSystem.API.DTOs.Auth
{
    public class LoginRequest
    {
        public string? Email { get; set; }
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string? Role { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public int? UserId { get; set; }
    }
}
