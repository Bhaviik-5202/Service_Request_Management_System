using Microsoft.AspNetCore.Http;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.ServiceRequests;

namespace ServiceRequestManagementSystem.API.Services.Interfaces
{
    public interface IServiceRequestService
    {
        Task<ApiResponseDto<IEnumerable<ServiceRequestResponseDto>>> GetRequestsAsync();
        Task<ApiResponseDto<ServiceRequestDetailResponseDto>> GetRequestByIdAsync(int id);
        Task<ApiResponseDto<ServiceRequestResponseDto>> CreateRequestAsync(
            CreateServiceRequestDto dto, string? ipAddress);
        Task<ApiResponseDto<object>> UpdateStatusAsync(
            int id, UpdateServiceRequestStatusDto dto, int actorUserId, string? ipAddress);
        Task<ApiResponseDto<object>> AssignTechnicianAsync(
            int id, AssignTechnicianDto dto, int actorUserId, string? ipAddress);
        Task<ApiResponseDto<object>> CancelRequestAsync(int id, int actorUserId, string? ipAddress);
        Task<ApiResponseDto<object>> ReopenRequestAsync(int id, int actorUserId, string? ipAddress);
        Task<ApiResponseDto<IEnumerable<ServiceRequestReplyResponseDto>>> GetRepliesAsync(int id);
        Task<ApiResponseDto<ServiceRequestReplyResponseDto>> AddReplyAsync(
            int id, CreateReplyDto dto, string? ipAddress);
        Task<ApiResponseDto<IEnumerable<ServiceRequestTimelineResponseDto>>> GetTimelineAsync(int id);
        Task<ApiResponseDto<ServiceRequestAttachmentResponseDto>> UploadAttachmentAsync(
            IFormFile file, int requestId, int? replyId, int uploadedByUserId, string? ipAddress);
        Task<ApiResponseDto<object>> DeleteRequestAsync(int id, string? ipAddress);
    }
}
