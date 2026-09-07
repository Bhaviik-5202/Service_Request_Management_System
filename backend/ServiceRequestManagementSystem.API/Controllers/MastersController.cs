using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceRequestManagementSystem.API.Data;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.Masters;
using ServiceRequestManagementSystem.API.Enums;
using ServiceRequestManagementSystem.API.Models;

namespace ServiceRequestManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MastersController : ControllerBase
    {
        private readonly AppDbContext _context;

        private readonly IValidator<StatusDto> _statusValidator;
        private readonly IValidator<DepartmentDto> _departmentValidator;
        private readonly IValidator<DepartmentPersonnelDto> _personnelValidator;
        private readonly IValidator<ServiceTypeDto> _serviceTypeValidator;
        private readonly IValidator<RequestTypeDto> _requestTypeValidator;
        private readonly IValidator<RequestTypeTechnicianMappingDto> _mappingValidator;

        public MastersController(
            AppDbContext context,
            IValidator<StatusDto> statusValidator,
            IValidator<DepartmentDto> departmentValidator,
            IValidator<DepartmentPersonnelDto> personnelValidator,
            IValidator<ServiceTypeDto> serviceTypeValidator,
            IValidator<RequestTypeDto> requestTypeValidator,
            IValidator<RequestTypeTechnicianMappingDto> mappingValidator)
        {
            _context = context;
            _statusValidator = statusValidator;
            _departmentValidator = departmentValidator;
            _personnelValidator = personnelValidator;
            _serviceTypeValidator = serviceTypeValidator;
            _requestTypeValidator = requestTypeValidator;
            _mappingValidator = mappingValidator;
        }

        [HttpGet("statuses")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<StatusDto>>>> GetStatuses()
        {
            var statuses = await _context.ServiceRequestStatuses
                .AsNoTracking()
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
                Message = "Statuses retrieved successfully.",
                Data = statuses
            });
        }

        [HttpPost("statuses")]
        public async Task<ActionResult<ApiResponseDto<StatusDto>>> CreateStatus(
            StatusDto dto)
        {
            var validation = await _statusValidator.ValidateAsync(dto);

            if (!validation.IsValid)
            {
                return BadRequest(new ApiResponseDto<StatusDto>
                {
                    Success = false,
                    Message = validation.Errors.First().ErrorMessage
                });
            }

            dto.StatusName = dto.StatusName.Trim();

            var duplicate = await _context.ServiceRequestStatuses
                .AnyAsync(s => s.StatusName == dto.StatusName);

            if (duplicate)
            {
                return BadRequest(new ApiResponseDto<StatusDto>
                {
                    Success = false,
                    Message = "Status name already exists."
                });
            }

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
            StatusDto dto)
        {
            var validation = await _statusValidator.ValidateAsync(dto);

            if (!validation.IsValid)
            {
                return BadRequest(new ApiResponseDto<StatusDto>
                {
                    Success = false,
                    Message = validation.Errors.First().ErrorMessage
                });
            }

            var status = await _context.ServiceRequestStatuses
                .FirstOrDefaultAsync(s => s.StatusId == id);

            if (status == null)
            {
                return NotFound(new ApiResponseDto<StatusDto>
                {
                    Success = false,
                    Message = "Status not found."
                });
            }

            dto.StatusName = dto.StatusName.Trim();

            var duplicate = await _context.ServiceRequestStatuses
                .AnyAsync(s =>
                    s.StatusId != id &&
                    s.StatusName == dto.StatusName);

            if (duplicate)
            {
                return BadRequest(new ApiResponseDto<StatusDto>
                {
                    Success = false,
                    Message = "Status name already exists."
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
            var status = await _context.ServiceRequestStatuses
                .FirstOrDefaultAsync(s => s.StatusId == id);

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
                Message = "Status deactivated successfully.",
                Data = true
            });
        }

        [HttpGet("departments")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<DepartmentDto>>>> GetDepartments()
        {
            var departments = await _context.Departments
                .AsNoTracking()
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
                Message = "Departments retrieved successfully.",
                Data = departments
            });
        }

        [HttpPost("departments")]
        public async Task<ActionResult<ApiResponseDto<DepartmentDto>>> CreateDepartment(
            DepartmentDto dto)
        {
            var validation = await _departmentValidator.ValidateAsync(dto);

            if (!validation.IsValid)
            {
                return BadRequest(new ApiResponseDto<DepartmentDto>
                {
                    Success = false,
                    Message = validation.Errors.First().ErrorMessage
                });
            }

            dto.DepartmentName = dto.DepartmentName.Trim();
            dto.DepartmentCode = dto.DepartmentCode.Trim().ToUpperInvariant();

            var duplicate = await _context.Departments
                .AnyAsync(d => d.DepartmentCode == dto.DepartmentCode);

            if (duplicate)
            {
                return BadRequest(new ApiResponseDto<DepartmentDto>
                {
                    Success = false,
                    Message = "Department code already exists."
                });
            }

            var now = DateTime.UtcNow;

            var department = new Department
            {
                DepartmentName = dto.DepartmentName,
                DepartmentCode = dto.DepartmentCode,
                Description = dto.Description,
                IsActive = dto.IsActive,
                CreatedAt = now,
                UpdatedAt = now
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
            DepartmentDto dto)
        {
            var validation = await _departmentValidator.ValidateAsync(dto);

            if (!validation.IsValid)
            {
                return BadRequest(new ApiResponseDto<DepartmentDto>
                {
                    Success = false,
                    Message = validation.Errors.First().ErrorMessage
                });
            }

            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.DepartmentId == id);

            if (department == null)
            {
                return NotFound(new ApiResponseDto<DepartmentDto>
                {
                    Success = false,
                    Message = "Department not found."
                });
            }

            dto.DepartmentName = dto.DepartmentName.Trim();
            dto.DepartmentCode = dto.DepartmentCode.Trim().ToUpperInvariant();

            var duplicate = await _context.Departments
                .AnyAsync(d =>
                    d.DepartmentId != id &&
                    d.DepartmentCode == dto.DepartmentCode);

            if (duplicate)
            {
                return BadRequest(new ApiResponseDto<DepartmentDto>
                {
                    Success = false,
                    Message = "Department code already exists."
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
            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.DepartmentId == id);

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
            department.IsActive = false;
            department.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new ApiResponseDto<bool>
            {
                Success = true,
                Message = "Department deleted successfully.",
                Data = true
            });
        }

        [HttpGet("personnel")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<DepartmentPersonnelDto>>>> GetPersonnel()
        {
            var personnel = await _context.DepartmentPersonnel
                .AsNoTracking()
                .OrderBy(p => p.DepartmentPersonnelId)
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
                Message = "Department personnel retrieved successfully.",
                Data = personnel
            });
        }

        [HttpPost("personnel")]
        public async Task<ActionResult<ApiResponseDto<DepartmentPersonnelDto>>> MapPersonnel(
            DepartmentPersonnelDto dto)
        {
            var validation = await _personnelValidator.ValidateAsync(dto);

            if (!validation.IsValid)
            {
                return BadRequest(new ApiResponseDto<DepartmentPersonnelDto>
                {
                    Success = false,
                    Message = validation.Errors.First().ErrorMessage
                });
            }

            var userExists = await _context.Users
                .AnyAsync(u =>
                    u.UserId == dto.UserId &&
                    u.Status == UserStatus.Active);

            if (!userExists)
            {
                return BadRequest(new ApiResponseDto<DepartmentPersonnelDto>
                {
                    Success = false,
                    Message = "Invalid or inactive user."
                });
            }

            var departmentExists = await _context.Departments
                .AnyAsync(d =>
                    d.DepartmentId == dto.DepartmentId &&
                    d.IsActive);

            if (!departmentExists)
            {
                return BadRequest(new ApiResponseDto<DepartmentPersonnelDto>
                {
                    Success = false,
                    Message = "Invalid or inactive department."
                });
            }

            var duplicate = await _context.DepartmentPersonnel
                .AnyAsync(p =>
                    p.UserId == dto.UserId &&
                    p.DepartmentId == dto.DepartmentId &&
                    p.IsActive);

            if (duplicate)
            {
                return BadRequest(new ApiResponseDto<DepartmentPersonnelDto>
                {
                    Success = false,
                    Message = "This user is already mapped to the department."
                });
            }

            var now = DateTime.UtcNow;

            var personnel = new DepartmentPersonnel
            {
                UserId = dto.UserId,
                DepartmentId = dto.DepartmentId,
                IsHOD = dto.IsHOD,
                IsActive = dto.IsActive,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.DepartmentPersonnel.Add(personnel);
            await _context.SaveChangesAsync();

            dto.DepartmentPersonnelId = personnel.DepartmentPersonnelId;

            await PopulatePersonnelDisplayFieldsAsync(dto, personnel);

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
            DepartmentPersonnelDto dto)
        {
            var validation = await _personnelValidator.ValidateAsync(dto);

            if (!validation.IsValid)
            {
                return BadRequest(new ApiResponseDto<DepartmentPersonnelDto>
                {
                    Success = false,
                    Message = validation.Errors.First().ErrorMessage
                });
            }

            var personnel = await _context.DepartmentPersonnel
                .FirstOrDefaultAsync(p => p.DepartmentPersonnelId == id);

            if (personnel == null)
            {
                return NotFound(new ApiResponseDto<DepartmentPersonnelDto>
                {
                    Success = false,
                    Message = "Personnel mapping not found."
                });
            }

            var userExists = await _context.Users
                .AnyAsync(u =>
                    u.UserId == dto.UserId &&
                    u.Status == UserStatus.Active);

            if (!userExists)
            {
                return BadRequest(new ApiResponseDto<DepartmentPersonnelDto>
                {
                    Success = false,
                    Message = "Invalid or inactive user."
                });
            }

            var departmentExists = await _context.Departments
                .AnyAsync(d =>
                    d.DepartmentId == dto.DepartmentId &&
                    d.IsActive);

            if (!departmentExists)
            {
                return BadRequest(new ApiResponseDto<DepartmentPersonnelDto>
                {
                    Success = false,
                    Message = "Invalid or inactive department."
                });
            }

            var duplicate = await _context.DepartmentPersonnel
                .AnyAsync(p =>
                    p.DepartmentPersonnelId != id &&
                    p.UserId == dto.UserId &&
                    p.DepartmentId == dto.DepartmentId &&
                    p.IsActive);

            if (duplicate)
            {
                return BadRequest(new ApiResponseDto<DepartmentPersonnelDto>
                {
                    Success = false,
                    Message = "This user is already mapped to the department."
                });
            }

            personnel.UserId = dto.UserId;
            personnel.DepartmentId = dto.DepartmentId;
            personnel.IsHOD = dto.IsHOD;
            personnel.IsActive = dto.IsActive;
            personnel.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            dto.DepartmentPersonnelId = personnel.DepartmentPersonnelId;

            await PopulatePersonnelDisplayFieldsAsync(dto, personnel);

            return Ok(new ApiResponseDto<DepartmentPersonnelDto>
            {
                Success = true,
                Message = "Personnel mapping updated successfully.",
                Data = dto
            });
        }

        [HttpDelete("personnel/{id}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeletePersonnel(int id)
        {
            var personnel = await _context.DepartmentPersonnel
                .FirstOrDefaultAsync(p => p.DepartmentPersonnelId == id);

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
            personnel.IsActive = false;
            personnel.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new ApiResponseDto<bool>
            {
                Success = true,
                Message = "Personnel mapping deleted successfully.",
                Data = true
            });
        }

        [HttpGet("service-types")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ServiceTypeDto>>>> GetServiceTypes()
        {
            var serviceTypes = await _context.ServiceTypes
                .AsNoTracking()
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
                Message = "Service types retrieved successfully.",
                Data = serviceTypes
            });
        }

        [HttpPost("service-types")]
        public async Task<ActionResult<ApiResponseDto<ServiceTypeDto>>> CreateServiceType(
            ServiceTypeDto dto)
        {
            var validation = await _serviceTypeValidator.ValidateAsync(dto);

            if (!validation.IsValid)
            {
                return BadRequest(new ApiResponseDto<ServiceTypeDto>
                {
                    Success = false,
                    Message = validation.Errors.First().ErrorMessage
                });
            }

            dto.ServiceTypeName = dto.ServiceTypeName.Trim();
            dto.ServiceTypeCode = dto.ServiceTypeCode
                .Trim()
                .ToUpperInvariant();

            var duplicate = await _context.ServiceTypes
                .AnyAsync(s => s.ServiceTypeCode == dto.ServiceTypeCode);

            if (duplicate)
            {
                return BadRequest(new ApiResponseDto<ServiceTypeDto>
                {
                    Success = false,
                    Message = "Service type code already exists."
                });
            }

            var now = DateTime.UtcNow;

            var serviceType = new ServiceType
            {
                ServiceTypeName = dto.ServiceTypeName,
                ServiceTypeCode = dto.ServiceTypeCode,
                Description = dto.Description,
                IsActive = dto.IsActive,
                CreatedAt = now,
                UpdatedAt = now
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
                .AsNoTracking()
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
                Message = "Request types retrieved successfully.",
                Data = requestTypes
            });
        }

        [HttpPost("request-types")]
        public async Task<ActionResult<ApiResponseDto<RequestTypeDto>>> CreateRequestType(
            RequestTypeDto dto)
        {
            var validation = await _requestTypeValidator.ValidateAsync(dto);

            if (!validation.IsValid)
            {
                return BadRequest(new ApiResponseDto<RequestTypeDto>
                {
                    Success = false,
                    Message = validation.Errors.First().ErrorMessage
                });
            }

            var serviceTypeExists = await _context.ServiceTypes
                .AnyAsync(s =>
                    s.ServiceTypeId == dto.ServiceTypeId &&
                    s.IsActive);

            if (!serviceTypeExists)
            {
                return BadRequest(new ApiResponseDto<RequestTypeDto>
                {
                    Success = false,
                    Message = "Invalid or inactive service type."
                });
            }

            dto.RequestTypeName = dto.RequestTypeName.Trim();

            var duplicate = await _context.RequestTypes
                .AnyAsync(r =>
                    r.ServiceTypeId == dto.ServiceTypeId &&
                    r.RequestTypeName == dto.RequestTypeName);

            if (duplicate)
            {
                return BadRequest(new ApiResponseDto<RequestTypeDto>
                {
                    Success = false,
                    Message = "Request type already exists for this service type."
                });
            }

            var now = DateTime.UtcNow;

            var requestType = new RequestType
            {
                ServiceTypeId = dto.ServiceTypeId,
                RequestTypeName = dto.RequestTypeName,
                Description = dto.Description,
                RequiresApproval = dto.RequiresApproval,
                IsActive = dto.IsActive,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.RequestTypes.Add(requestType);
            await _context.SaveChangesAsync();

            dto.RequestTypeId = requestType.RequestTypeId;

            dto.ServiceTypeName = await _context.ServiceTypes
                .Where(s => s.ServiceTypeId == requestType.ServiceTypeId)
                .Select(s => s.ServiceTypeName)
                .FirstOrDefaultAsync();

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
                .AsNoTracking()
                .OrderBy(m => m.MappingId)
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
                Message = "Technician mappings retrieved successfully.",
                Data = mappings
            });
        }

        [HttpPost("mappings")]
        public async Task<ActionResult<ApiResponseDto<RequestTypeTechnicianMappingDto>>> CreateMapping(
            RequestTypeTechnicianMappingDto dto)
        {
            var validation = await _mappingValidator.ValidateAsync(dto);

            if (!validation.IsValid)
            {
                return BadRequest(new ApiResponseDto<RequestTypeTechnicianMappingDto>
                {
                    Success = false,
                    Message = validation.Errors.First().ErrorMessage
                });
            }

            var requestTypeExists = await _context.RequestTypes
                .AnyAsync(r =>
                    r.RequestTypeId == dto.RequestTypeId &&
                    r.IsActive);

            if (!requestTypeExists)
            {
                return BadRequest(new ApiResponseDto<RequestTypeTechnicianMappingDto>
                {
                    Success = false,
                    Message = "Invalid or inactive request type."
                });
            }

            var personnelExists = await _context.DepartmentPersonnel
                .AnyAsync(p =>
                    p.DepartmentPersonnelId == dto.DepartmentPersonnelId &&
                    p.IsActive);

            if (!personnelExists)
            {
                return BadRequest(new ApiResponseDto<RequestTypeTechnicianMappingDto>
                {
                    Success = false,
                    Message = "Invalid or inactive department personnel."
                });
            }

            var duplicate = await _context.RequestTypeTechnicianMappings
                .AnyAsync(m =>
                    m.RequestTypeId == dto.RequestTypeId &&
                    m.DepartmentPersonnelId == dto.DepartmentPersonnelId &&
                    m.IsActive);

            if (duplicate)
            {
                return BadRequest(new ApiResponseDto<RequestTypeTechnicianMappingDto>
                {
                    Success = false,
                    Message = "This technician is already mapped to the request type."
                });
            }

            var now = DateTime.UtcNow;

            var mapping = new RequestTypeTechnicianMapping
            {
                RequestTypeId = dto.RequestTypeId,
                DepartmentPersonnelId = dto.DepartmentPersonnelId,
                IsActive = dto.IsActive,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.RequestTypeTechnicianMappings.Add(mapping);
            await _context.SaveChangesAsync();

            dto.MappingId = mapping.MappingId;

            dto.RequestTypeName = await _context.RequestTypes
                .Where(r => r.RequestTypeId == mapping.RequestTypeId)
                .Select(r => r.RequestTypeName)
                .FirstOrDefaultAsync() ?? string.Empty;

            dto.TechnicianName = await _context.DepartmentPersonnel
                .Where(p => p.DepartmentPersonnelId == mapping.DepartmentPersonnelId)
                .Select(p => p.User!.FullName)
                .FirstOrDefaultAsync() ?? string.Empty;

            return Ok(new ApiResponseDto<RequestTypeTechnicianMappingDto>
            {
                Success = true,
                Message = "Technician mapping created successfully.",
                Data = dto
            });
        }

        private async Task PopulatePersonnelDisplayFieldsAsync(
            DepartmentPersonnelDto dto,
            DepartmentPersonnel personnel)
        {
            dto.UserName = await _context.Users
                .Where(user => user.UserId == personnel.UserId)
                .Select(user => user.FullName)
                .FirstOrDefaultAsync() ?? string.Empty;

            dto.DepartmentName = await _context.Departments
                .Where(department => department.DepartmentId == personnel.DepartmentId)
                .Select(department => department.DepartmentName)
                .FirstOrDefaultAsync() ?? string.Empty;
        }
    }
}
