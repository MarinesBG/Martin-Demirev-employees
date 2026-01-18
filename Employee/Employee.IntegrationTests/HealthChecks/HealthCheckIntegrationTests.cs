using FluentAssertions;
using System.Net;
using Xunit;

namespace Employee.IntegrationTests.HealthChecks
{
    public class HealthCheckIntegrationTests : IntegrationTestBase
    {
        public HealthCheckIntegrationTests(EmployeeApiFactory factory) : base(factory) { }

        [Fact]
        public async Task HealthCheck_General_ShouldReturnHealthy()
        {
            // Act
            var response = await Client.GetAsync("/health");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Be("Healthy");
        }

        [Fact]
        public async Task HealthCheck_Ready_ShouldReturnHealthy()
        {
            // Act
            var response = await Client.GetAsync("/health/ready");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Be("Healthy");
        }

        [Fact]
        public async Task HealthCheck_Live_ShouldReturnHealthy()
        {
            // Act
            var response = await Client.GetAsync("/health/live");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Be("Healthy");
        }

        [Theory]
        [InlineData("/health")]
        [InlineData("/health/ready")]
        [InlineData("/health/live")]
        public async Task HealthCheck_AllEndpoints_ShouldReturnSuccess(string endpoint)
        {
            // Act
            var response = await Client.GetAsync(endpoint);

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue();
        }
    }
}