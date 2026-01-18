using Employee.Services.Services;
using Employee.Services.Services.Interfaces;
using FluentAssertions;
using System.Text;
using Xunit;

namespace Employee.Tests.Services
{
    public class CsvParserServiceTests
    {
        private readonly ICsvParserService _csvParser;

        public CsvParserServiceTests()
        {
            _csvParser = new CsvParserService();
        }

        [Fact]
        public async Task ParseAsync_ValidCsv_ShouldParseAllRecords()
        {
            // Arrange
            var csvContent = @"EmpID,ProjectID,DateFrom,DateTo
                               1,10,2020-01-01,2020-12-31
                               2,10,2020-03-01,2020-11-30
                               3,15,2021-01-01,2021-06-30";

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

            // Act
            var result = await _csvParser.ParseAsync(stream);

            // Assert
            result.Should().HaveCount(3);

            var firstRecord = result.ElementAt(0);
            firstRecord.EmployeeID.Should().Be(1);
            firstRecord.ProjectID.Should().Be(10);
            firstRecord.DateFrom.Should().Be("2020-01-01");
            firstRecord.DateTo.Should().Be("2020-12-31");
        }

        [Fact]
        public async Task ParseAsync_EmptyFile_ShouldReturnEmptyCollection()
        {
            // Arrange
            var stream = new MemoryStream();

            // Act
            var result = await _csvParser.ParseAsync(stream);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task ParseAsync_HeaderOnly_ShouldReturnEmptyCollection()
        {
            // Arrange
            var csvContent = "EmpID,ProjectID,DateFrom,DateTo";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

            // Act
            var result = await _csvParser.ParseAsync(stream);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task ParseAsync_MalformedCsv_ShouldThrowException()
        {
            // Arrange
            var csvContent = @"EmpID,ProjectID,DateFrom,DateTo
                               1,2020-01-01,2020-01-01"; // Missing column

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() => _csvParser.ParseAsync(stream));
        }
    }
}