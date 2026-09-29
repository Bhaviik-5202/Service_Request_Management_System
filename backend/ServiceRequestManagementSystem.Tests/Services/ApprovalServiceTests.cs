using FluentAssertions;
using ServiceRequestManagementSystem.API.DTOs.Approvals;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.Enums;
using ServiceRequestManagementSystem.API.Models;
using ServiceRequestManagementSystem.API.Repositories.Implementations;
using ServiceRequestManagementSystem.API.Services.Implementations;
using ServiceRequestManagementSystem.Tests.Helpers;
using Xunit;

namespace ServiceRequestManagementSystem.Tests.Services
{
    public class ApprovalServiceTests
    {
        [Fact]
        public async Task MakeDecisionAsync_WhenApproved_ShouldUpdateApprovalAndSetRequestToOpen()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var uow = new UnitOfWork(context);

            var statusPending = new ServiceRequestStatus { StatusName = RequestStatusNames.PendingApproval, IsActive = true, CreatedAt = DateTime.UtcNow };
            var statusOpen = new ServiceRequestStatus { StatusName = RequestStatusNames.Open, IsActive = true, CreatedAt = DateTime.UtcNow };
            await uow.ServiceRequestStatuses.AddRangeAsync(new[] { statusPending, statusOpen });

            var hodUser = new User
            {
                EmployeeId = "HOD-1",
                FullName = "HOD User",
                Email = "hod@test.com",
                PasswordHash = "h",
                PasswordSalt = "s",
                Role = UserRole.HOD,
                Status = UserStatus.Active,
                JoinedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await uow.Users.AddAsync(hodUser);

            var request = new ServiceRequest
            {
                RequestNumber = "SR-2026-000005",
                Title = "Hardware Request",
                Description = "New monitor needed for design workstation.",
                ServiceTypeId = 1,
                RequestTypeId = 1,
                DepartmentId = 1,
                RequesterUserId = 1,
                StatusId = statusPending.StatusId,
                Priority = Priority.High,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await uow.ServiceRequests.AddAsync(request);
            await uow.SaveChangesAsync();

            var approval = new Approval
            {
                RequestId = request.RequestId,
                Status = ApprovalStatus.Pending,
                SubmittedAt = DateTime.UtcNow
            };
            await uow.Approvals.AddAsync(approval);
            await uow.SaveChangesAsync();

            var service = new ApprovalService(uow, context);

            // Act
            var decisionDto = new ApprovalDecisionDto
            {
                Decision = ApprovalStatus.Approved,
                DecidedByUserId = hodUser.UserId,
                Remarks = "Approved as requested."
            };
            var result = await service.MakeDecisionAsync(approval.ApprovalId, decisionDto, "127.0.0.1");

            // Assert
            result.Success.Should().BeTrue();

            var updatedApproval = await uow.Approvals.GetByIdAsync(approval.ApprovalId);
            updatedApproval!.Status.Should().Be(ApprovalStatus.Approved);
            updatedApproval.Remarks.Should().Be("Approved as requested.");

            var updatedRequest = await uow.ServiceRequests.GetByIdAsync(request.RequestId);
            updatedRequest!.StatusId.Should().Be(statusOpen.StatusId);
        }

        [Fact]
        public async Task MakeDecisionAsync_WhenRejected_ShouldUpdateApprovalAndSetRequestToRejected()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var uow = new UnitOfWork(context);

            var statusPending = new ServiceRequestStatus { StatusName = RequestStatusNames.PendingApproval, IsActive = true, CreatedAt = DateTime.UtcNow };
            var statusRejected = new ServiceRequestStatus { StatusName = RequestStatusNames.Rejected, IsActive = true, CreatedAt = DateTime.UtcNow };
            await uow.ServiceRequestStatuses.AddRangeAsync(new[] { statusPending, statusRejected });

            var hodUser = new User
            {
                EmployeeId = "HOD-2",
                FullName = "HOD Two",
                Email = "hod2@test.com",
                PasswordHash = "h",
                PasswordSalt = "s",
                Role = UserRole.HOD,
                Status = UserStatus.Active,
                JoinedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await uow.Users.AddAsync(hodUser);

            var request = new ServiceRequest
            {
                RequestNumber = "SR-2026-000006",
                Title = "Expensive Software Request",
                Description = "Requesting expensive software suite.",
                ServiceTypeId = 1,
                RequestTypeId = 1,
                DepartmentId = 1,
                RequesterUserId = 1,
                StatusId = statusPending.StatusId,
                Priority = Priority.Medium,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await uow.ServiceRequests.AddAsync(request);
            await uow.SaveChangesAsync();

            var approval = new Approval
            {
                RequestId = request.RequestId,
                Status = ApprovalStatus.Pending,
                SubmittedAt = DateTime.UtcNow
            };
            await uow.Approvals.AddAsync(approval);
            await uow.SaveChangesAsync();

            var service = new ApprovalService(uow, context);

            // Act
            var decisionDto = new ApprovalDecisionDto
            {
                Decision = ApprovalStatus.Rejected,
                DecidedByUserId = hodUser.UserId,
                Remarks = "Budget limit exceeded."
            };
            var result = await service.MakeDecisionAsync(approval.ApprovalId, decisionDto, "127.0.0.1");

            // Assert
            result.Success.Should().BeTrue();

            var updatedApproval = await uow.Approvals.GetByIdAsync(approval.ApprovalId);
            updatedApproval!.Status.Should().Be(ApprovalStatus.Rejected);
            updatedApproval.Remarks.Should().Be("Budget limit exceeded.");

            var updatedRequest = await uow.ServiceRequests.GetByIdAsync(request.RequestId);
            updatedRequest!.StatusId.Should().Be(statusRejected.StatusId);
        }
    }
}
