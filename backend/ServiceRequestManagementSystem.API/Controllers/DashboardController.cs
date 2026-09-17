using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceRequestManagementSystem.API.Data;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.Dashboard;
using ServiceRequestManagementSystem.API.Enums;

namespace ServiceRequestManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("summary")]
        public async Task<ActionResult<ApiResponseDto<DashboardSummaryDto>>> GetSummary(
            int? userId)
        {
            var requests = _context.ServiceRequests
                .AsNoTracking();

            var totalRequests = await requests.CountAsync();

            var pendingApprovals = await _context.Approvals
                .AsNoTracking()
                .CountAsync(a => a.Status == ApprovalStatus.Pending);

            var activeUsers = await _context.Users
                .AsNoTracking()
                .CountAsync(u => u.Status == UserStatus.Active);

            var totalAssets = await _context.Assets
                .AsNoTracking()
                .CountAsync();

            var openRequests = await requests
                .CountAsync(r =>
                    r.Status != null &&
                    r.Status.StatusName == RequestStatusNames.Open);

            var inProgressRequests = await requests
                .CountAsync(r =>
                    r.Status != null &&
                    r.Status.StatusName == RequestStatusNames.InProgress);

            var resolvedRequests = await requests
                .CountAsync(r =>
                    r.Status != null &&
                    r.Status.StatusName == RequestStatusNames.Resolved);

            var closedRequests = await requests
                .CountAsync(r =>
                    r.Status != null &&
                    r.Status.StatusName == RequestStatusNames.Closed);

            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var resolvedToday = await requests
                .CountAsync(r =>
                    r.Status != null &&
                    r.Status.StatusName == RequestStatusNames.Resolved &&
                    r.ResolvedAt.HasValue &&
                    r.ResolvedAt.Value >= today &&
                    r.ResolvedAt.Value < tomorrow);

            var myAssignedRequests = userId.HasValue
                ? await requests.CountAsync(r =>
                    r.AssigneeUserId == userId.Value)
                : 0;

            var priorityBreakdown = await requests
                .GroupBy(r => r.Priority)
                .Select(g => new
                {
                    Priority = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToDictionaryAsync(
                    x => x.Priority,
                    x => x.Count);

            var statusBreakdown = await requests
                .Where(r => r.Status != null)
                .GroupBy(r => r.Status!.StatusName)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToDictionaryAsync(
                    x => x.Status,
                    x => x.Count);

            var resolvedData = await requests
                .Where(r =>
                    r.Status != null &&
                    r.Status.StatusName == RequestStatusNames.Resolved &&
                    r.ResolvedAt.HasValue)
                .Select(r => new
                {
                    r.Priority,
                    r.CreatedAt,
                    ResolvedAt = r.ResolvedAt!.Value
                })
                .ToListAsync();

            var averageResolutionHours = resolvedData.Count == 0
                ? 0
                : resolvedData.Average(r =>
                    (r.ResolvedAt - r.CreatedAt).TotalHours);

            var compliantTickets = resolvedData.Count(r =>
                (r.ResolvedAt - r.CreatedAt).TotalHours <=
                GetSlaHours(r.Priority));

            var slaComplianceRate = resolvedData.Count == 0
                ? 0
                : compliantTickets * 100.0 / resolvedData.Count;

            var summary = new DashboardSummaryDto
            {
                TotalRequests = totalRequests,
                PendingApprovals = pendingApprovals,
                ActiveUsers = activeUsers,
                TotalAssets = totalAssets,
                OpenRequests = openRequests,
                InProgressRequests = inProgressRequests,
                ResolvedRequests = resolvedRequests,
                ClosedRequests = closedRequests,
                MyAssignedRequests = myAssignedRequests,
                ResolvedToday = resolvedToday,
                AvgResolutionTimeHours =
                    Math.Round(averageResolutionHours, 2),
                SlaComplianceRate =
                    Math.Round(slaComplianceRate, 2),
                PriorityBreakdown = priorityBreakdown,
                StatusBreakdown = statusBreakdown
            };

            return Ok(new ApiResponseDto<DashboardSummaryDto>
            {
                Success = true,
                Message = "Dashboard summary fetched successfully.",
                Data = summary
            });
        }

        private static int GetSlaHours(Priority priority)
        {
            return priority switch
            {
                Priority.Critical => 4,
                Priority.High => 8,
                Priority.Medium => 24,
                Priority.Low => 48,
                _ => 24
            };
        }
    }
}
