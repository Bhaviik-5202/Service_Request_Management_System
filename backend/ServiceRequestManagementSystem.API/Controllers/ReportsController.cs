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
                .Where(d => !d.IsDeleted)
                .Select(d => new DepartmentReportDto
                {
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName,
                    TotalRequests = d.ServiceRequests.Count(r => !r.IsDeleted),
                    ResolvedRequests = d.ServiceRequests.Count(r => !r.IsDeleted && r.Status != null && r.Status.StatusName == "Resolved")
                })
                .ToListAsync();

            foreach (var department in departments)
            {
                department.ResolutionRatePercentage = department.TotalRequests == 0
                    ? 0
                    : department.ResolvedRequests * 100.0 / department.TotalRequests;
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
                .Where(r => !r.IsDeleted)
                .Select(r => new
                {
                    r.Priority,
                    r.CreatedAt,
                    r.UpdatedAt,
                    StatusName = r.Status != null ? r.Status.StatusName : null
                })
                .ToListAsync();

            var report = requests
                .GroupBy(r => r.Priority)
                .Select(group =>
                {
                    var targetHours = group.Key switch
                    {
                        Priority.Critical => 4,
                        Priority.High => 8,
                        Priority.Medium => 24,
                        Priority.Low => 48,
                        _ => 24
                    };

                    var resolvedRequests = group.Where(r => r.StatusName == "Resolved").ToList();
                    var compliantTickets = resolvedRequests.Count(r => (r.UpdatedAt - r.CreatedAt).TotalHours <= targetHours);
                    var compliancePercentage = resolvedRequests.Count == 0
                        ? 0
                        : compliantTickets * 100.0 / resolvedRequests.Count;

                    return new SlaReportDto
                    {
                        Priority = group.Key.ToString(),
                        TargetResolutionHours = targetHours,
                        TotalTickets = resolvedRequests.Count,
                        CompliantTickets = compliantTickets,
                        CompliancePercentage = compliancePercentage
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
                .Where(r => !r.IsDeleted)
                .Select(r => new
                {
                    r.CreatedAt,
                    r.UpdatedAt,
                    StatusName = r.Status != null ? r.Status.StatusName : null
                })
                .ToListAsync();

            var trends = requests
                .GroupBy(r => new
                {
                    r.CreatedAt.Year,
                    r.CreatedAt.Month
                })
                .OrderBy(group => group.Key.Year)
                .ThenBy(group => group.Key.Month)
                .Select(group => new TrendsReportDto
                {
                    Month = $"{group.Key.Year}-{group.Key.Month:00}",
                    CreatedCount = group.Count(),
                    ResolvedCount = group.Count(r => r.StatusName == "Resolved")
                })
                .ToList();

            return Ok(new ApiResponseDto<IEnumerable<TrendsReportDto>>
            {
                Success = true,
                Message = "Request trends fetched successfully.",
                Data = trends
            });
        }
    }
}
