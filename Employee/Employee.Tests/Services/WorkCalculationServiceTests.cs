using Employee.Domain.Models;
using Employee.Services.Services;
using Employee.Services.Services.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Employee.Tests.Services
{
    public class WorkCalculationServiceTests
    {
        private readonly Mock<ICsvParserService> _mockCsvParser;
        private readonly Mock<IDateParserService> _mockDateParser;
        private readonly Mock<ILogger<WorkCalculationService>> _mockLogger;
        private readonly WorkCalculationService _service;

        public WorkCalculationServiceTests()
        {
            _mockCsvParser = new Mock<ICsvParserService>();
            _mockDateParser = new Mock<IDateParserService>();
            _mockLogger = new Mock<ILogger<WorkCalculationService>>();

            _service = new WorkCalculationService(
                _mockCsvParser.Object,
                _mockDateParser.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task CalculatePairsFromCsvAsync_EmptyFile_ShouldReturnEmptyResult()
        {
            // Arrange
            var emptyStream = new MemoryStream();
            _mockCsvParser.Setup(x => x.ParseAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EmployeeProjectRecord>());

            // Act
            var result = await _service.CalculatePairsFromCsvAsync(emptyStream);

            // Assert
            result.Should().NotBeNull();
            result.TopPair.Should().BeNull();
            result.AllPairs.Should().BeEmpty();
        }

        [Fact]
        public async Task CalculatePairsFromCsvAsync_TwoEmployeesOverlap_ShouldCalculateCorrectDays()
        {
            // Arrange
            var csvRecords = new List<EmployeeProjectRecord>
            {
                new() { EmployeeID = 1, ProjectID = 10, DateFrom = "2020-01-01", DateTo = "2020-12-31" },
                new() { EmployeeID = 2, ProjectID = 10, DateFrom = "2020-03-01", DateTo = "2020-11-30" }
            };

            _mockCsvParser.Setup(x => x.ParseAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(csvRecords);

            SetupDateParser("2020-01-01", new DateTime(2020, 1, 1));
            SetupDateParser("2020-12-31", new DateTime(2020, 12, 31));
            SetupDateParser("2020-03-01", new DateTime(2020, 3, 1));
            SetupDateParser("2020-11-30", new DateTime(2020, 11, 30));

            var stream = new MemoryStream();

            // Act
            var result = await _service.CalculatePairsFromCsvAsync(stream);

            // Assert
            result.Should().NotBeNull();
            result.TopPair.Should().NotBeNull();
            result.TopPair.EmployeeIdA.Should().Be(1);
            result.TopPair.EmployeeIdB.Should().Be(2);
            result.TopPair.TotalDays.Should().Be(275); // March 1 to Nov 30, 2020
            result.TopPair.Projects.Should().HaveCount(1);
            result.TopPair.Projects[0].ProjectId.Should().Be(10);
        }

        [Fact]
        public async Task CalculatePairsFromCsvAsync_MultipleProjects_ShouldSumTotalDays()
        {
            // Arrange
            var csvRecords = new List<EmployeeProjectRecord>
            {
                // Project 10: 100 days overlap
                new() { EmployeeID = 1, ProjectID = 10, DateFrom = "2020-01-01", DateTo = "2020-12-31" },
                new() { EmployeeID = 2, ProjectID = 10, DateFrom = "2020-01-01", DateTo = "2020-04-09" },
                
                // Project 15: 50 days overlap
                new() { EmployeeID = 1, ProjectID = 15, DateFrom = "2021-01-01", DateTo = "2021-12-31" },
                new() { EmployeeID = 2, ProjectID = 15, DateFrom = "2021-01-01", DateTo = "2021-02-19" }
            };

            _mockCsvParser.Setup(x => x.ParseAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(csvRecords);

            SetupDateParser("2020-01-01", new DateTime(2020, 1, 1));
            SetupDateParser("2020-12-31", new DateTime(2020, 12, 31));
            SetupDateParser("2020-04-09", new DateTime(2020, 4, 9));
            SetupDateParser("2021-01-01", new DateTime(2021, 1, 1));
            SetupDateParser("2021-12-31", new DateTime(2021, 12, 31));
            SetupDateParser("2021-02-19", new DateTime(2021, 2, 19));

            var stream = new MemoryStream();

            // Act
            var result = await _service.CalculatePairsFromCsvAsync(stream);

            // Assert
            result.Should().NotBeNull();
            result.TopPair.Should().NotBeNull();
            result.TopPair.TotalDays.Should().Be(150); // 100 + 50
            result.TopPair.Projects.Should().HaveCount(2);
        }

        [Fact]
        public async Task CalculatePairsFromCsvAsync_ThreeEmployees_ShouldReturnAllPairs()
        {
            // Arrange
            var csvRecords = new List<EmployeeProjectRecord>
            {
                new() { EmployeeID = 1, ProjectID = 10, DateFrom = "2020-01-01", DateTo = "2020-12-31" },
                new() { EmployeeID = 2, ProjectID = 10, DateFrom = "2020-01-01", DateTo = "2020-12-31" },
                new() { EmployeeID = 3, ProjectID = 10, DateFrom = "2020-01-01", DateTo = "2020-12-31" }
            };

            _mockCsvParser.Setup(x => x.ParseAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(csvRecords);

            SetupDateParser("2020-01-01", new DateTime(2020, 1, 1));
            SetupDateParser("2020-12-31", new DateTime(2020, 12, 31));

            var stream = new MemoryStream();

            // Act
            var result = await _service.CalculatePairsFromCsvAsync(stream);

            // Assert
            result.Should().NotBeNull();
            result.AllPairs.Should().HaveCount(3); // (1,2), (1,3), (2,3)
            result.AllPairs.Should().AllSatisfy(pair => pair.TotalDays.Should().Be(366)); // Leap year
        }

        [Fact]
        public async Task CalculatePairsFromCsvAsync_InvalidDateFormat_ShouldThrowException()
        {
            // Arrange
            var csvRecords = new List<EmployeeProjectRecord>
            {
                new() { EmployeeID = 1, ProjectID = 10, DateFrom = "invalid-date", DateTo = "2020-12-31" }
            };

            _mockCsvParser.Setup(x => x.ParseAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(csvRecords);

            // Setup mock to return false for invalid date
            _mockDateParser
                .Setup(x => x.TryParse("invalid-date", false, out It.Ref<DateTime>.IsAny))
                .Returns(new TryParseDelegate((string? input, bool treatNull, out DateTime output) =>
                {
                    output = default;
                    return false;
                }));

            var stream = new MemoryStream();

            // Act & Assert
            await Assert.ThrowsAsync<FormatException>(() =>
                _service.CalculatePairsFromCsvAsync(stream));
        }

        [Fact]
        public async Task CalculatePairsFromCsvAsync_ShouldOrderPairsByTotalDaysDescending()
        {
            // Arrange
            var csvRecords = new List<EmployeeProjectRecord>
            {
                // Pair 1-2: 100 days
                new() { EmployeeID = 1, ProjectID = 10, DateFrom = "2020-01-01", DateTo = "2020-04-09" },
                new() { EmployeeID = 2, ProjectID = 10, DateFrom = "2020-01-01", DateTo = "2020-04-09" },
                
                // Pair 3-4: 200 days (should be first)
                new() { EmployeeID = 3, ProjectID = 15, DateFrom = "2020-01-01", DateTo = "2020-07-18" },
                new() { EmployeeID = 4, ProjectID = 15, DateFrom = "2020-01-01", DateTo = "2020-07-18" }
            };

            _mockCsvParser.Setup(x => x.ParseAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(csvRecords);

            SetupDateParser("2020-01-01", new DateTime(2020, 1, 1));
            SetupDateParser("2020-04-09", new DateTime(2020, 4, 9));
            SetupDateParser("2020-07-18", new DateTime(2020, 7, 18));

            var stream = new MemoryStream();

            // Act
            var result = await _service.CalculatePairsFromCsvAsync(stream);

            // Assert
            result.TopPair.EmployeeIdA.Should().Be(3);
            result.TopPair.EmployeeIdB.Should().Be(4);
            result.TopPair.TotalDays.Should().Be(200);

            result.AllPairs[0].TotalDays.Should().BeGreaterThan(result.AllPairs[1].TotalDays);
        }

        // FIXED: Proper mock setup for out parameters
        private void SetupDateParser(string dateString, DateTime parsedDate)
        {
            _mockDateParser
                .Setup(x => x.TryParse(
                    It.Is<string?>(s => s == dateString),
                    It.IsAny<bool>(),
                    out It.Ref<DateTime>.IsAny))
                .Returns(new TryParseDelegate((string? input, bool treatNull, out DateTime output) =>
                {
                    output = parsedDate;
                    return true;
                }));
        }

        // Delegate for TryParse method signature
        private delegate bool TryParseDelegate(string? input, bool treatNull, out DateTime output);
    }
}