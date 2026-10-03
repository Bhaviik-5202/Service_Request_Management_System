using System.Net;
using System.Text.Json;
using ServiceRequestManagementSystem.API.Common.Exceptions;
using ServiceRequestManagementSystem.API.DTOs.Common;

namespace ServiceRequestManagementSystem.API.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString("N");
            var statusCode = (int)HttpStatusCode.InternalServerError;
            var message = "An unexpected internal server error occurred.";
            IDictionary<string, string[]>? errors = null;

            switch (exception)
            {
                case AppException appEx:
                    statusCode = appEx.StatusCode;
                    message = appEx.Message;
                    if (appEx is BusinessValidationException bve && bve.Errors != null)
                    {
                        errors = bve.Errors;
                    }
                    _logger.LogWarning(exception, "[{CorrelationId}] Application Exception ({StatusCode}): {Message}", correlationId, statusCode, message);
                    break;

                case FluentValidation.ValidationException valEx:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    message = "Validation failed.";
                    errors = valEx.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                    _logger.LogWarning(exception, "[{CorrelationId}] Validation Exception: {Message}", correlationId, message);
                    break;

                case UnauthorizedAccessException:
                    statusCode = (int)HttpStatusCode.Unauthorized;
                    message = "Unauthorized access.";
                    _logger.LogWarning(exception, "[{CorrelationId}] Unauthorized Access", correlationId);
                    break;

                case KeyNotFoundException:
                    statusCode = (int)HttpStatusCode.NotFound;
                    message = "Requested resource was not found.";
                    _logger.LogWarning(exception, "[{CorrelationId}] Resource Not Found", correlationId);
                    break;

                default:
                    _logger.LogError(exception, "[{CorrelationId}] Unhandled Exception: {Message}", correlationId, exception.Message);
                    if (_env.IsDevelopment())
                    {
                        message = exception.Message;
                    }
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new ApiResponseDto<object>
            {
                Success = false,
                Message = message,
                Errors = errors,
                Data = null,
                Timestamp = DateTime.UtcNow
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }
}
