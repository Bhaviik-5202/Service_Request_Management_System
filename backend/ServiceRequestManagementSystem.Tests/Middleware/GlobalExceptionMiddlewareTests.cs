using System.IO;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ServiceRequestManagementSystem.API.Common.Exceptions;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.Middleware;
using Xunit;

namespace ServiceRequestManagementSystem.Tests.Middleware
{
    public class GlobalExceptionMiddlewareTests
    {
        private readonly Mock<IHostEnvironment> _envMock = new();

        [Fact]
        public async Task InvokeAsync_WhenNotFoundExceptionThrown_ShouldReturn404JsonResponse()
        {
            // Arrange
            _envMock.Setup(e => e.EnvironmentName).Returns(Environments.Production);
            var middleware = new GlobalExceptionMiddleware(
                _ => throw new NotFoundException("User not found"),
                NullLogger<GlobalExceptionMiddleware>.Instance,
                _envMock.Object);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.Should().Be(404);
            context.Response.ContentType.Should().StartWith("application/json");

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body);
            var json = await reader.ReadToEndAsync();
            var result = JsonSerializer.Deserialize<ApiResponseDto<object>>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            result.Should().NotBeNull();
            result!.Success.Should().BeFalse();
            result.Message.Should().Be("User not found");
        }

        [Fact]
        public async Task InvokeAsync_WhenConflictExceptionThrown_ShouldReturn409JsonResponse()
        {
            // Arrange
            _envMock.Setup(e => e.EnvironmentName).Returns(Environments.Production);
            var middleware = new GlobalExceptionMiddleware(
                _ => throw new ConflictException("Email already in use"),
                NullLogger<GlobalExceptionMiddleware>.Instance,
                _envMock.Object);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.Should().Be(409);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body);
            var json = await reader.ReadToEndAsync();
            var result = JsonSerializer.Deserialize<ApiResponseDto<object>>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            result!.Success.Should().BeFalse();
            result.Message.Should().Be("Email already in use");
        }

        [Fact]
        public async Task InvokeAsync_WhenUnhandledExceptionThrown_ShouldReturn500JsonResponse()
        {
            // Arrange
            _envMock.Setup(e => e.EnvironmentName).Returns(Environments.Production);
            var middleware = new GlobalExceptionMiddleware(
                _ => throw new InvalidOperationException("Fatal database crash"),
                NullLogger<GlobalExceptionMiddleware>.Instance,
                _envMock.Object);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.Should().Be(500);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body);
            var json = await reader.ReadToEndAsync();
            var result = JsonSerializer.Deserialize<ApiResponseDto<object>>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            result!.Success.Should().BeFalse();
            result.Message.Should().Be("An unexpected internal server error occurred.");
        }
    }
}
