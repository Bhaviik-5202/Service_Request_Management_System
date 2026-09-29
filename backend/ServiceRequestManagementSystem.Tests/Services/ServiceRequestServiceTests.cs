using FluentAssertions;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.ServiceRequests;
using ServiceRequestManagementSystem.API.Enums;
using ServiceRequestManagementSystem.API.Models;
using ServiceRequestManagementSystem.API.Repositories.Implementations;
using ServiceRequestManagementSystem.API.Services.Implementations;
using ServiceRequestManagementSystem.API.Validator;
using ServiceRequestManagementSystem.Tests.Helpers;
using Xunit;

namespace ServiceRequestManagementSystem.Tests.Services
{
    public class ServiceRequestServiceTests
    {
        private readonly CreateServiceRequestDtoValidator _createValidator = new();
        private readonly UpdateServiceRequestStatusDtoValidator _statusValidator = new();
        private readonly AssignTechnicianDtoValidator _assignValidator = new();
        private readonly CreateReplyDtoValidator _replyValidator = new();

        [Fact]
        public async Task CreateRequestAsync_WhenValid_ShouldGenerateFormattedRequestNumberAndAddTimeline()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var uow = new UnitOfWork(context);

            var dept = new Department { DepartmentName = "IT Dept", DepartmentCode = "IT", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            var st = new ServiceType { ServiceTypeName = "Hardware", ServiceTypeCode = "HW", IsActive = true, CreatedAt = DateTime.UtcNow };
            var rt = new RequestType { ServiceTypeId = 1, RequestTypeName = "Laptop Repair", RequiresApproval = false, IsActive = true, CreatedAt = DateTime.UtcNow };
            var statusOpen = new ServiceRequestStatus { StatusName = RequestStatusNames.Open, IsActive = true, CreatedAt = DateTime.UtcNow };
            var user = new User { EmployeeId = "REQ-1", FullName = "Requester One", Email = "req1@test.com", PasswordHash = "h", PasswordSalt = "s", Role = UserRole.Requestor, Status = UserStatus.Active, JoinedDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };

            await uow.Departments.AddAsync(dept);
            await uow.ServiceTypes.AddAsync(st);
            await uow.RequestTypes.AddAsync(rt);
            await uow.ServiceRequestStatuses.AddAsync(statusOpen);
            await uow.Users.AddAsync(user);
            await uow.SaveChangesAsync();

            var service = new ServiceRequestService(uow, context, _createValidator, _statusValidator, _assignValidator, _replyValidator);

            var createDto = new CreateServiceRequestDto
            {
                Title = "My Laptop is not booting",
                Description = "When I press the power button, nothing happens and the screen stays black.",
                ServiceTypeId = st.ServiceTypeId,
                RequestTypeId = rt.RequestTypeId,
                DepartmentId = dept.DepartmentId,
                RequesterUserId = user.UserId,
                Priority = Priority.High
            };

            // Act
            var result = await service.CreateRequestAsync(createDto, "127.0.0.1");

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.RequestNumber.Should().StartWith("SR-");
            result.Data.Title.Should().Be("My Laptop is not booting");
            result.Data.Status.Should().Be(RequestStatusNames.Open);

            var timeline = await uow.ServiceRequestTimeline.FindAsync(t => t.RequestId == result.Data.RequestId);
            timeline.Should().ContainSingle();
            timeline.First().StatusName.Should().Be(RequestStatusNames.Open);
        }

        [Fact]
        public async Task CreateRequestAsync_WhenRequiresApproval_ShouldCreateApprovalAndSetPendingApprovalStatus()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var uow = new UnitOfWork(context);

            var dept = new Department { DepartmentName = "Finance", DepartmentCode = "FIN", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            var st = new ServiceType { ServiceTypeName = "Software", ServiceTypeCode = "SW", IsActive = true, CreatedAt = DateTime.UtcNow };
            var rt = new RequestType { ServiceTypeId = 1, RequestTypeName = "Software License", RequiresApproval = true, IsActive = true, CreatedAt = DateTime.UtcNow };
            var statusPending = new ServiceRequestStatus { StatusName = RequestStatusNames.PendingApproval, IsActive = true, CreatedAt = DateTime.UtcNow };
            var user = new User { EmployeeId = "REQ-2", FullName = "Requester Two", Email = "req2@test.com", PasswordHash = "h", PasswordSalt = "s", Role = UserRole.Requestor, Status = UserStatus.Active, JoinedDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };

            await uow.Departments.AddAsync(dept);
            await uow.ServiceTypes.AddAsync(st);
            await uow.RequestTypes.AddAsync(rt);
            await uow.ServiceRequestStatuses.AddAsync(statusPending);
            await uow.Users.AddAsync(user);
            await uow.SaveChangesAsync();

            var service = new ServiceRequestService(uow, context, _createValidator, _statusValidator, _assignValidator, _replyValidator);

            var createDto = new CreateServiceRequestDto
            {
                Title = "Need Visual Studio Enterprise License",
                Description = "Requesting annual license for development and testing tasks.",
                ServiceTypeId = st.ServiceTypeId,
                RequestTypeId = rt.RequestTypeId,
                DepartmentId = dept.DepartmentId,
                RequesterUserId = user.UserId,
                Priority = Priority.Medium
            };

            // Act
            var result = await service.CreateRequestAsync(createDto, "127.0.0.1");

            // Assert
            result.Success.Should().BeTrue();
            result.Data!.Status.Should().Be(RequestStatusNames.PendingApproval);

            var approval = await uow.Approvals.FirstOrDefaultAsync(a => a.RequestId == result.Data.RequestId);
            approval.Should().NotBeNull();
            approval!.Status.Should().Be(ApprovalStatus.Pending);
        }

        [Fact]
        public async Task UpdateStatusAsync_ShouldUpdateStatusAndLogTimeline()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var uow = new UnitOfWork(context);

            var statusOpen = new ServiceRequestStatus { StatusName = RequestStatusNames.Open, IsActive = true, CreatedAt = DateTime.UtcNow };
            var statusResolved = new ServiceRequestStatus { StatusName = RequestStatusNames.Resolved, IsActive = true, CreatedAt = DateTime.UtcNow };
            await uow.ServiceRequestStatuses.AddRangeAsync(new[] { statusOpen, statusResolved });

            var request = new ServiceRequest
            {
                RequestNumber = "SR-2026-000001",
                Title = "Network Issue",
                Description = "Cannot connect to the office Wi-Fi network from the second floor.",
                ServiceTypeId = 1,
                RequestTypeId = 1,
                DepartmentId = 1,
                RequesterUserId = 1,
                StatusId = statusOpen.StatusId,
                Priority = Priority.High,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await uow.ServiceRequests.AddAsync(request);
            await uow.SaveChangesAsync();

            var service = new ServiceRequestService(uow, context, _createValidator, _statusValidator, _assignValidator, _replyValidator);

            // Act
            var updateResult = await service.UpdateStatusAsync(request.RequestId, new UpdateServiceRequestStatusDto
            {
                StatusId = statusResolved.StatusId,
                Note = "Replaced router switch."
            }, 1, "127.0.0.1");

            // Assert
            updateResult.Success.Should().BeTrue();
            var updatedRequest = await uow.ServiceRequests.GetByIdAsync(request.RequestId);
            updatedRequest!.StatusId.Should().Be(statusResolved.StatusId);
            updatedRequest.ResolvedAt.Should().NotBeNull();
        }
    }
}
