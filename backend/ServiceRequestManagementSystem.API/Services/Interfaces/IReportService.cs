using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.Dashboard;

namespace ServiceRequestManagementSystem.API.Services.Interfaces
{
    public interface IReportService
    {
        Task<ApiResponseDto<IEnumerable<DepartmentReportDto>>> GetDepartmentReportAsync();
        Task<ApiResponseDto<IEnumerable<SlaReportDto>>> GetSlaReportAsync();
        Task<ApiResponseDto<IEnumerable<TrendsReportDto>>> GetTrendsAsync();
    }
}
