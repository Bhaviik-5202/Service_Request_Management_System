using System.Diagnostics;

namespace ServiceRequestManagementSystem.API.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Correlation ID tracking
            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = Guid.NewGuid().ToString("N")[..12];
            }

            context.Items["CorrelationId"] = correlationId;
            context.Response.Headers["X-Correlation-ID"] = correlationId;

            var stopwatch = Stopwatch.StartNew();
            var method = context.Request.Method;
            var path = context.Request.Path;
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            _logger.LogInformation("[{CorrelationId}] HTTP {Method} {Path} initiated from {IpAddress}", correlationId, method, path, ipAddress);

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                var statusCode = context.Response.StatusCode;
                var elapsedMs = stopwatch.ElapsedMilliseconds;

                if (statusCode >= 500)
                {
                    _logger.LogError("[{CorrelationId}] HTTP {Method} {Path} completed with status {StatusCode} in {ElapsedMs}ms", correlationId, method, path, statusCode, elapsedMs);
                }
                else if (statusCode >= 400)
                {
                    _logger.LogWarning("[{CorrelationId}] HTTP {Method} {Path} completed with status {StatusCode} in {ElapsedMs}ms", correlationId, method, path, statusCode, elapsedMs);
                }
                else
                {
                    _logger.LogInformation("[{CorrelationId}] HTTP {Method} {Path} completed with status {StatusCode} in {ElapsedMs}ms", correlationId, method, path, statusCode, elapsedMs);
                }
            }
        }
    }
}
