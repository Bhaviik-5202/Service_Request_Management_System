using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Request_Management_System.Data;
using Service_Request_Management_System.Models;

namespace Service_Request_Management_System.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [Authorize]
    public class ApprovalsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ApprovalsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Approvals/GetApprovals
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Approval>>> GetApprovals()
        {
            return await _context.Approvals
                .Include(a => a.ServiceRequest)
                .ThenInclude(sr => sr.RequesterUser)
                .Include(a => a.ServiceRequest)
                .ThenInclude(sr => sr.RequestType)
                .Include(a => a.DecidedByUser)
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync();
        }

        // GET: api/Approvals/GetApproval/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Approval>> GetApproval(int id)
        {
            var approval = await _context.Approvals
                .Include(a => a.ServiceRequest)
                .ThenInclude(sr => sr.RequesterUser)
                .Include(a => a.ServiceRequest)
                .ThenInclude(sr => sr.RequestType)
                .Include(a => a.DecidedByUser)
                .FirstOrDefaultAsync(a => a.ApprovalId == id);

            if (approval == null)
            {
                return NotFound($"Approval with ID {id} not found.");
            }

            return Ok(approval);
        }

        // GET: api/Approvals/GetApprovalByRequest/request/5
        [HttpGet("request/{requestId}")]
        public async Task<ActionResult<Approval>> GetApprovalByRequest(int requestId)
        {
            var approval = await _context.Approvals
                .Include(a => a.ServiceRequest)
                .Include(a => a.DecidedByUser)
                .FirstOrDefaultAsync(a => a.RequestId == requestId);

            if (approval == null)
            {
                return NotFound($"Approval for Request ID {requestId} not found.");
            }

            return Ok(approval);
        }

        // GET: api/Approvals/GetPendingApprovals/pending
        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<Approval>>> GetPendingApprovals()
        {
            var approvals = await _context.Approvals
                .Where(a => a.Status == "Pending")
                .Include(a => a.ServiceRequest)
                .ThenInclude(sr => sr.RequesterUser)
                .Include(a => a.ServiceRequest)
                .ThenInclude(sr => sr.RequestType)
                .Include(a => a.DecidedByUser)
                .OrderBy(a => a.SubmittedAt)
                .ToListAsync();

            return Ok(approvals);
        }

        // GET: api/Approvals/GetApprovalsByHOD/hod/5
        [HttpGet("hod/{hodUserId}")]
        public async Task<ActionResult<IEnumerable<Approval>>> GetApprovalsByHOD(int hodUserId)
        {
            var approvals = await _context.Approvals
                .Where(a => a.DecidedByUserId == hodUserId)
                .Include(a => a.ServiceRequest)
                .ThenInclude(sr => sr.RequesterUser)
                .Include(a => a.ServiceRequest)
                .ThenInclude(sr => sr.RequestType)
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync();

            return Ok(approvals);
        }

        // GET: api/Approvals/GetPendingApprovalsByHOD/pending/hod/5
        [HttpGet("pending/hod/{hodUserId}")]
        public async Task<ActionResult<IEnumerable<Approval>>> GetPendingApprovalsByHOD(int hodUserId)
        {
            var approvals = await _context.Approvals
                .Where(a => a.Status == "Pending" && a.DecidedByUserId == hodUserId)
                .Include(a => a.ServiceRequest)
                .ThenInclude(sr => sr.RequesterUser)
                .Include(a => a.ServiceRequest)
                .ThenInclude(sr => sr.RequestType)
                .OrderBy(a => a.SubmittedAt)
                .ToListAsync();

            return Ok(approvals);
        }

        // POST: api/Approvals/PostApproval
        [HttpPost]
        public async Task<ActionResult<Approval>> PostApproval(Approval approval)
        {
            var serviceRequest = await _context.ServiceRequests
                .FirstOrDefaultAsync(sr => sr.RequestId == approval.RequestId && !sr.IsDeleted);

            if (serviceRequest == null)
            {
                return BadRequest($"Service Request with ID {approval.RequestId} not found.");
            }

            if (await _context.Approvals.AnyAsync(a => a.RequestId == approval.RequestId))
            {
                return BadRequest($"Approval already exists for Request ID {approval.RequestId}.");
            }

            if (approval.DecidedByUserId.HasValue)
            {
                var hod = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == approval.DecidedByUserId && !u.IsDeleted);

                if (hod == null)
                {
                    return BadRequest($"HOD with ID {approval.DecidedByUserId} not found or is deleted.");
                }

                var isHOD = await _context.DepartmentPersonnel
                    .AnyAsync(dp => dp.UserId == approval.DecidedByUserId && dp.IsHOD && !dp.IsDeleted);

                if (!isHOD)
                {
                    return BadRequest($"User with ID {approval.DecidedByUserId} is not a Department Head (HOD).");
                }
            }

            var validStatuses = new[] { "Pending", "Approved", "Rejected" };
            if (!validStatuses.Contains(approval.Status))
            {
                return BadRequest($"Invalid status. Valid values: {string.Join(", ", validStatuses)}");
            }

            approval.SubmittedAt = DateTime.UtcNow;
            if (approval.Status != "Pending")
            {
                approval.DecidedAt = DateTime.UtcNow;
            }

            _context.Approvals.Add(approval);
            await _context.SaveChangesAsync();

            if (approval.Status == "Approved")
            {
                var assignedStatus = await _context.ServiceRequestStatuses
                    .FirstOrDefaultAsync(s => s.StatusName == "Assigned");

                if (assignedStatus != null)
                {
                    serviceRequest.StatusId = assignedStatus.StatusId;
                    serviceRequest.UpdatedAt = DateTime.UtcNow;
                    _context.Entry(serviceRequest).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
            }
            else if (approval.Status == "Rejected")
            {
                var rejectedStatus = await _context.ServiceRequestStatuses
                    .FirstOrDefaultAsync(s => s.StatusName == "Rejected");

                if (rejectedStatus != null)
                {
                    serviceRequest.StatusId = rejectedStatus.StatusId;
                    serviceRequest.UpdatedAt = DateTime.UtcNow;
                    _context.Entry(serviceRequest).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
            }

            return CreatedAtAction(nameof(GetApproval), new { id = approval.ApprovalId }, approval);
        }

        // PUT: api/Approvals/DecideApproval/5/decide
        [HttpPut("{id}/decide")]
        public async Task<IActionResult> DecideApproval(int id, [FromBody] ApprovalDecision decision)
        {
            var approval = await _context.Approvals
                .Include(a => a.ServiceRequest)
                .FirstOrDefaultAsync(a => a.ApprovalId == id);

            if (approval == null)
            {
                return NotFound($"Approval with ID {id} not found.");
            }

            if (approval.Status != "Pending")
            {
                return BadRequest($"Approval is already {approval.Status}.");
            }

            var validStatuses = new[] { "Approved", "Rejected" };
            if (!validStatuses.Contains(decision.Status))
            {
                return BadRequest($"Invalid status. Valid values: {string.Join(", ", validStatuses)}");
            }

            var hod = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == decision.DecidedByUserId && !u.IsDeleted);

            if (hod == null)
            {
                return BadRequest($"HOD with ID {decision.DecidedByUserId} not found or is deleted.");
            }

            var isHOD = await _context.DepartmentPersonnel
                .AnyAsync(dp => dp.UserId == decision.DecidedByUserId && dp.IsHOD && !dp.IsDeleted);

            if (!isHOD)
            {
                return BadRequest($"User with ID {decision.DecidedByUserId} is not a Department Head (HOD).");
            }

            approval.Status = decision.Status;
            approval.DecidedByUserId = decision.DecidedByUserId;
            approval.DecidedAt = DateTime.UtcNow;
            approval.Remarks = decision.Remarks;

            _context.Entry(approval).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var serviceRequest = approval.ServiceRequest;
            if (decision.Status == "Approved")
            {
                var assignedStatus = await _context.ServiceRequestStatuses
                    .FirstOrDefaultAsync(s => s.StatusName == "Assigned");

                if (assignedStatus != null)
                {
                    serviceRequest.StatusId = assignedStatus.StatusId;
                    serviceRequest.UpdatedAt = DateTime.UtcNow;
                    _context.Entry(serviceRequest).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
            }
            else if (decision.Status == "Rejected")
            {
                var rejectedStatus = await _context.ServiceRequestStatuses
                    .FirstOrDefaultAsync(s => s.StatusName == "Rejected");

                if (rejectedStatus != null)
                {
                    serviceRequest.StatusId = rejectedStatus.StatusId;
                    serviceRequest.UpdatedAt = DateTime.UtcNow;
                    _context.Entry(serviceRequest).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
            }

            return NoContent();
        }

        // DELETE: api/Approvals/DeleteApproval/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApproval(int id)
        {
            var approval = await _context.Approvals
                .FirstOrDefaultAsync(a => a.ApprovalId == id);

            if (approval == null)
            {
                return NotFound($"Approval with ID {id} not found.");
            }

            _context.Approvals.Remove(approval);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class ApprovalDecision
    {
        public string Status { get; set; } = string.Empty;
        public int DecidedByUserId { get; set; }
        public string? Remarks { get; set; }
    }
}