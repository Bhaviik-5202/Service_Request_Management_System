using FluentAssertions;
using Microsoft.Extensions.Options;
using ServiceRequestManagementSystem.API.DTOs.Auth;
using ServiceRequestManagementSystem.API.Enums;
using ServiceRequestManagementSystem.API.Models;
using ServiceRequestManagementSystem.API.Repositories.Implementations;
using ServiceRequestManagementSystem.API.Services;
using ServiceRequestManagementSystem.API.Services.Implementations;
using ServiceRequestManagementSystem.Tests.Helpers;
using Xunit;

namespace ServiceRequestManagementSystem.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly IPasswordHasher _passwordHasher = new PasswordHasher();
        private readonly TokenService _tokenService;

        public AuthServiceTests()
        {
            var jwtSettings = Options.Create(new JwtSettings
            {
                Key = "SuperSecretKeyForUnitTestingPurposesOnly123456789!",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                ExpiryMinutes = 60
            });
            _tokenService = new TokenService(jwtSettings);
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldSucceedAndReturnToken()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var uow = new UnitOfWork(context);
            var (hash, salt) = _passwordHasher.HashPassword("Password@123");

            var user = new User
            {
                EmployeeId = "EMP-001",
                FullName = "Admin User",
                Email = "admin@test.com",
                PasswordHash = hash,
                PasswordSalt = salt,
                Role = UserRole.Admin,
                Status = UserStatus.Active,
                JoinedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await uow.Users.AddAsync(user);
            await uow.SaveChangesAsync();

            var authService = new AuthService(uow, context, _tokenService, _passwordHasher);

            // Act
            var result = await authService.LoginAsync(new LoginRequest
            {
                Email = "admin@test.com",
                Password = "Password@123"
            }, "127.0.0.1");

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Token.Should().NotBeNullOrWhiteSpace();
            result.Data.Role.Should().Be("Admin");
            result.Data.Email.Should().Be("admin@test.com");
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ShouldFail()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var uow = new UnitOfWork(context);
            var (hash, salt) = _passwordHasher.HashPassword("CorrectPassword123");

            var user = new User
            {
                EmployeeId = "EMP-002",
                FullName = "Test User",
                Email = "user@test.com",
                PasswordHash = hash,
                PasswordSalt = salt,
                Role = UserRole.Requestor,
                Status = UserStatus.Active,
                JoinedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await uow.Users.AddAsync(user);
            await uow.SaveChangesAsync();

            var authService = new AuthService(uow, context, _tokenService, _passwordHasher);

            // Act
            var result = await authService.LoginAsync(new LoginRequest
            {
                Email = "user@test.com",
                Password = "WrongPassword"
            }, "127.0.0.1");

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Invalid credentials or inactive account.");
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task RegisterAsync_WithNewEmail_ShouldCreateUserAndReturnToken()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var uow = new UnitOfWork(context);
            var authService = new AuthService(uow, context, _tokenService, _passwordHasher);

            var registerDto = new RegisterRequestDto
            {
                FullName = "New Requester",
                Email = "newreq@test.com",
                Password = "SecurePassword@123",
                Role = UserRole.Requestor,
                EmployeeId = "EMP-999"
            };

            // Act
            var result = await authService.RegisterAsync(registerDto, "127.0.0.1");

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Email.Should().Be("newreq@test.com");

            var createdUser = await uow.Users.FirstOrDefaultAsync(u => u.Email == "newreq@test.com");
            createdUser.Should().NotBeNull();
            createdUser!.FullName.Should().Be("New Requester");
        }

        [Fact]
        public async Task RegisterAsync_WithDuplicateEmail_ShouldFail()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var uow = new UnitOfWork(context);
            var (hash, salt) = _passwordHasher.HashPassword("TestPass123");

            await uow.Users.AddAsync(new User
            {
                EmployeeId = "EMP-EXIST",
                FullName = "Existing User",
                Email = "existing@test.com",
                PasswordHash = hash,
                PasswordSalt = salt,
                Role = UserRole.Requestor,
                Status = UserStatus.Active,
                JoinedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await uow.SaveChangesAsync();

            var authService = new AuthService(uow, context, _tokenService, _passwordHasher);

            // Act
            var result = await authService.RegisterAsync(new RegisterRequestDto
            {
                FullName = "Duplicate User",
                Email = "existing@test.com",
                Password = "Password123"
            }, "127.0.0.1");

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("already exists");
        }

        [Fact]
        public async Task ChangePasswordAsync_WithValidCurrentPassword_ShouldUpdatePassword()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var uow = new UnitOfWork(context);
            var (hash, salt) = _passwordHasher.HashPassword("OldPassword123");

            var user = new User
            {
                EmployeeId = "EMP-PWD",
                FullName = "Pwd User",
                Email = "pwd@test.com",
                PasswordHash = hash,
                PasswordSalt = salt,
                Role = UserRole.Requestor,
                Status = UserStatus.Active,
                JoinedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await uow.Users.AddAsync(user);
            await uow.SaveChangesAsync();

            var authService = new AuthService(uow, context, _tokenService, _passwordHasher);

            // Act
            var result = await authService.ChangePasswordAsync(user.UserId, new ChangePasswordDto
            {
                CurrentPassword = "OldPassword123",
                NewPassword = "NewPassword456!"
            }, "127.0.0.1");

            // Assert
            result.Success.Should().BeTrue();
            var updatedUser = await uow.Users.GetByIdAsync(user.UserId);
            _passwordHasher.VerifyPassword("NewPassword456!", updatedUser!.PasswordHash, updatedUser.PasswordSalt).Should().BeTrue();
        }
    }
}
