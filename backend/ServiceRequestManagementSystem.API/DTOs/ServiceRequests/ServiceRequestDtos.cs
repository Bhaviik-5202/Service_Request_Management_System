using System.ComponentModel.DataAnnotations;
using ServiceRequestManagementSystem.API.Enums;

namespace ServiceRequestManagementSystem.API.DTOs.ServiceRequests
{
    public class ServiceRequestResponseDto
    {
        public int RequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public string RequestType { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Requester { get; set; } = string.Empty;
        public string RequesterEmail { get; set; } = string.Empty;
        public string? Assignee { get; set; }
        public string Status { get; set; } = string.Empty;
        public Priority Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ServiceRequestDetailResponseDto : ServiceRequestResponseDto
    {
        public List<ServiceRequestReplyResponseDto> Replies { get; set; } = new();
        public List<ServiceRequestTimelineResponseDto> Timeline { get; set; } = new();
        public List<ServiceRequestAttachmentResponseDto> Attachments { get; set; } = new();
    }

    public class CreateServiceRequestDto
    {
        [Required]
        [StringLength(150, MinimumLength = 5)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MinLength(20, ErrorMessage = "Description must be at least 20 characters long.")]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int ServiceTypeId { get; set; }

        [Required]
        public int RequestTypeId { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [Required]
        public Priority Priority { get; set; } = Priority.Medium;
    }

    public class UpdateServiceRequestStatusDto
    {
        [Required]
        public int StatusId { get; set; }

        public string? Note { get; set; }
    }

    public class AssignTechnicianDto
    {
        [Required]
        public int AssigneeUserId { get; set; }
    }

    public class CreateReplyDto
    {
        [Required]
        public string Message { get; set; } = string.Empty;

        public int? StatusTransitionId { get; set; }
    }

    public class ServiceRequestReplyResponseDto
    {
        public int ReplyId { get; set; }
        public string Author { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? StatusTransition { get; set; }
    }

    public class ServiceRequestTimelineResponseDto
    {
        public int TimelineId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ChangedBy { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
        public string Note { get; set; } = string.Empty;
    }

    public class ServiceRequestAttachmentResponseDto
    {
        public int AttachmentId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public int FileSizeKB { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }
}
