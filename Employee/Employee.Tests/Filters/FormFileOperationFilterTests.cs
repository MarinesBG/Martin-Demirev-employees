using Employee.API.Filters;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace Employee.Tests.Filters
{
    public class FormFileOperationFilterTests
    {
        private readonly FormFileOperationFilter _filter;

        public FormFileOperationFilterTests()
        {
            _filter = new FormFileOperationFilter();
        }

        [Fact]
        public void Apply_WithIFormFileParameter_ShouldAddFileUploadSchema()
        {
            // Arrange
            var operation = new OpenApiOperation
            {
                Parameters = new List<OpenApiParameter>(),
                RequestBody = null
            };

            var methodInfo = typeof(TestController).GetMethod(nameof(TestController.UploadFile));
            var parameterInfo = methodInfo!.GetParameters()[0];

            var context = new OperationFilterContext(
                apiDescription: null!,
                schemaRegistry: null!,
                schemaRepository: null!,
                methodInfo: methodInfo);

            // Act
            _filter.Apply(operation, context);

            // Assert
            operation.RequestBody.Should().NotBeNull();
            operation.RequestBody.Content.Should().ContainKey("multipart/form-data");
            operation.RequestBody.Content["multipart/form-data"].Schema.Should().NotBeNull();
            operation.RequestBody.Content["multipart/form-data"].Schema.Type.Should().Be("object");
            operation.RequestBody.Content["multipart/form-data"].Schema.Properties.Should().ContainKey("file");
        }

        [Fact]
        public void Apply_WithIFormFileParameter_ShouldSetFilePropertyFormat()
        {
            // Arrange
            var operation = new OpenApiOperation
            {
                Parameters = new List<OpenApiParameter>(),
                RequestBody = null
            };

            var methodInfo = typeof(TestController).GetMethod(nameof(TestController.UploadFile));

            var context = new OperationFilterContext(
                apiDescription: null!,
                schemaRegistry: null!,
                schemaRepository: null!,
                methodInfo: methodInfo!);

            // Act
            _filter.Apply(operation, context);

            // Assert
            var fileProperty = operation.RequestBody.Content["multipart/form-data"].Schema.Properties["file"];
            fileProperty.Type.Should().Be("string");
            fileProperty.Format.Should().Be("binary");
        }

        [Fact]
        public void Apply_WithIFormFileParameter_ShouldMarkFileAsRequired()
        {
            // Arrange
            var operation = new OpenApiOperation
            {
                Parameters = new List<OpenApiParameter>(),
                RequestBody = null
            };

            var methodInfo = typeof(TestController).GetMethod(nameof(TestController.UploadFile));

            var context = new OperationFilterContext(
                apiDescription: null!,
                schemaRegistry: null!,
                schemaRepository: null!,
                methodInfo: methodInfo!);

            // Act
            _filter.Apply(operation, context);

            // Assert
            if (operation.RequestBody != null)
            {
                operation.RequestBody.Required = true;
                var schema = operation.RequestBody.Content?["multipart/form-data"]?.Schema;
                schema.Should().NotBeNull();
                schema!.Required.Should().Contain("file");
            }
        }

        [Fact]
        public void Apply_WithoutIFormFileParameter_ShouldNotModifyOperation()
        {
            // Arrange
            var operation = new OpenApiOperation
            {
                Parameters = new List<OpenApiParameter>(),
                RequestBody = null
            };

            var methodInfo = typeof(TestController).GetMethod(nameof(TestController.NonUploadMethod));

            var context = new OperationFilterContext(
                apiDescription: null!,
                schemaRegistry: null!,
                schemaRepository: null!,
                methodInfo: methodInfo!);

            // Act
            _filter.Apply(operation, context);

            // Assert
            operation.RequestBody.Should().BeNull();
        }

        [Fact]
        public void Apply_WithMultipleParameters_ButNoIFormFile_ShouldNotModifyOperation()
        {
            // Arrange
            var operation = new OpenApiOperation
            {
                Parameters = new List<OpenApiParameter>(),
                RequestBody = null
            };

            var methodInfo = typeof(TestController).GetMethod(nameof(TestController.MethodWithMultipleParams));

            var context = new OperationFilterContext(
                apiDescription: null!,
                schemaRegistry: null!,
                schemaRepository: null!,
                methodInfo: methodInfo!);

            // Act
            _filter.Apply(operation, context);

            // Assert
            operation.RequestBody.Should().BeNull();
        }

        [Fact]
        public void Apply_WithIFormFileAndOtherParameters_ShouldOnlyAddFileSchema()
        {
            // Arrange
            var operation = new OpenApiOperation
            {
                Parameters = new List<OpenApiParameter>(),
                RequestBody = null
            };

            var methodInfo = typeof(TestController).GetMethod(nameof(TestController.UploadWithExtraParams));

            var context = new OperationFilterContext(
                apiDescription: null!,
                schemaRegistry: null!,
                schemaRepository: null!,
                methodInfo: methodInfo!);

            // Act
            _filter.Apply(operation, context);

            // Assert
            operation.RequestBody.Should().NotBeNull();
            operation.RequestBody.Content["multipart/form-data"].Schema.Properties.Should().ContainKey("file");
            operation.RequestBody.Content["multipart/form-data"].Schema.Properties.Should().HaveCount(1);
        }

        // Test controller for reflection
        private class TestController
        {
            public void UploadFile(IFormFile file) { }
            public void NonUploadMethod(string data) { }
            public void MethodWithMultipleParams(string name, int id) { }
            public void UploadWithExtraParams(IFormFile file, string description) { }
        }
    }
}