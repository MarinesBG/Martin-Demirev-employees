using AutoMapper;
using Employee.API.Controllers;
using Employee.Contracts.Models;
using Employee.Domain.Models;
using Employee.Mapping;
using Employee.Services.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using System.Text;
using Xunit;

namespace Employee.Tests.Controllers
{
    public class EmployeeControllerTests
    {
        private readonly Mock<ILogger<EmployeeController>> _mockLogger;
        private readonly Mock<IWorkCalculationService> _mockService;
        private readonly IMapper _mapper;
        private readonly EmployeeController _controller;

        public EmployeeControllerTests()
        {
            _mockLogger = new Mock<ILogger<EmployeeController>>();
            _mockService = new Mock<IWorkCalculationService>();
            
            // Setup real AutoMapper for testing
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();
            
            _controller = new EmployeeController(_mockLogger.Object, _mockService.Object, _mapper);
            
            // Setup default HTTP context with headers
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Fact]
        public async Task UploadCsv_NullFile_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.UploadCsv(null!, CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UploadCsv_EmptyFile_ShouldReturnBadRequest()
        {
            // Arrange
            var mockFile = CreateMockFile("test.csv", "");

            // Act
            var result = await _controller.UploadCsv(mockFile.Object, CancellationToken.None);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeEquivalentTo(new { error = "No file uploaded" });
        }

        [Fact]
        public async Task UploadCsv_NonCsvFile_ShouldReturnBadRequest()
        {
            // Arrange
            var mockFile = CreateMockFile("test.txt", "some content");

            // Act
            var result = await _controller.UploadCsv(mockFile.Object, CancellationToken.None);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeEquivalentTo(new { error = "Only CSV files are allowed" });
        }

        [Fact]
        public async Task UploadCsv_ValidFile_WithoutHeader_ShouldReturnSummary()
        {
            // Arrange
            var csvContent = "EmpID,ProjectID,DateFrom,DateTo\n1,10,2020-01-01,2020-12-31";
            var mockFile = CreateMockFile("test.csv", csvContent);

            var calculationResult = new PairCalculationResult
            {
                TopPair = new PairResult
                {
                    EmployeeIdA = 1,
                    EmployeeIdB = 2,
                    TotalDays = 100,
                    Projects = new List<ProjectDetail>
                    {
                        new() { EmployeeId1 = 1, EmployeeId2 = 2, ProjectId = 10, DaysWorked = 100 }
                    }
                },
                AllPairs = new List<PairResult>
                {
                    new() { EmployeeIdA = 1, EmployeeIdB = 2, TotalDays = 100, Projects = new List<ProjectDetail>() }
                }
            };

            _mockService.Setup(x => x.CalculatePairsFromCsvAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(calculationResult);

            // Act
            var result = await _controller.UploadCsv(mockFile.Object, CancellationToken.None);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(new
            {
                EmployeeIdA = 1,
                EmployeeIdB = 2,
                TotalDays = 100
            });
        }

        [Fact]
        public async Task UploadCsv_NoPairsFound_ShouldReturnNoResultsMessage()
        {
            // Arrange
            var csvContent = "EmpID,ProjectID,DateFrom,DateTo\n1,10,2020-01-01,2020-12-31";
            var mockFile = CreateMockFile("test.csv", csvContent);

            var calculationResult = new PairCalculationResult
            {
                TopPair = null!,
                AllPairs = new List<PairResult>()
            };

            _mockService.Setup(x => x.CalculatePairsFromCsvAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(calculationResult);

            // Act
            var result = await _controller.UploadCsv(mockFile.Object, CancellationToken.None);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(new
            {
                message = "No employee pairs found. Employees must work on the same project with overlapping dates.",
                topPair = (object?)null,
                allPairs = Array.Empty<object>(),
                totalPairsFound = 0
            });
        }

        [Fact]
        public async Task UploadCsv_FormatException_ShouldReturnBadRequest()
        {
            // Arrange
            var csvContent = "EmpID,ProjectID,DateFrom,DateTo\n1,10,invalid-date,2020-12-31";
            var mockFile = CreateMockFile("test.csv", csvContent);

            _mockService.Setup(x => x.CalculatePairsFromCsvAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new FormatException("Invalid date format"));

            // Act
            var result = await _controller.UploadCsv(mockFile.Object, CancellationToken.None);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task UploadCsv_InvalidOperationException_ShouldReturnBadRequest()
        {
            // Arrange
            var csvContent = "EmpID,ProjectID,DateFrom,DateTo\n1,10,2020-01-01,2020-12-31";
            var mockFile = CreateMockFile("test.csv", csvContent);

            _mockService.Setup(x => x.CalculatePairsFromCsvAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Invalid data range"));

            // Act
            var result = await _controller.UploadCsv(mockFile.Object, CancellationToken.None);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task UploadCsv_OperationCanceledException_ShouldReturn499()
        {
            // Arrange
            var csvContent = "EmpID,ProjectID,DateFrom,DateTo\n1,10,2020-01-01,2020-12-31";
            var mockFile = CreateMockFile("test.csv", csvContent);

            _mockService.Setup(x => x.CalculatePairsFromCsvAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            // Act
            var result = await _controller.UploadCsv(mockFile.Object, CancellationToken.None);

            // Assert
            var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status499ClientClosedRequest);
        }

        [Fact]
        public async Task UploadCsv_UnexpectedException_ShouldReturn500()
        {
            // Arrange
            var csvContent = "EmpID,ProjectID,DateFrom,DateTo\n1,10,2020-01-01,2020-12-31";
            var mockFile = CreateMockFile("test.csv", csvContent);

            _mockService.Setup(x => x.CalculatePairsFromCsvAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.UploadCsv(mockFile.Object, CancellationToken.None);

            // Assert
            var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(500);
        }

        // Helper method to create mock IFormFile
        private Mock<IFormFile> CreateMockFile(string fileName, string content)
        {
            var mockFile = new Mock<IFormFile>();
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));

            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.Length).Returns(ms.Length);
            mockFile.Setup(f => f.OpenReadStream()).Returns(ms);

            return mockFile;
        }
    }
}