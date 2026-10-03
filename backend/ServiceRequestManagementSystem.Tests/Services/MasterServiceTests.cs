using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using ServiceRequestManagementSystem.API.DTOs.Masters;
using ServiceRequestManagementSystem.API.Models;
using ServiceRequestManagementSystem.API.Repositories.Implementations;
using ServiceRequestManagementSystem.API.Services.Implementations;
using ServiceRequestManagementSystem.API.Services.Interfaces;
using ServiceRequestManagementSystem.Tests.Helpers;
using Xunit;

namespace ServiceRequestManagementSystem.Tests.Services
{
    public class MasterServiceTests
    {
        private static ICacheService CreateCacheService()
        {
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            return new MemoryCacheService(memoryCache, NullLogger<MemoryCacheService>.Instance);
        }

        [Fact]
        public async Task Department_CRUD_ShouldPerformSuccessfully()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var uow = new UnitOfWork(context);
            var cache = CreateCacheService();
            var service = new MasterService(uow, context, cache);

            // Create
            var createResult = await service.CreateDepartmentAsync(new DepartmentDto
            {
                DepartmentName = "Information Technology",
                DepartmentCode = "IT",
                Description = "Handles computing infrastructure",
                IsActive = true
            });
            createResult.Success.Should().BeTrue();
            createResult.Data.Should().NotBeNull();
            var deptId = createResult.Data!.DepartmentId;

            // Get
            var getResult = await service.GetDepartmentByIdAsync(deptId);
            getResult.Success.Should().BeTrue();
            getResult.Data!.DepartmentName.Should().Be("Information Technology");

            // Update
            var updateResult = await service.UpdateDepartmentAsync(deptId, new DepartmentDto
            {
                DepartmentName = "IT & Systems",
                DepartmentCode = "ITS",
                Description = "Updated description",
                IsActive = true
            });
            updateResult.Success.Should().BeTrue();
            updateResult.Data!.DepartmentName.Should().Be("IT & Systems");

            // Delete (Soft)
            var deleteResult = await service.DeleteDepartmentAsync(deptId);
            deleteResult.Success.Should().BeTrue();

            var afterDelete = await service.GetDepartmentByIdAsync(deptId);
            afterDelete.Success.Should().BeFalse();
        }

        [Fact]
        public async Task Status_CRUD_ShouldPerformSuccessfully()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var uow = new UnitOfWork(context);
            var cache = CreateCacheService();
            var service = new MasterService(uow, context, cache);

            // Create
            var createResult = await service.CreateStatusAsync(new StatusDto
            {
                StatusName = "Under Review",
                ColorCode = "#FF5733",
                Description = "Waiting for supervisor review",
                IsActive = true
            });
            createResult.Success.Should().BeTrue();

            // List
            var listResult = await service.GetStatusesAsync();
            listResult.Success.Should().BeTrue();
            listResult.Data.Should().ContainSingle();
        }

        [Fact]
        public async Task ServiceType_And_RequestType_Creation_ShouldLinkCorrectly()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var uow = new UnitOfWork(context);
            var cache = CreateCacheService();
            var service = new MasterService(uow, context, cache);

            // Create Service Type
            var stResult = await service.CreateServiceTypeAsync(new ServiceTypeDto
            {
                ServiceTypeName = "Facility Management",
                ServiceTypeCode = "FAC",
                Description = "Building and facilities",
                IsActive = true
            });
            stResult.Success.Should().BeTrue();
            var stId = stResult.Data!.ServiceTypeId;

            // Create Request Type
            var rtResult = await service.CreateRequestTypeAsync(new RequestTypeDto
            {
                ServiceTypeId = stId,
                RequestTypeName = "AC Maintenance",
                Description = "HVAC cooling issue",
                RequiresApproval = false,
                IsActive = true
            });
            rtResult.Success.Should().BeTrue();
            rtResult.Data!.ServiceTypeName.Should().Be("Facility Management");
        }
    }
}
