using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace ServiceRequestManagementSystem.Tests.Helpers
{
    public class HealthCheckTests
    {
        [Fact]
        public async Task HealthCheckService_WhenRegistered_ReturnsHealthyStatus()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHealthChecks();
            var serviceProvider = services.BuildServiceProvider();

            var healthCheckService = serviceProvider.GetRequiredService<HealthCheckService>();

            // Act
            var report = await healthCheckService.CheckHealthAsync();

            // Assert
            report.Should().NotBeNull();
            report.Status.Should().Be(HealthStatus.Healthy);
        }
    }
}
