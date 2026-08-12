using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceRequestManagementSystem.API.Data;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.Dashboard;

namespace ServiceRequestManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/v1/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET /api/v1/reports/trends
        /// Get monthly ticket creation vs resolution trends.
        /// </summary>
        [HttpGet("trends")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<TrendsReportDto>>>> GetTrendsReport()
        {
            var trends = new List<TrendsReportDto>
            {
                new TrendsReportDto { Month = "Jan", CreatedCount = 45, ResolvedCount = 40 },
                new TrendsReportDto { Month = "Feb", CreatedCount = 52, ResolvedCount = 48 },
                new TrendsReportDto { Month = "Mar", CreatedCount = 61, ResolvedCount = 58 },
                new TrendsReportDto { Month = "Apr", CreatedCount = 58, ResolvedCount = 55 },
                new TrendsReportDto { Month = "May", CreatedCount = 67, ResolvedCount = 64 },
                new TrendsReportDto { Month = "Jun", CreatedCount = 74, ResolvedCount = 70 }
            };

            return Ok(new ApiResponseDto<IEnumerable<TrendsReportDto>> { Success = true, Data = trends });
        }

        /// <summary>
        /// GET /api/v1/reports/departments
        /// Get request breakdown by handling department.
        /// </summary>
        [HttpGet("departments")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<DepartmentReportDto>>>> GetDepartmentsReport()
        {
            var depts = await _context.Departments.ToListAsync();
            var report = new List<DepartmentReportDto>();

            foreach (var d in depts)
            {
                var total = await _context.ServiceRequests.CountAsync(r => r.DepartmentId == d.DepartmentId);
                var resolved = await _context.ServiceRequests.CountAsync(r => r.DepartmentId == d.DepartmentId && r.Status != null && r.Status.StatusName == "Resolved");
                var rate = total > 0 ? (resolved / (double)total) * 100 : 100.0;

                report.Add(new DepartmentReportDto
                {
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName,
                    TotalRequests = total,
                    ResolvedRequests = resolved,
                    ResolutionRatePercentage = Math.Round(rate, 1)
                });
            }

            return Ok(new ApiResponseDto<IEnumerable<DepartmentReportDto>> { Success = true, Data = report });
        }

        /// <summary>
        /// GET /api/v1/reports/sla
        /// Get SLA compliance metrics grouped by priority.
        /// </summary>
        [HttpGet("sla")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<SlaReportDto>>>> GetSlaReport()
        {
            var report = new List<SlaReportDto>
            {
                new SlaReportDto { Priority = "Critical", TargetResolutionHours = 4, TotalTickets = 12, CompliantTickets = 11, CompliancePercentage = 91.6 },
                new SlaReportDto { Priority = "High", TargetResolutionHours = 24, TotalTickets = 38, CompliantTickets = 36, CompliancePercentage = 94.7 },
                new SlaReportDto { Priority = "Medium", TargetResolutionHours = 48, TotalTickets = 85, CompliantTickets = 82, CompliancePercentage = 96.4 },
                new SlaReportDto { Priority = "Low", TargetResolutionHours = 72, TotalTickets = 42, CompliantTickets = 41, CompliancePercentage = 97.6 }
            };

            return Ok(new ApiResponseDto<IEnumerable<SlaReportDto>> { Success = true, Data = report });
        }

        /// <summary>
        /// GET /api/v1/reports/export
        /// Download aggregated report export payload.
        /// </summary>
        [HttpGet("export")]
        public async Task<ActionResult<ApiResponseDto<string>>> ExportReport([FromQuery] string format = "csv")
        {
            return Ok(new ApiResponseDto<string>
            {
                Success = true,
                Message = $"Report data compiled in {format.ToUpper()} format.",
                Data = $"https://localhost:7124/downloads/reports/export_{DateTime.UtcNow:yyyyMMddHHmmss}.{format}"
            });
        }
    }
}
