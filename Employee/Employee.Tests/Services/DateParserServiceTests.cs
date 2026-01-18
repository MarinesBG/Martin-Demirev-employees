using Employee.Services.Services;
using Employee.Services.Services.Interfaces;
using FluentAssertions;
using Xunit;

namespace Employee.Tests.Services
{
    public class DateParserServiceTests
    {
        private readonly IDateParserService _dateParser;

        public DateParserServiceTests()
        {
            _dateParser = new DateParserService();
        }

        [Theory]
        [InlineData("2020-01-01", 2020, 1, 1)]
        [InlineData("2021/06/30", 2021, 6, 30)]
        [InlineData("31-12-2020", 2020, 12, 31)]
        [InlineData("31/12/2020", 2020, 12, 31)]
        [InlineData("12/31/2020", 2020, 12, 31)]
        public void TryParse_ValidDateFormats_ShouldReturnTrue(string dateString, int expectedYear, int expectedMonth, int expectedDay)
        {
            // Act
            var result = _dateParser.TryParse(dateString, treatNullAsToday: false, out DateTime parsedDate);

            // Assert
            result.Should().BeTrue();
            parsedDate.Year.Should().Be(expectedYear);
            parsedDate.Month.Should().Be(expectedMonth);
            parsedDate.Day.Should().Be(expectedDay);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("NULL")]
        [InlineData("null")]
        public void TryParse_NullValues_WithTreatNullAsToday_ShouldReturnToday(string? dateString)
        {
            // Act
            var result = _dateParser.TryParse(dateString, treatNullAsToday: true, out DateTime parsedDate);

            // Assert
            result.Should().BeTrue();
            parsedDate.Date.Should().Be(DateTime.Today);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("NULL")]
        public void TryParse_NullValues_WithoutTreatNullAsToday_ShouldReturnFalse(string? dateString)
        {
            // Act
            var result = _dateParser.TryParse(dateString, treatNullAsToday: false, out DateTime parsedDate);

            // Assert
            result.Should().BeFalse();
            parsedDate.Should().Be(default(DateTime));
        }

        [Theory]
        [InlineData("invalid-date")]
        [InlineData("2020-13-01")] // Invalid month
        [InlineData("2020-01-32")] // Invalid day
        [InlineData("not a date")]
        public void TryParse_InvalidDateFormats_ShouldReturnFalse(string dateString)
        {
            // Act
            var result = _dateParser.TryParse(dateString, treatNullAsToday: false, out DateTime parsedDate);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void TryParse_DateWithTime_ShouldParse()
        {
            // Arrange
            var dateString = "2020-01-01 14:30:00";

            // Act
            var result = _dateParser.TryParse(dateString, treatNullAsToday: false, out DateTime parsedDate);

            // Assert
            result.Should().BeTrue();
            parsedDate.Year.Should().Be(2020);
            parsedDate.Month.Should().Be(1);
            parsedDate.Day.Should().Be(1);
            parsedDate.Hour.Should().Be(14);
            parsedDate.Minute.Should().Be(30);
        }
    }
}