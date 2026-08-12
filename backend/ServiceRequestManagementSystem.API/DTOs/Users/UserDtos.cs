using System.ComponentModel.DataAnnotations;
using ServiceRequestManagementSystem.API.Enums;

namespace ServiceRequestManagementSystem.API.DTOs.Users
{
    public class UserResponseDto
    {
        public int UserId { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? Phone { get; set; }
        public UserStatus Status { get; set; }
        public DateTime JoinedDate { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public int RequestsRaised { get; set; }
        public int RequestsResolved { get; set; }
    }

    public class CreateUserDto
    {
        [Required]
        public string EmployeeId { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; } = UserRole.Requestor;

        public int? DepartmentId { get; set; }

        public string? Phone { get; set; }
    }

    public class UpdateUserDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        public int? DepartmentId { get; set; }

        public string? Phone { get; set; }

        public UserStatus Status { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
