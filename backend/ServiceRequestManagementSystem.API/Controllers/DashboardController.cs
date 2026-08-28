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
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("summary")]
        public async Task<ActionResult<ApiResponseDto<DashboardSummaryDto>>> GetSummary()
        {
            var requests = _context.ServiceRequests
                .Where(r => !r.IsDeleted);

            var totalRequests = await requests.CountAsync();

            var pendingApprovals = await _context.Approvals
                .CountAsync(a => a.Status == ApprovalStatus.Pending);

            var activeUsers = await _context.Users
                .CountAsync(u => !u.IsDeleted && u.Status == UserStatus.Active);

            var totalAssets = await _context.Assets
                .CountAsync(a => !a.IsDeleted);

            var openRequests = await requests
                .CountAsync(r => r.Status!.StatusName == "Open");

            var inProgressRequests = await requests
                .CountAsync(r => r.Status!.StatusName == "In Progress");

            var resolvedRequests = await requests
                .CountAsync(r => r.Status!.StatusName == "Resolved");

            var closedRequests = await requests
                .CountAsync(r => r.Status!.StatusName == "Closed");

            var resolvedToday = await requests
                .CountAsync(r => r.Status!.StatusName == "Resolved" && r.UpdatedAt.Date == DateTime.UtcNow.Date);

            var myAssignedRequests = await requests
                .CountAsync(r => r.AssigneeUserId != null);

            var priorityBreakdown = await requests
                .GroupBy(r => r.Priority)
                .Select(g => new
                {
                    Priority = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.Priority, x => x.Count);

            var statusBreakdown = await requests
                .Include(r => r.Status)
                .GroupBy(r => r.Status!.StatusName)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.Status, x => x.Count);

            var averageResolutionHours = await requests
                .Where(r => r.Status!.StatusName == "Resolved")
                .Select(r => EF.Functions.DateDiffHour(r.CreatedAt, r.UpdatedAt))
                .DefaultIfEmpty()
                .AverageAsync();

            var totalResolved = await requests
                .CountAsync(r => r.Status!.StatusName == "Resolved");

            var slaComplianceRate = totalResolved == 0
                ? 0
                : 100.0;

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
                AvgResolutionTimeHours = averageResolutionHours,
                SlaComplianceRate = slaComplianceRate,
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
    }
}
