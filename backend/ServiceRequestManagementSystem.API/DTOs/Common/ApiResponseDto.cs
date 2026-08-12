namespace ServiceRequestManagementSystem.API.DTOs.Common
{
    public class ApiResponseDto<T>
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public PaginationMetadataDto? Pagination { get; set; }
        public IDictionary<string, string[]>? Errors { get; set; }
        public string? Code { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class PaginationMetadataDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
    }
}
