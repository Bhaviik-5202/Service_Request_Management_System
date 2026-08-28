using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceRequestManagementSystem.API.Data;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.Masters;
using ServiceRequestManagementSystem.API.Models;

namespace ServiceRequestManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class MastersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MastersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("statuses")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<StatusDto>>>> GetStatuses()
        {
            var statuses = await _context.ServiceRequestStatuses
                .OrderBy(s => s.StatusId)
                .Select(s => new StatusDto
                {
                    StatusId = s.StatusId,
                    StatusName = s.StatusName,
                    ColorCode = s.ColorCode,
                    Description = s.Description,
                    IsActive = s.IsActive
                })
                .ToListAsync();

            return Ok(new ApiResponseDto<IEnumerable<StatusDto>>
            {
                Success = true,
                Data = statuses
            });
        }

        [HttpPost("statuses")]
        public async Task<ActionResult<ApiResponseDto<StatusDto>>> CreateStatus(
            [FromBody] StatusDto dto)
        {
            var status = new ServiceRequestStatus
            {
                StatusName = dto.StatusName,
                ColorCode = dto.ColorCode,
                Description = dto.Description,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.ServiceRequestStatuses.Add(status);
            await _context.SaveChangesAsync();

            dto.StatusId = status.StatusId;

            return Ok(new ApiResponseDto<StatusDto>
            {
                Success = true,
                Message = "Status created successfully.",
                Data = dto
            });
        }

        [HttpPut("statuses/{id}")]
        public async Task<ActionResult<ApiResponseDto<StatusDto>>> UpdateStatus(
            int id,
            [FromBody] StatusDto dto)
        {
            var status = await _context.ServiceRequestStatuses.FindAsync(id);

            if (status == null)
            {
                return NotFound(new ApiResponseDto<StatusDto>
                {
                    Success = false,
                    Message = "Status not found."
                });
            }

            status.StatusName = dto.StatusName;
            status.ColorCode = dto.ColorCode;
            status.Description = dto.Description;
            status.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            dto.StatusId = status.StatusId;

            return Ok(new ApiResponseDto<StatusDto>
            {
                Success = true,
                Message = "Status updated successfully.",
                Data = dto
            });
        }

        [HttpDelete("statuses/{id}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteStatus(int id)
        {
            var status = await _context.ServiceRequestStatuses.FindAsync(id);

            if (status == null)
            {
                return NotFound(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Status not found."
                });
            }

            status.IsActive = false;

            await _context.SaveChangesAsync();

            return Ok(new ApiResponseDto<bool>
            {
                Success = true,
                Message = "Status deactivated.",
                Data = true
            });
        }

        [HttpGet("departments")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<DepartmentDto>>>> GetDepartments()
        {
            var departments = await _context.Departments
                .OrderBy(d => d.DepartmentId)
                .Select(d => new DepartmentDto
                {
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName,
                    DepartmentCode = d.DepartmentCode,
                    Description = d.Description,
                    IsActive = d.IsActive
                })
                .ToListAsync();

            return Ok(new ApiResponseDto<IEnumerable<DepartmentDto>>
            {
                Success = true,
                Data = departments
            });
        }

        [HttpPost("departments")]
        public async Task<ActionResult<ApiResponseDto<DepartmentDto>>> CreateDepartment(
            [FromBody] DepartmentDto dto)
        {
            var department = new Department
            {
                DepartmentName = dto.DepartmentName,
                DepartmentCode = dto.DepartmentCode,
                Description = dto.Description,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            dto.DepartmentId = department.DepartmentId;

            return Ok(new ApiResponseDto<DepartmentDto>
            {
                Success = true,
                Message = "Department created successfully.",
                Data = dto
            });
        }

        [HttpPut("departments/{id}")]
        public async Task<ActionResult<ApiResponseDto<DepartmentDto>>> UpdateDepartment(
            int id,
            [FromBody] DepartmentDto dto)
        {
            var department = await _context.Departments.FindAsync(id);

            if (department == null)
            {
                return NotFound(new ApiResponseDto<DepartmentDto>
                {
                    Success = false,
                    Message = "Department not found."
                });
            }

            department.DepartmentName = dto.DepartmentName;
            department.DepartmentCode = dto.DepartmentCode;
            department.Description = dto.Description;
            department.IsActive = dto.IsActive;
            department.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            dto.DepartmentId = department.DepartmentId;

            return Ok(new ApiResponseDto<DepartmentDto>
            {
                Success = true,
                Message = "Department updated successfully.",
                Data = dto
            });
        }

        [HttpDelete("departments/{id}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteDepartment(int id)
        {
            var department = await _context.Departments.FindAsync(id);

            if (department == null)
            {
                return NotFound(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Department not found."
                });
            }

            department.IsDeleted = true;
            department.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new ApiResponseDto<bool>
            {
                Success = true,
                Message = "Department soft deleted.",
                Data = true
            });
        }

        [HttpGet("personnel")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<DepartmentPersonnelDto>>>> GetPersonnel()
        {
            var personnel = await _context.DepartmentPersonnel
                .Include(p => p.User)
                .Include(p => p.Department)
                .Select(p => new DepartmentPersonnelDto
                {
                    DepartmentPersonnelId = p.DepartmentPersonnelId,
                    UserId = p.UserId,
                    UserName = p.User!.FullName,
                    DepartmentId = p.DepartmentId,
                    DepartmentName = p.Department!.DepartmentName,
                    IsHOD = p.IsHOD,
                    IsActive = p.IsActive
                })
                .ToListAsync();

            return Ok(new ApiResponseDto<IEnumerable<DepartmentPersonnelDto>>
            {
                Success = true,
                Data = personnel
            });
        }

        [HttpPost("personnel")]
        public async Task<ActionResult<ApiResponseDto<DepartmentPersonnelDto>>> MapPersonnel(
            [FromBody] DepartmentPersonnelDto dto)
        {
            var personnel = new DepartmentPersonnel
            {
                UserId = dto.UserId,
                DepartmentId = dto.DepartmentId,
                IsHOD = dto.IsHOD,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.DepartmentPersonnel.Add(personnel);
            await _context.SaveChangesAsync();

            dto.DepartmentPersonnelId = personnel.DepartmentPersonnelId;

            return Ok(new ApiResponseDto<DepartmentPersonnelDto>
            {
                Success = true,
                Message = "Personnel mapped successfully.",
                Data = dto
            });
        }

        [HttpPut("personnel/{id}")]
        public async Task<ActionResult<ApiResponseDto<DepartmentPersonnelDto>>> UpdatePersonnel(
            int id,
            [FromBody] DepartmentPersonnelDto dto)
        {
            var personnel = await _context.DepartmentPersonnel.FindAsync(id);

            if (personnel == null)
            {
                return NotFound(new ApiResponseDto<DepartmentPersonnelDto>
                {
                    Success = false,
                    Message = "Personnel mapping not found."
                });
            }

            personnel.UserId = dto.UserId;
            personnel.DepartmentId = dto.DepartmentId;
            personnel.IsHOD = dto.IsHOD;
            personnel.IsActive = dto.IsActive;
            personnel.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            dto.DepartmentPersonnelId = personnel.DepartmentPersonnelId;

            return Ok(new ApiResponseDto<DepartmentPersonnelDto>
            {
                Success = true,
                Message = "Personnel mapping updated.",
                Data = dto
            });
        }

        [HttpDelete("personnel/{id}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeletePersonnel(int id)
        {
            var personnel = await _context.DepartmentPersonnel.FindAsync(id);

            if (personnel == null)
            {
                return NotFound(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Personnel mapping not found."
                });
            }

            personnel.IsDeleted = true;
            personnel.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new ApiResponseDto<bool>
            {
                Success = true,
                Message = "Personnel mapping soft deleted.",
                Data = true
            });
        }

        [HttpGet("service-types")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ServiceTypeDto>>>> GetServiceTypes()
        {
            var serviceTypes = await _context.ServiceTypes
                .OrderBy(s => s.ServiceTypeId)
                .Select(s => new ServiceTypeDto
                {
                    ServiceTypeId = s.ServiceTypeId,
                    ServiceTypeName = s.ServiceTypeName,
                    ServiceTypeCode = s.ServiceTypeCode,
                    Description = s.Description,
                    IsActive = s.IsActive
                })
                .ToListAsync();

            return Ok(new ApiResponseDto<IEnumerable<ServiceTypeDto>>
            {
                Success = true,
                Data = serviceTypes
            });
        }

        [HttpPost("service-types")]
        public async Task<ActionResult<ApiResponseDto<ServiceTypeDto>>> CreateServiceType(
            [FromBody] ServiceTypeDto dto)
        {
            var serviceType = new ServiceType
            {
                ServiceTypeName = dto.ServiceTypeName,
                ServiceTypeCode = dto.ServiceTypeCode,
                Description = dto.Description,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ServiceTypes.Add(serviceType);
            await _context.SaveChangesAsync();

            dto.ServiceTypeId = serviceType.ServiceTypeId;

            return Ok(new ApiResponseDto<ServiceTypeDto>
            {
                Success = true,
                Message = "Service type created successfully.",
                Data = dto
            });
        }

        [HttpGet("request-types")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<RequestTypeDto>>>> GetRequestTypes()
        {
            var requestTypes = await _context.RequestTypes
                .Include(r => r.ServiceType)
                .OrderBy(r => r.RequestTypeId)
                .Select(r => new RequestTypeDto
                {
                    RequestTypeId = r.RequestTypeId,
                    ServiceTypeId = r.ServiceTypeId,
                    ServiceTypeName = r.ServiceType != null
                        ? r.ServiceType.ServiceTypeName
                        : null,
                    RequestTypeName = r.RequestTypeName,
                    Description = r.Description,
                    RequiresApproval = r.RequiresApproval,
                    IsActive = r.IsActive
                })
                .ToListAsync();

            return Ok(new ApiResponseDto<IEnumerable<RequestTypeDto>>
            {
                Success = true,
                Data = requestTypes
            });
        }

        [HttpPost("request-types")]
        public async Task<ActionResult<ApiResponseDto<RequestTypeDto>>> CreateRequestType(
            [FromBody] RequestTypeDto dto)
        {
            var requestType = new RequestType
            {
                ServiceTypeId = dto.ServiceTypeId,
                RequestTypeName = dto.RequestTypeName,
                Description = dto.Description,
                RequiresApproval = dto.RequiresApproval,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.RequestTypes.Add(requestType);
            await _context.SaveChangesAsync();

            dto.RequestTypeId = requestType.RequestTypeId;

            return Ok(new ApiResponseDto<RequestTypeDto>
            {
                Success = true,
                Message = "Request type created successfully.",
                Data = dto
            });
        }

        [HttpGet("mappings")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<RequestTypeTechnicianMappingDto>>>> GetMappings()
        {
            var mappings = await _context.RequestTypeTechnicianMappings
                .Include(m => m.RequestType)
                .Include(m => m.DepartmentPersonnel)
                .ThenInclude(dp => dp!.User)
                .Select(m => new RequestTypeTechnicianMappingDto
                {
                    MappingId = m.MappingId,
                    RequestTypeId = m.RequestTypeId,
                    RequestTypeName = m.RequestType!.RequestTypeName,
                    DepartmentPersonnelId = m.DepartmentPersonnelId,
                    TechnicianName = m.DepartmentPersonnel!.User!.FullName,
                    IsActive = m.IsActive
                })
                .ToListAsync();

            return Ok(new ApiResponseDto<IEnumerable<RequestTypeTechnicianMappingDto>>
            {
                Success = true,
                Data = mappings
            });
        }

        [HttpPost("mappings")]
        public async Task<ActionResult<ApiResponseDto<RequestTypeTechnicianMappingDto>>> CreateMapping(
            [FromBody] RequestTypeTechnicianMappingDto dto)
        {
            var mapping = new RequestTypeTechnicianMapping
            {
                RequestTypeId = dto.RequestTypeId,
                DepartmentPersonnelId = dto.DepartmentPersonnelId,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.RequestTypeTechnicianMappings.Add(mapping);
            await _context.SaveChangesAsync();

            dto.MappingId = mapping.MappingId;

            return Ok(new ApiResponseDto<RequestTypeTechnicianMappingDto>
            {
                Success = true,
                Message = "Auto-assignment mapping created.",
                Data = dto
            });
        }
    }
}
