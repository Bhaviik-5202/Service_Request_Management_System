namespace ServiceRequestManagementSystem.API.DTOs.Common
{
    public class ApiResponseDto<T>
    {
        public bool Success { get; set; } = true;

        public string Message { get; set; } = string.Empty;

        public T? Data { get; set; }

        public IDictionary<string, string[]>? Errors { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

}
