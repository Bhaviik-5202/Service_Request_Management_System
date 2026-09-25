using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.Dashboard;

namespace ServiceRequestManagementSystem.API.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<ApiResponseDto<DashboardSummaryDto>> GetSummaryAsync(int? userId);
    }
}
