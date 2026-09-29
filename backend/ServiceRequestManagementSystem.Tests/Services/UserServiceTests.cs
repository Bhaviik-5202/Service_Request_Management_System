using FluentAssertions;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.Users;
using ServiceRequestManagementSystem.API.Enums;
using ServiceRequestManagementSystem.API.Models;
using ServiceRequestManagementSystem.API.Repositories.Implementations;
using ServiceRequestManagementSystem.API.Services;
using ServiceRequestManagementSystem.API.Services.Implementations;
using ServiceRequestManagementSystem.Tests.Helpers;
using Xunit;

namespace ServiceRequestManagementSystem.Tests.Services
{
    public class UserServiceTests
    {
        private readonly IPasswordHasher _passwordHasher = new PasswordHasher();

        [Fact]
        public async Task GetUsersAsync_ShouldFilterByRoleAndSearch()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var uow = new UnitOfWork(context);

            await uow.Users.AddRangeAsync(new[]
            {
                new User { EmployeeId = "E1", FullName = "Alice Admin", Email = "alice@test.com", PasswordHash = "h", PasswordSalt = "s", Role = UserRole.Admin, Status = UserStatus.Active, JoinedDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new User { EmployeeId = "E2", FullName = "Bob Tech", Email = "bob@test.com", PasswordHash = "h", PasswordSalt = "s", Role = UserRole.Technician, Status = UserStatus.Active, JoinedDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            });
            await uow.SaveChangesAsync();

            var userService = new UserService(uow, context, _passwordHasher);

            // Act
            var admins = await userService.GetUsersAsync(null, "Admin", null);
            var searchBob = await userService.GetUsersAsync("Bob", null, null);

            // Assert
            admins.Data.Should().ContainSingle();
            admins.Data!.First().FullName.Should().Be("Alice Admin");

            searchBob.Data.Should().ContainSingle();
            searchBob.Data!.First().FullName.Should().Be("Bob Tech");
        }

        [Fact]
        public async Task CreateUserAsync_ShouldAddUserAndReturnResponse()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var uow = new UnitOfWork(context);
            var userService = new UserService(uow, context, _passwordHasher);

            var createDto = new CreateUserDto
            {
                EmployeeId = "EMP-100",
                FullName = "Charlie User",
                Email = "charlie@test.com",
                Role = UserRole.Requestor,
                Phone = "1234567890"
            };

            // Act
            var result = await userService.CreateUserAsync(createDto, "127.0.0.1");

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.EmployeeId.Should().Be("EMP-100");
            result.Data.FullName.Should().Be("Charlie User");
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldSoftDeleteUser()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var uow = new UnitOfWork(context);

            var user = new User
            {
                EmployeeId = "EMP-DEL",
                FullName = "Delete Me",
                Email = "del@test.com",
                PasswordHash = "h",
                PasswordSalt = "s",
                Role = UserRole.Requestor,
                Status = UserStatus.Active,
                JoinedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await uow.Users.AddAsync(user);
            await uow.SaveChangesAsync();

            var userService = new UserService(uow, context, _passwordHasher);

            // Act
            var result = await userService.DeleteUserAsync(user.UserId, "127.0.0.1");

            // Assert
            result.Success.Should().BeTrue();
            var deletedUser = await uow.Users.GetByIdAsync(user.UserId);
            deletedUser!.IsDeleted.Should().BeTrue();
        }
    }
}
