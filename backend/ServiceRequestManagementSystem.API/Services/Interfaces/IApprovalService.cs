using ServiceRequestManagementSystem.API.DTOs.Approvals;
using ServiceRequestManagementSystem.API.DTOs.Common;

namespace ServiceRequestManagementSystem.API.Services.Interfaces
{
    public interface IApprovalService
    {
        Task<ApiResponseDto<IEnumerable<ApprovalResponseDto>>> GetApprovalsAsync();
        Task<ApiResponseDto<ApprovalResponseDto>> GetApprovalByIdAsync(int id);
        Task<ApiResponseDto<IEnumerable<ApprovalResponseDto>>> GetPendingApprovalsAsync();
        Task<ApiResponseDto<bool>> MakeDecisionAsync(int id, ApprovalDecisionDto dto, string? ipAddress);
    }
}
