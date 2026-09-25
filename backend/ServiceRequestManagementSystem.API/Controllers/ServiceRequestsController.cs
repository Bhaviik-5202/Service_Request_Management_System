using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.ServiceRequests;
using ServiceRequestManagementSystem.API.Services.Interfaces;

namespace ServiceRequestManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly IServiceRequestService _requestService;

        public ServiceRequestsController(IServiceRequestService requestService)
        {
            _requestService = requestService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ServiceRequestResponseDto>>>> GetRequests()
        {
            var result = await _requestService.GetRequestsAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<ServiceRequestDetailResponseDto>>> GetRequestById(int id)
        {
            var result = await _requestService.GetRequestByIdAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<ServiceRequestResponseDto>>> CreateRequest([FromBody] CreateServiceRequestDto dto)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _requestService.CreateRequestAsync(dto, ipAddress);
            if (!result.Success)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetRequestById), new { id = result.Data?.RequestId }, result);
        }

        [HttpPut("{id}/status")]
        public async Task<ActionResult<ApiResponseDto<object>>> UpdateStatus(int id, [FromBody] UpdateServiceRequestStatusDto dto)
        {
            var actorUserId = GetCurrentUserId();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _requestService.UpdateStatusAsync(id, dto, actorUserId, ipAddress);
            if (!result.Success)
            {
                if (result.Message == "Service request not found.")
                    return NotFound(result);

                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPut("{id}/assign")]
        public async Task<ActionResult<ApiResponseDto<object>>> AssignTechnician(int id, [FromBody] AssignTechnicianDto dto)
        {
            var actorUserId = GetCurrentUserId();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _requestService.AssignTechnicianAsync(id, dto, actorUserId, ipAddress);
            if (!result.Success)
            {
                if (result.Message == "Service request not found.")
                    return NotFound(result);

                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPut("{id}/cancel")]
        public async Task<ActionResult<ApiResponseDto<object>>> CancelRequest(int id)
        {
            var actorUserId = GetCurrentUserId();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _requestService.CancelRequestAsync(id, actorUserId, ipAddress);
            if (!result.Success)
            {
                if (result.Message == "Service request not found.")
                    return NotFound(result);

                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPut("{id}/reopen")]
        public async Task<ActionResult<ApiResponseDto<object>>> ReopenRequest(int id)
        {
            var actorUserId = GetCurrentUserId();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _requestService.ReopenRequestAsync(id, actorUserId, ipAddress);
            if (!result.Success)
            {
                if (result.Message == "Service request not found.")
                    return NotFound(result);

                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("{id}/replies")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ServiceRequestReplyResponseDto>>>> GetReplies(int id)
        {
            var result = await _requestService.GetRepliesAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost("{id}/replies")]
        public async Task<ActionResult<ApiResponseDto<ServiceRequestReplyResponseDto>>> PostReply(int id, [FromBody] CreateReplyDto dto)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _requestService.AddReplyAsync(id, dto, ipAddress);
            if (!result.Success)
            {
                if (result.Message == "Service request not found.")
                    return NotFound(result);

                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("{id}/timeline")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ServiceRequestTimelineResponseDto>>>> GetTimeline(int id)
        {
            var result = await _requestService.GetTimelineAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost("attachments/upload")]
        public async Task<ActionResult<ApiResponseDto<ServiceRequestAttachmentResponseDto>>> UploadAttachment(
            [FromForm] IFormFile file,
            [FromForm] int requestId,
            [FromForm] int? replyId,
            [FromForm] int uploadedByUserId)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _requestService.UploadAttachmentAsync(file, requestId, replyId, uploadedByUserId, ipAddress);
            if (!result.Success)
            {
                if (result.Message == "Service request not found.")
                    return NotFound(result);

                return BadRequest(result);
            }

            return Ok(result);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("userId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdClaim, out var userId);
            return userId;
        }
    }
}
