using Employee.Contracts.Models;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Employee.IntegrationTests.Controllers
{
    public class EmployeeControllerIntegrationTests : IntegrationTestBase
    {
        public EmployeeControllerIntegrationTests(EmployeeApiFactory factory) : base(factory) { }

        [Fact]
        public async Task UploadCsv_WithValidData_ShouldReturn200AndPairResult()
        {
            // Arrange
            var csvContent = @"EmpID,ProjectID,DateFrom,DateTo
1,10,2020-01-01,2020-12-31
2,10,2020-03-01,2020-11-30";

            var content = CreateCsvFileContent(csvContent);

            // Act
            var response = await Client.PostAsync("/api/employee/upload", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<PairResultViewModel>();
            result.Should().NotBeNull();
            result!.EmployeeIdA.Should().Be(1);
            result.EmployeeIdB.Should().Be(2);
            result.TotalDays.Should().Be(275); // March 1 to Nov 30, 2020
            result.Projects.Should().HaveCount(1);
        }

        [Fact]
        public async Task UploadCsv_WithMultipleProjects_ShouldCalculateTotalDays()
        {
            // Arrange
            var csvContent = @"EmpID,ProjectID,DateFrom,DateTo
1,10,2020-01-01,2020-04-09
2,10,2020-01-01,2020-04-09
1,15,2021-01-01,2021-02-19
2,15,2021-01-01,2021-02-19";

            var content = CreateCsvFileContent(csvContent);

            // Act
            var response = await Client.PostAsync("/api/employee/upload", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<PairResultViewModel>();
            result.Should().NotBeNull();
            result!.TotalDays.Should().Be(150); // 100 + 50
            result.Projects.Should().HaveCount(2);
        }

        [Fact]
        public async Task UploadCsv_WithNoOverlap_ShouldReturnNoResultsMessage()
        {
            // Arrange
            var csvContent = @"EmpID,ProjectID,DateFrom,DateTo
1,10,2020-01-01,2020-06-30
2,10,2020-07-01,2020-12-31";

            var content = CreateCsvFileContent(csvContent);

            // Act
            var response = await Client.PostAsync("/api/employee/upload", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            responseContent.Should().Contain("No employee pairs found");
        }

        [Fact]
        public async Task UploadCsv_WithNullDates_ShouldUseTodayAsDefault()
        {
            // Arrange
            var csvContent = @"EmpID,ProjectID,DateFrom,DateTo
1,10,2020-01-01,NULL
2,10,2020-01-01,NULL";

            var content = CreateCsvFileContent(csvContent);

            // Act
            var response = await Client.PostAsync("/api/employee/upload", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<PairResultViewModel>();
            result.Should().NotBeNull();
            result!.EmployeeIdA.Should().Be(1);
            result.EmployeeIdB.Should().Be(2);
        }

        [Fact]
        public async Task UploadCsv_WithInvalidDateFormat_ShouldReturn400()
        {
            // Arrange
            var csvContent = @"EmpID,ProjectID,DateFrom,DateTo
1,10,invalid-date,2020-12-31";

            var content = CreateCsvFileContent(csvContent);

            // Act
            var response = await Client.PostAsync("/api/employee/upload", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().Contain("Invalid CSV format");
        }

        [Fact]
        public async Task UploadCsv_WithNonCsvFile_ShouldReturn400()
        {
            // Arrange
            var content = CreateCsvFileContent("some text content", "test.txt");

            // Act
            var response = await Client.PostAsync("/api/employee/upload", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().Contain("Only CSV files are allowed");
        }

        [Fact]
        public async Task UploadCsv_WithEmptyFile_ShouldReturn400()
        {
            // Arrange
            var content = CreateCsvFileContent("");

            // Act
            var response = await Client.PostAsync("/api/employee/upload", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UploadCsv_WithMalformedCsv_ShouldReturn400()
        {
            // Arrange
            var csvContent = @"EmpID,ProjectID,DateFrom,DateTo
1,20"; // Missing column

            var content = CreateCsvFileContent(csvContent);

            // Act
            var response = await Client.PostAsync("/api/employee/upload", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UploadCsv_WithThreeEmployees_ShouldReturnAllPairs()
        {
            // Arrange
            var csvContent = @"EmpID,ProjectID,DateFrom,DateTo
1,10,2020-01-01,2020-12-31
2,10,2020-01-01,2020-12-31
3,10,2020-01-01,2020-12-31";

            var content = CreateCsvFileContent(csvContent);

            // Act
            var response = await Client.PostAsync("/api/employee/upload", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<PairResultViewModel>();
            result.Should().NotBeNull();
            result!.TotalDays.Should().Be(366); // 2020 is a leap year
        }

        [Fact]
        public async Task UploadCsv_WithLargeFile_ShouldProcessSuccessfully()
        {
            // Arrange - Generate CSV with 100 employees and 10 projects
            var csvLines = new List<string> { "EmpID,ProjectID,DateFrom,DateTo" };
            for (int emp = 1; emp <= 100; emp++)
            {
                for (int proj = 1; proj <= 10; proj++)
                {
                    csvLines.Add($"{emp},{proj},2020-01-01,2020-12-31");
                }
            }
            var csvContent = string.Join("\n", csvLines);

            var content = CreateCsvFileContent(csvContent);

            // Act
            var response = await Client.PostAsync("/api/employee/upload", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task UploadCsv_WithDifferentDateFormats_ShouldParseCorrectly()
        {
            // Arrange
            var csvContent = @"EmpID,ProjectID,DateFrom,DateTo
1,10,2020-01-01,2020-12-31
2,10,01/03/2020,30/11/2020";

            var content = CreateCsvFileContent(csvContent);

            // Act
            var response = await Client.PostAsync("/api/employee/upload", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<PairResultViewModel>();
            result.Should().NotBeNull();
        }
    }
}