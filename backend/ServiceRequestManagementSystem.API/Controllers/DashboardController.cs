using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceRequestManagementSystem.API.Data;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.Dashboard;
using ServiceRequestManagementSystem.API.Enums;

namespace ServiceRequestManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/v1/dashboard")]
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
            var totalRequests = await _context.ServiceRequests.CountAsync();
            var pendingApprovals = await _context.Approvals.CountAsync(a => a.Status == ApprovalStatus.Pending);
            var activeUsers = await _context.Users.CountAsync(u => u.Status == UserStatus.Active);
            var totalAssets = await _context.Assets.CountAsync();

            var openRequests = await _context.ServiceRequests.CountAsync(r => r.Status != null && r.Status.StatusName == "Open");
            var inProgressRequests = await _context.ServiceRequests.CountAsync(r => r.Status != null && r.Status.StatusName == "In Progress");
            var resolvedRequests = await _context.ServiceRequests.CountAsync(r => r.Status != null && r.Status.StatusName == "Resolved");
            var closedRequests = await _context.ServiceRequests.CountAsync(r => r.Status != null && r.Status.StatusName == "Closed");

            var today = DateTime.UtcNow.Date;
            var resolvedToday = await _context.ServiceRequests.CountAsync(r => r.Status != null && r.Status.StatusName == "Resolved" && r.UpdatedAt >= today);

            var priorityBreakdown = await _context.ServiceRequests
                .GroupBy(r => r.Priority)
                .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count());

            var statusBreakdown = await _context.ServiceRequests
                .Include(r => r.Status)
                .Where(r => r.Status != null)
                .GroupBy(r => r.Status!.StatusName)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

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
                MyAssignedRequests = 0,
                ResolvedToday = resolvedToday,
                AvgResolutionTimeHours = 4.2,
                SlaComplianceRate = 94.5,
                PriorityBreakdown = priorityBreakdown,
                StatusBreakdown = statusBreakdown
            };

            return Ok(new ApiResponseDto<DashboardSummaryDto> { Success = true, Data = summary });
        }
    }
}
