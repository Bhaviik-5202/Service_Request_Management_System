using FluentAssertions;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.Enums;
using ServiceRequestManagementSystem.API.Models;
using ServiceRequestManagementSystem.API.Repositories.Implementations;
using ServiceRequestManagementSystem.API.Services.Implementations;
using ServiceRequestManagementSystem.Tests.Helpers;
using Xunit;

namespace ServiceRequestManagementSystem.Tests.Services
{
    public class DashboardServiceTests
    {
        [Fact]
        public async Task GetSummaryAsync_ShouldCalculateAggregatedMetricsCorrectly()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var uow = new UnitOfWork(context);

            var statusOpen = new ServiceRequestStatus { StatusName = RequestStatusNames.Open, IsActive = true, CreatedAt = DateTime.UtcNow };
            var statusResolved = new ServiceRequestStatus { StatusName = RequestStatusNames.Resolved, IsActive = true, CreatedAt = DateTime.UtcNow };
            await uow.ServiceRequestStatuses.AddRangeAsync(new[] { statusOpen, statusResolved });

            var user1 = new User { EmployeeId = "U1", FullName = "U One", Email = "u1@test.com", PasswordHash = "h", PasswordSalt = "s", Role = UserRole.Requestor, Status = UserStatus.Active, JoinedDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            var tech1 = new User { EmployeeId = "T1", FullName = "Tech One", Email = "t1@test.com", PasswordHash = "h", PasswordSalt = "s", Role = UserRole.Technician, Status = UserStatus.Active, JoinedDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            await uow.Users.AddRangeAsync(new[] { user1, tech1 });

            var asset1 = new Asset { AssetTag = "A1", AssetName = "Laptop", Category = "Laptop", SerialNumber = "S1", Status = AssetStatus.InUse, PurchaseDate = DateTime.UtcNow, WarrantyUntil = DateTime.UtcNow, BookValue = 50000m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            await uow.Assets.AddAsync(asset1);

            var req1 = new ServiceRequest
            {
                RequestNumber = "SR-01",
                Title = "T1",
                Description = "Description 1",
                ServiceTypeId = 1,
                RequestTypeId = 1,
                DepartmentId = 1,
                RequesterUserId = 1,
                AssigneeUserId = tech1.UserId,
                StatusId = statusOpen.StatusId,
                Priority = Priority.High,
                CreatedAt = DateTime.UtcNow.AddHours(-10),
                UpdatedAt = DateTime.UtcNow
            };

            var req2 = new ServiceRequest
            {
                RequestNumber = "SR-02",
                Title = "T2",
                Description = "Description 2",
                ServiceTypeId = 1,
                RequestTypeId = 1,
                DepartmentId = 1,
                RequesterUserId = 1,
                AssigneeUserId = tech1.UserId,
                StatusId = statusResolved.StatusId,
                Priority = Priority.Medium,
                CreatedAt = DateTime.UtcNow.AddHours(-5),
                UpdatedAt = DateTime.UtcNow,
                ResolvedAt = DateTime.UtcNow.AddHours(-1)
            };

            await uow.ServiceRequests.AddRangeAsync(new[] { req1, req2 });
            await uow.SaveChangesAsync();

            var service = new DashboardService(context);

            // Act
            var result = await service.GetSummaryAsync(tech1.UserId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.TotalRequests.Should().Be(2);
            result.Data.OpenRequests.Should().Be(1);
            result.Data.ResolvedRequests.Should().Be(1);
            result.Data.ActiveUsers.Should().Be(2);
            result.Data.TotalAssets.Should().Be(1);
            result.Data.MyAssignedRequests.Should().Be(2);
            result.Data.PriorityBreakdown.Should().ContainKey("High");
            result.Data.PriorityBreakdown.Should().ContainKey("Medium");
        }
    }
}
