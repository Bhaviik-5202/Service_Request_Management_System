using Microsoft.EntityFrameworkCore;
using ServiceRequestManagementSystem.API.Data;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.Masters;
using ServiceRequestManagementSystem.API.Models;
using ServiceRequestManagementSystem.API.Repositories.Interfaces;
using ServiceRequestManagementSystem.API.Services.Interfaces;

namespace ServiceRequestManagementSystem.API.Services.Implementations
{
    public class MasterService : IMasterService
    {
        private readonly IUnitOfWork _uow;
        private readonly AppDbContext _context;

        public MasterService(IUnitOfWork uow, AppDbContext context)
        {
            _uow = uow;
            _context = context;
        }

        // ─────────────── STATUSES ───────────────

        public async Task<ApiResponseDto<IEnumerable<StatusDto>>> GetStatusesAsync()
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

            return Ok("Statuses retrieved successfully.", (IEnumerable<StatusDto>)statuses);
        }

        public async Task<ApiResponseDto<StatusDto>> GetStatusByIdAsync(int id)
        {
            var s = await _uow.ServiceRequestStatuses.GetByIdAsync(id);
            if (s == null) return Fail<StatusDto>("Status not found.");
            return Ok("Status retrieved successfully.", new StatusDto
            {
                StatusId = s.StatusId, StatusName = s.StatusName,
                ColorCode = s.ColorCode, Description = s.Description, IsActive = s.IsActive
            });
        }

        public async Task<ApiResponseDto<StatusDto>> CreateStatusAsync(StatusDto dto)
        {
            dto.StatusName = dto.StatusName.Trim();
            if (await _uow.ServiceRequestStatuses.ExistsAsync(s => s.StatusName == dto.StatusName))
                return Fail<StatusDto>("Status name already exists.");

            var status = new ServiceRequestStatus
            {
                StatusName = dto.StatusName, ColorCode = dto.ColorCode,
                Description = dto.Description, IsActive = dto.IsActive, CreatedAt = DateTime.UtcNow
            };
            await _uow.ServiceRequestStatuses.AddAsync(status);
            await _uow.SaveChangesAsync();
            dto.StatusId = status.StatusId;
            return Ok("Status created successfully.", dto);
        }

        public async Task<ApiResponseDto<StatusDto>> UpdateStatusAsync(int id, StatusDto dto)
        {
            var status = await _uow.ServiceRequestStatuses.GetByIdAsync(id);
            if (status == null) return Fail<StatusDto>("Status not found.");

            dto.StatusName = dto.StatusName.Trim();
            if (await _uow.ServiceRequestStatuses.ExistsAsync(s => s.StatusId != id && s.StatusName == dto.StatusName))
                return Fail<StatusDto>("Status name already exists.");

            status.StatusName = dto.StatusName; status.ColorCode = dto.ColorCode;
            status.Description = dto.Description; status.IsActive = dto.IsActive;
            _uow.ServiceRequestStatuses.Update(status);
            await _uow.SaveChangesAsync();
            dto.StatusId = status.StatusId;
            return Ok("Status updated successfully.", dto);
        }

        public async Task<ApiResponseDto<object>> DeleteStatusAsync(int id)
        {
            var status = await _uow.ServiceRequestStatuses.GetByIdAsync(id);
            if (status == null) return Fail<object>("Status not found.");
            status.IsActive = false;
            _uow.ServiceRequestStatuses.Update(status);
            await _uow.SaveChangesAsync();
            return Ok<object>("Status deactivated successfully.", null!);
        }

        // ─────────────── DEPARTMENTS ───────────────

        public async Task<ApiResponseDto<IEnumerable<DepartmentDto>>> GetDepartmentsAsync()
        {
            var depts = await _context.Departments
                .AsNoTracking()
                .Where(d => !d.IsDeleted)
                .OrderBy(d => d.DepartmentId)
                .Select(d => new DepartmentDto
                {
                    DepartmentId = d.DepartmentId, DepartmentName = d.DepartmentName,
                    DepartmentCode = d.DepartmentCode, Description = d.Description, IsActive = d.IsActive
                })
                .ToListAsync();

            return Ok("Departments retrieved successfully.", (IEnumerable<DepartmentDto>)depts);
        }

        public async Task<ApiResponseDto<DepartmentDto>> GetDepartmentByIdAsync(int id)
        {
            var d = await _uow.Departments.FirstOrDefaultAsync(x => x.DepartmentId == id && !x.IsDeleted);
            if (d == null) return Fail<DepartmentDto>("Department not found.");
            return Ok("Department retrieved successfully.", new DepartmentDto
            {
                DepartmentId = d.DepartmentId, DepartmentName = d.DepartmentName,
                DepartmentCode = d.DepartmentCode, Description = d.Description, IsActive = d.IsActive
            });
        }

        public async Task<ApiResponseDto<DepartmentDto>> CreateDepartmentAsync(DepartmentDto dto)
        {
            dto.DepartmentName = dto.DepartmentName.Trim();
            dto.DepartmentCode = dto.DepartmentCode.Trim().ToUpperInvariant();

            if (await _uow.Departments.ExistsAsync(d =>
                (d.DepartmentName == dto.DepartmentName || d.DepartmentCode == dto.DepartmentCode) && !d.IsDeleted))
                return Fail<DepartmentDto>("Department name or code already exists.");

            var now = DateTime.UtcNow;
            var dept = new Department
            {
                DepartmentName = dto.DepartmentName, DepartmentCode = dto.DepartmentCode,
                Description = dto.Description, IsActive = dto.IsActive,
                CreatedAt = now, UpdatedAt = now
            };
            await _uow.Departments.AddAsync(dept);
            await _uow.SaveChangesAsync();
            dto.DepartmentId = dept.DepartmentId;
            return Ok("Department created successfully.", dto);
        }

        public async Task<ApiResponseDto<DepartmentDto>> UpdateDepartmentAsync(int id, DepartmentDto dto)
        {
            var dept = await _uow.Departments.FirstOrDefaultAsync(d => d.DepartmentId == id && !d.IsDeleted);
            if (dept == null) return Fail<DepartmentDto>("Department not found.");

            dto.DepartmentName = dto.DepartmentName.Trim();
            dto.DepartmentCode = dto.DepartmentCode.Trim().ToUpperInvariant();

            if (await _uow.Departments.ExistsAsync(d =>
                d.DepartmentId != id &&
                (d.DepartmentName == dto.DepartmentName || d.DepartmentCode == dto.DepartmentCode) && !d.IsDeleted))
                return Fail<DepartmentDto>("Department name or code already exists.");

            dept.DepartmentName = dto.DepartmentName; dept.DepartmentCode = dto.DepartmentCode;
            dept.Description = dto.Description; dept.IsActive = dto.IsActive;
            dept.UpdatedAt = DateTime.UtcNow;
            _uow.Departments.Update(dept);
            await _uow.SaveChangesAsync();
            dto.DepartmentId = dept.DepartmentId;
            return Ok("Department updated successfully.", dto);
        }

        public async Task<ApiResponseDto<object>> DeleteDepartmentAsync(int id)
        {
            var dept = await _uow.Departments.FirstOrDefaultAsync(d => d.DepartmentId == id && !d.IsDeleted);
            if (dept == null) return Fail<object>("Department not found.");
            dept.IsDeleted = true; dept.UpdatedAt = DateTime.UtcNow;
            _uow.Departments.Update(dept);
            await _uow.SaveChangesAsync();
            return Ok<object>("Department deleted successfully.", null!);
        }

        // ─────────────── DEPARTMENT PERSONNEL ───────────────

        public async Task<ApiResponseDto<IEnumerable<DepartmentPersonnelDto>>> GetPersonnelByDepartmentAsync(int departmentId)
        {
            var personnel = await _context.DepartmentPersonnel
                .AsNoTracking()
                .Where(p => p.DepartmentId == departmentId && p.IsActive)
                .Select(p => new DepartmentPersonnelDto
                {
                    DepartmentPersonnelId = p.DepartmentPersonnelId,
                    UserId = p.UserId,
                    UserName = p.User != null ? p.User.FullName : string.Empty,
                    DepartmentId = p.DepartmentId,
                    DepartmentName = p.Department != null ? p.Department.DepartmentName : string.Empty,
                    IsHOD = p.IsHOD,
                    IsActive = p.IsActive
                })
                .ToListAsync();

            return Ok("Department personnel retrieved successfully.", (IEnumerable<DepartmentPersonnelDto>)personnel);
        }

        public async Task<ApiResponseDto<DepartmentPersonnelDto>> AddPersonnelAsync(DepartmentPersonnelDto dto)
        {
            var exists = await _uow.DepartmentPersonnel.ExistsAsync(p =>
                p.UserId == dto.UserId && p.DepartmentId == dto.DepartmentId && p.IsActive);
            if (exists) return Fail<DepartmentPersonnelDto>("User is already a member of this department.");

            var personnel = new DepartmentPersonnel
            {
                UserId = dto.UserId, DepartmentId = dto.DepartmentId,
                IsHOD = dto.IsHOD, IsActive = dto.IsActive
            };
            await _uow.DepartmentPersonnel.AddAsync(personnel);
            await _uow.SaveChangesAsync();
            dto.DepartmentPersonnelId = personnel.DepartmentPersonnelId;
            return Ok("Personnel added successfully.", dto);
        }

        public async Task<ApiResponseDto<object>> RemovePersonnelAsync(int personnelId)
        {
            var personnel = await _uow.DepartmentPersonnel.GetByIdAsync(personnelId);
            if (personnel == null) return Fail<object>("Personnel record not found.");
            personnel.IsActive = false;
            _uow.DepartmentPersonnel.Update(personnel);
            await _uow.SaveChangesAsync();
            return Ok<object>("Personnel removed successfully.", null!);
        }

        // ─────────────── SERVICE TYPES ───────────────

        public async Task<ApiResponseDto<IEnumerable<ServiceTypeDto>>> GetServiceTypesAsync()
        {
            var types = await _context.ServiceTypes
                .AsNoTracking()
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.ServiceTypeId)
                .Select(s => new ServiceTypeDto
                {
                    ServiceTypeId = s.ServiceTypeId, ServiceTypeName = s.ServiceTypeName,
                    ServiceTypeCode = s.ServiceTypeCode, Description = s.Description, IsActive = s.IsActive
                })
                .ToListAsync();

            return Ok("Service types retrieved successfully.", (IEnumerable<ServiceTypeDto>)types);
        }

        public async Task<ApiResponseDto<ServiceTypeDto>> GetServiceTypeByIdAsync(int id)
        {
            var s = await _uow.ServiceTypes.FirstOrDefaultAsync(x => x.ServiceTypeId == id && !x.IsDeleted);
            if (s == null) return Fail<ServiceTypeDto>("Service type not found.");
            return Ok("Service type retrieved successfully.", new ServiceTypeDto
            {
                ServiceTypeId = s.ServiceTypeId, ServiceTypeName = s.ServiceTypeName,
                ServiceTypeCode = s.ServiceTypeCode, Description = s.Description, IsActive = s.IsActive
            });
        }

        public async Task<ApiResponseDto<ServiceTypeDto>> CreateServiceTypeAsync(ServiceTypeDto dto)
        {
            dto.ServiceTypeName = dto.ServiceTypeName.Trim();
            dto.ServiceTypeCode = dto.ServiceTypeCode.Trim().ToUpperInvariant();

            if (await _uow.ServiceTypes.ExistsAsync(s =>
                (s.ServiceTypeName == dto.ServiceTypeName || s.ServiceTypeCode == dto.ServiceTypeCode) && !s.IsDeleted))
                return Fail<ServiceTypeDto>("Service type name or code already exists.");

            var now = DateTime.UtcNow;
            var svcType = new ServiceType
            {
                ServiceTypeName = dto.ServiceTypeName, ServiceTypeCode = dto.ServiceTypeCode,
                Description = dto.Description, IsActive = dto.IsActive, CreatedAt = now, UpdatedAt = now
            };
            await _uow.ServiceTypes.AddAsync(svcType);
            await _uow.SaveChangesAsync();
            dto.ServiceTypeId = svcType.ServiceTypeId;
            return Ok("Service type created successfully.", dto);
        }

        public async Task<ApiResponseDto<ServiceTypeDto>> UpdateServiceTypeAsync(int id, ServiceTypeDto dto)
        {
            var svcType = await _uow.ServiceTypes.FirstOrDefaultAsync(s => s.ServiceTypeId == id && !s.IsDeleted);
            if (svcType == null) return Fail<ServiceTypeDto>("Service type not found.");

            dto.ServiceTypeName = dto.ServiceTypeName.Trim();
            dto.ServiceTypeCode = dto.ServiceTypeCode.Trim().ToUpperInvariant();

            if (await _uow.ServiceTypes.ExistsAsync(s =>
                s.ServiceTypeId != id &&
                (s.ServiceTypeName == dto.ServiceTypeName || s.ServiceTypeCode == dto.ServiceTypeCode) && !s.IsDeleted))
                return Fail<ServiceTypeDto>("Service type name or code already exists.");

            svcType.ServiceTypeName = dto.ServiceTypeName; svcType.ServiceTypeCode = dto.ServiceTypeCode;
            svcType.Description = dto.Description; svcType.IsActive = dto.IsActive;
            svcType.UpdatedAt = DateTime.UtcNow;
            _uow.ServiceTypes.Update(svcType);
            await _uow.SaveChangesAsync();
            dto.ServiceTypeId = svcType.ServiceTypeId;
            return Ok("Service type updated successfully.", dto);
        }

        public async Task<ApiResponseDto<object>> DeleteServiceTypeAsync(int id)
        {
            var svcType = await _uow.ServiceTypes.FirstOrDefaultAsync(s => s.ServiceTypeId == id && !s.IsDeleted);
            if (svcType == null) return Fail<object>("Service type not found.");
            svcType.IsDeleted = true; svcType.UpdatedAt = DateTime.UtcNow;
            _uow.ServiceTypes.Update(svcType);
            await _uow.SaveChangesAsync();
            return Ok<object>("Service type deleted successfully.", null!);
        }

        // ─────────────── REQUEST TYPES ───────────────

        public async Task<ApiResponseDto<IEnumerable<RequestTypeDto>>> GetRequestTypesAsync(int? serviceTypeId)
        {
            var query = _context.RequestTypes
                .AsNoTracking()
                .Where(r => !r.IsDeleted);

            if (serviceTypeId.HasValue)
                query = query.Where(r => r.ServiceTypeId == serviceTypeId.Value);

            var types = await query.OrderBy(r => r.RequestTypeId)
                .Select(r => new RequestTypeDto
                {
                    RequestTypeId = r.RequestTypeId, ServiceTypeId = r.ServiceTypeId,
                    ServiceTypeName = r.ServiceType != null ? r.ServiceType.ServiceTypeName : null,
                    RequestTypeName = r.RequestTypeName, Description = r.Description,
                    RequiresApproval = r.RequiresApproval, IsActive = r.IsActive
                })
                .ToListAsync();

            return Ok("Request types retrieved successfully.", (IEnumerable<RequestTypeDto>)types);
        }

        public async Task<ApiResponseDto<RequestTypeDto>> GetRequestTypeByIdAsync(int id)
        {
            var r = await _context.RequestTypes
                .AsNoTracking()
                .Where(x => x.RequestTypeId == id && !x.IsDeleted)
                .Select(x => new RequestTypeDto
                {
                    RequestTypeId = x.RequestTypeId, ServiceTypeId = x.ServiceTypeId,
                    ServiceTypeName = x.ServiceType != null ? x.ServiceType.ServiceTypeName : null,
                    RequestTypeName = x.RequestTypeName, Description = x.Description,
                    RequiresApproval = x.RequiresApproval, IsActive = x.IsActive
                })
                .FirstOrDefaultAsync();

            if (r == null) return Fail<RequestTypeDto>("Request type not found.");
            return Ok("Request type retrieved successfully.", r);
        }

        public async Task<ApiResponseDto<RequestTypeDto>> CreateRequestTypeAsync(RequestTypeDto dto)
        {
            dto.RequestTypeName = dto.RequestTypeName.Trim();

            var serviceType = await _uow.ServiceTypes.FirstOrDefaultAsync(s => s.ServiceTypeId == dto.ServiceTypeId && s.IsActive && !s.IsDeleted);
            if (serviceType == null)
                return Fail<RequestTypeDto>("Service type not found or inactive.");

            if (await _uow.RequestTypes.ExistsAsync(r =>
                r.ServiceTypeId == dto.ServiceTypeId && r.RequestTypeName == dto.RequestTypeName && !r.IsDeleted))
                return Fail<RequestTypeDto>("Request type name already exists for this service type.");

            var now = DateTime.UtcNow;
            var reqType = new RequestType
            {
                ServiceTypeId = dto.ServiceTypeId, RequestTypeName = dto.RequestTypeName,
                Description = dto.Description, RequiresApproval = dto.RequiresApproval,
                IsActive = dto.IsActive, CreatedAt = now, UpdatedAt = now
            };
            await _uow.RequestTypes.AddAsync(reqType);
            await _uow.SaveChangesAsync();
            dto.RequestTypeId = reqType.RequestTypeId;
            dto.ServiceTypeName = serviceType.ServiceTypeName;
            return Ok("Request type created successfully.", dto);
        }

        public async Task<ApiResponseDto<RequestTypeDto>> UpdateRequestTypeAsync(int id, RequestTypeDto dto)
        {
            var reqType = await _uow.RequestTypes.FirstOrDefaultAsync(r => r.RequestTypeId == id && !r.IsDeleted);
            if (reqType == null) return Fail<RequestTypeDto>("Request type not found.");

            dto.RequestTypeName = dto.RequestTypeName.Trim();

            if (await _uow.RequestTypes.ExistsAsync(r =>
                r.RequestTypeId != id && r.ServiceTypeId == dto.ServiceTypeId &&
                r.RequestTypeName == dto.RequestTypeName && !r.IsDeleted))
                return Fail<RequestTypeDto>("Request type name already exists for this service type.");

            reqType.ServiceTypeId = dto.ServiceTypeId; reqType.RequestTypeName = dto.RequestTypeName;
            reqType.Description = dto.Description; reqType.RequiresApproval = dto.RequiresApproval;
            reqType.IsActive = dto.IsActive; reqType.UpdatedAt = DateTime.UtcNow;
            _uow.RequestTypes.Update(reqType);
            await _uow.SaveChangesAsync();
            dto.RequestTypeId = reqType.RequestTypeId;
            return Ok("Request type updated successfully.", dto);
        }

        public async Task<ApiResponseDto<object>> DeleteRequestTypeAsync(int id)
        {
            var reqType = await _uow.RequestTypes.FirstOrDefaultAsync(r => r.RequestTypeId == id && !r.IsDeleted);
            if (reqType == null) return Fail<object>("Request type not found.");
            reqType.IsDeleted = true; reqType.UpdatedAt = DateTime.UtcNow;
            _uow.RequestTypes.Update(reqType);
            await _uow.SaveChangesAsync();
            return Ok<object>("Request type deleted successfully.", null!);
        }

        // ─────────────── TECHNICIAN MAPPINGS ───────────────

        public async Task<ApiResponseDto<IEnumerable<RequestTypeTechnicianMappingDto>>> GetMappingsByRequestTypeAsync(int requestTypeId)
        {
            var mappings = await _context.RequestTypeTechnicianMappings
                .AsNoTracking()
                .Where(m => m.RequestTypeId == requestTypeId && m.IsActive)
                .Select(m => new RequestTypeTechnicianMappingDto
                {
                    MappingId = m.MappingId,
                    RequestTypeId = m.RequestTypeId,
                    RequestTypeName = m.RequestType != null ? m.RequestType.RequestTypeName : string.Empty,
                    DepartmentPersonnelId = m.DepartmentPersonnelId,
                    TechnicianName = m.DepartmentPersonnel != null && m.DepartmentPersonnel.User != null
                        ? m.DepartmentPersonnel.User.FullName
                        : string.Empty,
                    IsActive = m.IsActive
                })
                .ToListAsync();

            return Ok("Technician mappings retrieved successfully.", (IEnumerable<RequestTypeTechnicianMappingDto>)mappings);
        }

        public async Task<ApiResponseDto<RequestTypeTechnicianMappingDto>> AddMappingAsync(RequestTypeTechnicianMappingDto dto)
        {
            var exists = await _uow.RequestTypeTechnicianMappings.ExistsAsync(m =>
                m.RequestTypeId == dto.RequestTypeId &&
                m.DepartmentPersonnelId == dto.DepartmentPersonnelId &&
                m.IsActive);
            if (exists) return Fail<RequestTypeTechnicianMappingDto>("This technician is already mapped to this request type.");

            var mapping = new RequestTypeTechnicianMapping
            {
                RequestTypeId = dto.RequestTypeId,
                DepartmentPersonnelId = dto.DepartmentPersonnelId,
                IsActive = dto.IsActive
            };
            await _uow.RequestTypeTechnicianMappings.AddAsync(mapping);
            await _uow.SaveChangesAsync();
            dto.MappingId = mapping.MappingId;
            return Ok("Technician mapped successfully.", dto);
        }

        public async Task<ApiResponseDto<object>> RemoveMappingAsync(int mappingId)
        {
            var mapping = await _uow.RequestTypeTechnicianMappings.GetByIdAsync(mappingId);
            if (mapping == null) return Fail<object>("Mapping not found.");
            mapping.IsActive = false;
            _uow.RequestTypeTechnicianMappings.Update(mapping);
            await _uow.SaveChangesAsync();
            return Ok<object>("Technician mapping removed successfully.", null!);
        }

        private static ApiResponseDto<T> Ok<T>(string message, T data) =>
            new() { Success = true, Message = message, Data = data };

        private static ApiResponseDto<T> Fail<T>(string message) =>
            new() { Success = false, Message = message, Data = default };
    }
}
