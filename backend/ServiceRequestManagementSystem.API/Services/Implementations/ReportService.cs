using Microsoft.EntityFrameworkCore;
using ServiceRequestManagementSystem.API.Data;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.Dashboard;
using ServiceRequestManagementSystem.API.Enums;
using ServiceRequestManagementSystem.API.Services.Interfaces;

namespace ServiceRequestManagementSystem.API.Services.Implementations
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;

        public ReportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponseDto<IEnumerable<DepartmentReportDto>>> GetDepartmentReportAsync()
        {
            var departments = await _context.Departments
                .AsNoTracking()
                .Select(d => new DepartmentReportDto
                {
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName,
                    TotalRequests = d.ServiceRequests.Count(),
                    ResolvedRequests = d.ServiceRequests.Count(r =>
                        r.Status != null && r.Status.StatusName == RequestStatusNames.Resolved)
                })
                .ToListAsync();

            foreach (var dept in departments)
            {
                dept.ResolutionRatePercentage = dept.TotalRequests == 0
                    ? 0
                    : Math.Round(dept.ResolvedRequests * 100.0 / dept.TotalRequests, 2);
            }

            return Ok("Department report fetched successfully.", (IEnumerable<DepartmentReportDto>)departments);
        }

        public async Task<ApiResponseDto<IEnumerable<SlaReportDto>>> GetSlaReportAsync()
        {
            var requests = await _context.ServiceRequests
                .AsNoTracking()
                .Select(r => new
                {
                    r.Priority, r.CreatedAt, r.ResolvedAt,
                    StatusName = r.Status != null ? r.Status.StatusName : null
                })
                .ToListAsync();

            var report = requests
                .GroupBy(r => r.Priority)
                .Select(group =>
                {
                    var targetHours = GetSlaHours(group.Key);
                    var resolved = group.Where(r =>
                        r.StatusName == RequestStatusNames.Resolved && r.ResolvedAt.HasValue).ToList();
                    var compliant = resolved.Count(r =>
                        (r.ResolvedAt!.Value - r.CreatedAt).TotalHours <= targetHours);
                    var compliance = resolved.Count == 0 ? 0 : compliant * 100.0 / resolved.Count;
                    return new SlaReportDto
                    {
                        Priority = group.Key.ToString(), TargetResolutionHours = targetHours,
                        TotalTickets = resolved.Count, CompliantTickets = compliant,
                        CompliancePercentage = Math.Round(compliance, 2)
                    };
                })
                .OrderByDescending(r => r.CompliancePercentage)
                .ToList();

            return Ok("SLA report fetched successfully.", (IEnumerable<SlaReportDto>)report);
        }

        public async Task<ApiResponseDto<IEnumerable<TrendsReportDto>>> GetTrendsAsync()
        {
            var requests = await _context.ServiceRequests
                .AsNoTracking()
                .Select(r => new { r.CreatedAt, r.ResolvedAt, StatusName = r.Status != null ? r.Status.StatusName : null })
                .ToListAsync();

            var createdByMonth = requests
                .GroupBy(r => new { r.CreatedAt.Year, r.CreatedAt.Month })
                .ToDictionary(g => (g.Key.Year, g.Key.Month), g => g.Count());

            var resolvedByMonth = requests
                .Where(r => r.StatusName == RequestStatusNames.Resolved && r.ResolvedAt.HasValue)
                .GroupBy(r => new { r.ResolvedAt!.Value.Year, r.ResolvedAt.Value.Month })
                .ToDictionary(g => (g.Key.Year, g.Key.Month), g => g.Count());

            var months = createdByMonth.Keys.Union(resolvedByMonth.Keys)
                .OrderBy(x => x.Year).ThenBy(x => x.Month);

            var trends = months.Select(month => new TrendsReportDto
            {
                Month = $"{month.Year}-{month.Month:00}",
                CreatedCount = createdByMonth.TryGetValue(month, out var c) ? c : 0,
                ResolvedCount = resolvedByMonth.TryGetValue(month, out var r) ? r : 0
            }).ToList();

            return Ok("Request trends fetched successfully.", (IEnumerable<TrendsReportDto>)trends);
        }

        private static int GetSlaHours(Priority priority) => priority switch
        {
            Priority.Critical => 4, Priority.High => 8, Priority.Medium => 24, Priority.Low => 48, _ => 24
        };

        private static ApiResponseDto<T> Ok<T>(string message, T data) =>
            new() { Success = true, Message = message, Data = data };
    }
}
