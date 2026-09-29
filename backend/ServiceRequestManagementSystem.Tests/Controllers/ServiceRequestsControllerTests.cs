using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServiceRequestManagementSystem.API.Controllers;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.ServiceRequests;
using ServiceRequestManagementSystem.API.Enums;
using ServiceRequestManagementSystem.API.Services.Interfaces;
using Xunit;

namespace ServiceRequestManagementSystem.Tests.Controllers
{
    public class ServiceRequestsControllerTests
    {
        private readonly Mock<IServiceRequestService> _serviceMock = new();

        [Fact]
        public async Task GetRequests_ShouldReturnOkWithRequestList()
        {
            // Arrange
            var list = new List<ServiceRequestResponseDto>
            {
                new() { RequestId = 1, RequestNumber = "SR-01", Title = "Ticket 1", Priority = Priority.High, Status = "Open" },
                new() { RequestId = 2, RequestNumber = "SR-02", Title = "Ticket 2", Priority = Priority.Low, Status = "Resolved" }
            };

            _serviceMock.Setup(s => s.GetRequestsAsync())
                .ReturnsAsync(new ApiResponseDto<IEnumerable<ServiceRequestResponseDto>>
                {
                    Success = true,
                    Message = "Service requests fetched successfully.",
                    Data = list
                });

            var controller = new ServiceRequestsController(_serviceMock.Object);

            // Act
            var result = await controller.GetRequests();

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var response = okResult.Value as ApiResponseDto<IEnumerable<ServiceRequestResponseDto>>;
            response!.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task CreateRequest_WhenValid_ShouldReturnCreatedAtAction()
        {
            // Arrange
            var createDto = new CreateServiceRequestDto { Title = "New Request", Description = "Testing creation" };
            var responseDto = new ServiceRequestResponseDto { RequestId = 10, RequestNumber = "SR-2026-000010", Title = "New Request" };

            _serviceMock.Setup(s => s.CreateRequestAsync(createDto, It.IsAny<string?>()))
                .ReturnsAsync(new ApiResponseDto<ServiceRequestResponseDto>
                {
                    Success = true,
                    Message = "Service request created successfully.",
                    Data = responseDto
                });

            var controller = new ServiceRequestsController(_serviceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            // Act
            var result = await controller.CreateRequest(createDto);

            // Assert
            var createdResult = result.Result as CreatedAtActionResult;
            createdResult.Should().NotBeNull();
            createdResult!.StatusCode.Should().Be(201);
            createdResult.ActionName.Should().Be(nameof(ServiceRequestsController.GetRequestById));
        }
    }
}
