using FluentAssertions;
using ServiceRequestManagementSystem.API.DTOs.Assets;
using ServiceRequestManagementSystem.API.Enums;
using ServiceRequestManagementSystem.API.Models;
using ServiceRequestManagementSystem.API.Repositories.Implementations;
using ServiceRequestManagementSystem.API.Services.Implementations;
using ServiceRequestManagementSystem.API.Validator;
using ServiceRequestManagementSystem.Tests.Helpers;
using Xunit;

namespace ServiceRequestManagementSystem.Tests.Services
{
    public class AssetServiceTests
    {
        private readonly CreateAssetDtoValidator _createValidator = new();
        private readonly UpdateAssetDtoValidator _updateValidator = new();
        private readonly AssignAssetDtoValidator _assignValidator = new();

        [Fact]
        public async Task CreateAssetAsync_WithUniqueTag_ShouldCreateAsset()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var uow = new UnitOfWork(context);
            var service = new AssetService(uow, context, _createValidator, _updateValidator, _assignValidator);

            var createDto = new CreateAssetDto
            {
                AssetTag = "AST-001",
                AssetName = "Dell Laptop",
                Category = "Laptop",
                SerialNumber = "SN123456",
                Status = AssetStatus.Available,
                PurchaseDate = DateTime.UtcNow.AddMonths(-6),
                WarrantyUntil = DateTime.UtcNow.AddYears(2),
                BookValue = 65000.00m
            };

            // Act
            var result = await service.CreateAssetAsync(createDto, "127.0.0.1");

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.AssetTag.Should().Be("AST-001");
            result.Data.AssetName.Should().Be("Dell Laptop");
        }

        [Fact]
        public async Task AssignAssetAsync_ShouldAssignUserAndSetInUseStatus()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var uow = new UnitOfWork(context);

            var user = new User
            {
                EmployeeId = "EMP-ASST",
                FullName = "Asset User",
                Email = "asst@test.com",
                PasswordHash = "h",
                PasswordSalt = "s",
                Role = UserRole.Requestor,
                Status = UserStatus.Active,
                JoinedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await uow.Users.AddAsync(user);

            var asset = new Asset
            {
                AssetTag = "AST-002",
                AssetName = "HP Monitor",
                Category = "Monitor",
                SerialNumber = "SN987654",
                Status = AssetStatus.Available,
                PurchaseDate = DateTime.UtcNow.AddMonths(-3),
                WarrantyUntil = DateTime.UtcNow.AddYears(1),
                BookValue = 12000.00m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await uow.Assets.AddAsync(asset);
            await uow.SaveChangesAsync();

            var service = new AssetService(uow, context, _createValidator, _updateValidator, _assignValidator);

            // Act
            var assignDto = new AssignAssetDto { AssignedToUserId = user.UserId };
            var result = await service.AssignAssetAsync(asset.AssetId, assignDto, "127.0.0.1");

            // Assert
            result.Success.Should().BeTrue();
            var updatedAsset = await uow.Assets.GetByIdAsync(asset.AssetId);
            updatedAsset!.AssignedToUserId.Should().Be(user.UserId);
            updatedAsset.Status.Should().Be(AssetStatus.InUse);
        }
    }
}
