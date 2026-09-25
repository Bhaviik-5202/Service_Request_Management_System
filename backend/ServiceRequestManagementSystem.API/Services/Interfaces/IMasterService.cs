using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.Masters;

namespace ServiceRequestManagementSystem.API.Services.Interfaces
{
    public interface IMasterService
    {
        // Statuses
        Task<ApiResponseDto<IEnumerable<StatusDto>>> GetStatusesAsync();
        Task<ApiResponseDto<StatusDto>> GetStatusByIdAsync(int id);
        Task<ApiResponseDto<StatusDto>> CreateStatusAsync(StatusDto dto);
        Task<ApiResponseDto<StatusDto>> UpdateStatusAsync(int id, StatusDto dto);
        Task<ApiResponseDto<object>> DeleteStatusAsync(int id);

        // Departments
        Task<ApiResponseDto<IEnumerable<DepartmentDto>>> GetDepartmentsAsync();
        Task<ApiResponseDto<DepartmentDto>> GetDepartmentByIdAsync(int id);
        Task<ApiResponseDto<DepartmentDto>> CreateDepartmentAsync(DepartmentDto dto);
        Task<ApiResponseDto<DepartmentDto>> UpdateDepartmentAsync(int id, DepartmentDto dto);
        Task<ApiResponseDto<object>> DeleteDepartmentAsync(int id);

        // Department Personnel
        Task<ApiResponseDto<IEnumerable<DepartmentPersonnelDto>>> GetPersonnelByDepartmentAsync(int departmentId);
        Task<ApiResponseDto<DepartmentPersonnelDto>> AddPersonnelAsync(DepartmentPersonnelDto dto);
        Task<ApiResponseDto<object>> RemovePersonnelAsync(int personnelId);

        // Service Types
        Task<ApiResponseDto<IEnumerable<ServiceTypeDto>>> GetServiceTypesAsync();
        Task<ApiResponseDto<ServiceTypeDto>> GetServiceTypeByIdAsync(int id);
        Task<ApiResponseDto<ServiceTypeDto>> CreateServiceTypeAsync(ServiceTypeDto dto);
        Task<ApiResponseDto<ServiceTypeDto>> UpdateServiceTypeAsync(int id, ServiceTypeDto dto);
        Task<ApiResponseDto<object>> DeleteServiceTypeAsync(int id);

        // Request Types
        Task<ApiResponseDto<IEnumerable<RequestTypeDto>>> GetRequestTypesAsync(int? serviceTypeId);
        Task<ApiResponseDto<RequestTypeDto>> GetRequestTypeByIdAsync(int id);
        Task<ApiResponseDto<RequestTypeDto>> CreateRequestTypeAsync(RequestTypeDto dto);
        Task<ApiResponseDto<RequestTypeDto>> UpdateRequestTypeAsync(int id, RequestTypeDto dto);
        Task<ApiResponseDto<object>> DeleteRequestTypeAsync(int id);

        // Technician Mappings
        Task<ApiResponseDto<IEnumerable<RequestTypeTechnicianMappingDto>>> GetMappingsByRequestTypeAsync(int requestTypeId);
        Task<ApiResponseDto<RequestTypeTechnicianMappingDto>> AddMappingAsync(RequestTypeTechnicianMappingDto dto);
        Task<ApiResponseDto<object>> RemoveMappingAsync(int mappingId);
    }
}
