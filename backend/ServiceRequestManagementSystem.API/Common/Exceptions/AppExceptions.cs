namespace ServiceRequestManagementSystem.API.Common.Exceptions
{
    public abstract class AppException : Exception
    {
        public int StatusCode { get; }

        protected AppException(string message, int statusCode = 400) : base(message)
        {
            StatusCode = statusCode;
        }
    }

    public class NotFoundException : AppException
    {
        public NotFoundException(string message) : base(message, 404) { }
        public NotFoundException(string entityName, object key) : base($"{entityName} with key '{key}' was not found.", 404) { }
    }

    public class ConflictException : AppException
    {
        public ConflictException(string message) : base(message, 409) { }
    }

    public class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message = "Unauthorized access.") : base(message, 401) { }
    }

    public class ForbiddenException : AppException
    {
        public ForbiddenException(string message = "Forbidden. You do not have permission to access this resource.") : base(message, 403) { }
    }

    public class BusinessValidationException : AppException
    {
        public IDictionary<string, string[]>? Errors { get; }

        public BusinessValidationException(string message, IDictionary<string, string[]>? errors = null) : base(message, 400)
        {
            Errors = errors;
        }
    }
}
