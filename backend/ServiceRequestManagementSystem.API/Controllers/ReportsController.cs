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
    [Authorize(Roles = "Admin,HOD")]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("departments")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<DepartmentReportDto>>>> GetDepartmentReport()
        {
            var departments = await _context.Departments
                .AsNoTracking()
                .Select(d => new DepartmentReportDto
                {
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName,
                    TotalRequests = d.ServiceRequests.Count(),
                    ResolvedRequests = d.ServiceRequests.Count(r =>
                        r.Status != null &&
                        r.Status.StatusName == RequestStatusNames.Resolved)
                })
                .ToListAsync();

            foreach (var department in departments)
            {
                department.ResolutionRatePercentage =
                    department.TotalRequests == 0
                        ? 0
                        : Math.Round(
                            department.ResolvedRequests * 100.0 /
                            department.TotalRequests,
                            2);
            }

            return Ok(new ApiResponseDto<IEnumerable<DepartmentReportDto>>
            {
                Success = true,
                Message = "Department report fetched successfully.",
                Data = departments
            });
        }

        [HttpGet("sla")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<SlaReportDto>>>> GetSlaReport()
        {
            var requests = await _context.ServiceRequests
                .AsNoTracking()
                .Select(r => new
                {
                    r.Priority,
                    r.CreatedAt,
                    r.ResolvedAt,
                    StatusName = r.Status != null
                        ? r.Status.StatusName
                        : null
                })
                .ToListAsync();

            var report = requests
                .GroupBy(r => r.Priority)
                .Select(group =>
                {
                    var targetHours = GetSlaHours(group.Key);

                    var resolvedRequests = group
                        .Where(r =>
                            r.StatusName == RequestStatusNames.Resolved &&
                            r.ResolvedAt.HasValue)
                        .ToList();

                    var compliantTickets = resolvedRequests.Count(r =>
                        (r.ResolvedAt!.Value - r.CreatedAt).TotalHours <= targetHours);

                    var compliancePercentage = resolvedRequests.Count == 0
                        ? 0
                        : compliantTickets * 100.0 / resolvedRequests.Count;

                    return new SlaReportDto
                    {
                        Priority = group.Key.ToString(),
                        TargetResolutionHours = targetHours,
                        TotalTickets = resolvedRequests.Count,
                        CompliantTickets = compliantTickets,
                        CompliancePercentage = Math.Round(
                            compliancePercentage,
                            2)
                    };
                })
                .OrderByDescending(r => r.CompliancePercentage)
                .ToList();

            return Ok(new ApiResponseDto<IEnumerable<SlaReportDto>>
            {
                Success = true,
                Message = "SLA report fetched successfully.",
                Data = report
            });
        }

        [HttpGet("trends")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<TrendsReportDto>>>> GetTrends()
        {
            var requests = await _context.ServiceRequests
                .AsNoTracking()
                .Select(r => new
                {
                    r.CreatedAt,
                    r.ResolvedAt,
                    StatusName = r.Status != null
                        ? r.Status.StatusName
                        : null
                })
                .ToListAsync();

            var createdByMonth = requests
                .GroupBy(r => new
                {
                    r.CreatedAt.Year,
                    r.CreatedAt.Month
                })
                .ToDictionary(
                    g => (g.Key.Year, g.Key.Month),
                    g => g.Count());

            var resolvedByMonth = requests
                .Where(r =>
                    r.StatusName == RequestStatusNames.Resolved &&
                    r.ResolvedAt.HasValue)
                .GroupBy(r => new
                {
                    r.ResolvedAt!.Value.Year,
                    r.ResolvedAt.Value.Month
                })
                .ToDictionary(
                    g => (g.Key.Year, g.Key.Month),
                    g => g.Count());

            var months = createdByMonth.Keys
                .Union(resolvedByMonth.Keys)
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month);

            var trends = months
                .Select(month => new TrendsReportDto
                {
                    Month = $"{month.Year}-{month.Month:00}",
                    CreatedCount = createdByMonth.TryGetValue(
                        month,
                        out var created)
                        ? created
                        : 0,
                    ResolvedCount = resolvedByMonth.TryGetValue(
                        month,
                        out var resolved)
                        ? resolved
                        : 0
                })
                .ToList();

            return Ok(new ApiResponseDto<IEnumerable<TrendsReportDto>>
            {
                Success = true,
                Message = "Request trends fetched successfully.",
                Data = trends
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
