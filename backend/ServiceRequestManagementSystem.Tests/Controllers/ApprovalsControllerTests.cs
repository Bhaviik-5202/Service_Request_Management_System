using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServiceRequestManagementSystem.API.Controllers;
using ServiceRequestManagementSystem.API.DTOs.Approvals;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.Enums;
using ServiceRequestManagementSystem.API.Services.Interfaces;
using Xunit;

namespace ServiceRequestManagementSystem.Tests.Controllers
{
    public class ApprovalsControllerTests
    {
        private readonly Mock<IApprovalService> _serviceMock = new();

        [Fact]
        public async Task MakeDecision_WhenSuccess_ShouldReturnOk()
        {
            // Arrange
            var decisionDto = new ApprovalDecisionDto
            {
                Decision = ApprovalStatus.Approved,
                DecidedByUserId = 5,
                Remarks = "Looks good"
            };

            _serviceMock.Setup(s => s.MakeDecisionAsync(1, decisionDto, It.IsAny<string?>()))
                .ReturnsAsync(new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Approval approved successfully.",
                    Data = true
                });

            var controller = new ApprovalsController(_serviceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            // Act
            var result = await controller.MakeDecision(1, decisionDto);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var apiResponse = okResult.Value as ApiResponseDto<bool>;
            apiResponse!.Success.Should().BeTrue();
            apiResponse.Data.Should().BeTrue();
        }

        [Fact]
        public async Task MakeDecision_WhenNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var decisionDto = new ApprovalDecisionDto
            {
                Decision = ApprovalStatus.Approved,
                DecidedByUserId = 5
            };

            _serviceMock.Setup(s => s.MakeDecisionAsync(999, decisionDto, It.IsAny<string?>()))
                .ReturnsAsync(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Approval not found.",
                    Data = false
                });

            var controller = new ApprovalsController(_serviceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            // Act
            var result = await controller.MakeDecision(999, decisionDto);

            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult!.StatusCode.Should().Be(404);
        }
    }
}
