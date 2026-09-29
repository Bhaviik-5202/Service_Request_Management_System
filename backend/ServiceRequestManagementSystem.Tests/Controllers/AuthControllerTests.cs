using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServiceRequestManagementSystem.API.Controllers;
using ServiceRequestManagementSystem.API.DTOs.Auth;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.Services.Interfaces;
using Xunit;

namespace ServiceRequestManagementSystem.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock = new();

        [Fact]
        public async Task Login_WhenSuccessful_ShouldReturnOkWithAuthResponse()
        {
            // Arrange
            var loginRequest = new LoginRequest { Email = "admin@test.com", Password = "Password123" };
            var authResponse = new AuthResponse { Token = "mock.jwt.token", Email = "admin@test.com", Role = "Admin" };

            _authServiceMock.Setup(s => s.LoginAsync(loginRequest, It.IsAny<string?>()))
                .ReturnsAsync(new ApiResponseDto<AuthResponse>
                {
                    Success = true,
                    Message = "Login successful.",
                    Data = authResponse
                });

            var controller = new AuthController(_authServiceMock.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await controller.Login(loginRequest);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var apiResponse = okResult.Value as ApiResponseDto<AuthResponse>;
            apiResponse.Should().NotBeNull();
            apiResponse!.Success.Should().BeTrue();
            apiResponse.Data!.Token.Should().Be("mock.jwt.token");
        }

        [Fact]
        public async Task Login_WhenUnauthorized_ShouldReturnUnauthorized()
        {
            // Arrange
            var loginRequest = new LoginRequest { Email = "wrong@test.com", Password = "WrongPassword" };

            _authServiceMock.Setup(s => s.LoginAsync(loginRequest, It.IsAny<string?>()))
                .ReturnsAsync(new ApiResponseDto<AuthResponse>
                {
                    Success = false,
                    Message = "Invalid credentials or inactive account.",
                    Data = null
                });

            var controller = new AuthController(_authServiceMock.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await controller.Login(loginRequest);

            // Assert
            var unauthResult = result.Result as UnauthorizedObjectResult;
            unauthResult.Should().NotBeNull();
            unauthResult!.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task Me_WhenAuthenticated_ShouldReturnCurrentUser()
        {
            // Arrange
            var userMockData = new { UserId = 42, FullName = "Current User", Role = "Admin" };

            _authServiceMock.Setup(s => s.MeAsync(42))
                .ReturnsAsync(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Current user retrieved successfully.",
                    Data = userMockData
                });

            var controller = new AuthController(_authServiceMock.Object);
            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("userId", "42"),
                new Claim(ClaimTypes.Role, "Admin")
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            // Act
            var result = await controller.Me();

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);
        }
    }
}
