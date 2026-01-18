using FluentAssertions;
using System.Net;
using Xunit;

namespace Employee.IntegrationTests.Api
{
    public class ApiEndpointsIntegrationTests : IntegrationTestBase
    {
        public ApiEndpointsIntegrationTests(EmployeeApiFactory factory) : base(factory) { }

        [Fact]
        public async Task Api_InvalidRoute_ShouldReturn404()
        {
            // Act
            var response = await Client.GetAsync("/api/invalid");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Api_UploadEndpoint_WithGetRequest_ShouldReturn405()
        {
            // Act
            var response = await Client.GetAsync("/api/employee/upload");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
        }

        [Fact]
        public async Task Api_UploadEndpoint_WithoutFile_ShouldReturn400()
        {
            // Arrange
            var content = new MultipartFormDataContent();

            // Act
            var response = await Client.PostAsync("/api/employee/upload", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Api_RootEndpoint_ShouldReturnRedirectOrNotFound()
        {
            // Act
            var response = await Client.GetAsync("/");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Redirect, HttpStatusCode.MovedPermanently);
        }
    }
}