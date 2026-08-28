namespace ServiceRequestManagementSystem.API.DTOs.Masters
{
    public class StatusDto
    {
        public int StatusId { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public string? ColorCode { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class DepartmentDto
    {
        public int DepartmentId { get; set; }

        public string DepartmentName { get; set; } = string.Empty;

        public string DepartmentCode { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class DepartmentPersonnelDto
    {
        public int DepartmentPersonnelId { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public int DepartmentId { get; set; }

        public string DepartmentName { get; set; } = string.Empty;

        public bool IsHOD { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class ServiceTypeDto
    {
        public int ServiceTypeId { get; set; }

        public string ServiceTypeName { get; set; } = string.Empty;

        public string ServiceTypeCode { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class RequestTypeDto
    {
        public int RequestTypeId { get; set; }

        public int ServiceTypeId { get; set; }

        public string? ServiceTypeName { get; set; }

        public string RequestTypeName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool RequiresApproval { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class RequestTypeTechnicianMappingDto
    {
        public int MappingId { get; set; }

        public int RequestTypeId { get; set; }

        public string RequestTypeName { get; set; } = string.Empty;

        public int DepartmentPersonnelId { get; set; }

        public string TechnicianName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
