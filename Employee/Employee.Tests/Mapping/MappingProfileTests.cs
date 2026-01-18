using AutoMapper;
using Employee.Contracts.Models;
using Employee.Domain.Models;
using Employee.Mapping;
using FluentAssertions;
using Xunit;

namespace Employee.Tests.Mapping
{
    public class MappingProfileTests
    {
        private readonly IMapper _mapper;
        private readonly IConfigurationProvider _configuration;

        public MappingProfileTests()
        {
            _configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            _mapper = _configuration.CreateMapper();
        }

        [Fact]
        public void AutoMapper_Configuration_ShouldBeValid()
        {
            // Assert - This will throw if configuration is invalid
            _configuration.AssertConfigurationIsValid();
        }

        [Fact]
        public void PairResult_ShouldMapTo_PairResultViewModel()
        {
            // Arrange
            var source = new PairResult
            {
                EmployeeIdA = 1,
                EmployeeIdB = 2,
                TotalDays = 366,
                Projects = new List<ProjectDetail>
                {
                    new() { EmployeeId1 = 1, EmployeeId2 = 2, ProjectId = 10, DaysWorked = 200 },
                    new() { EmployeeId1 = 1, EmployeeId2 = 2, ProjectId = 15, DaysWorked = 166 }
                }
            };

            // Act
            var result = _mapper.Map<PairResultViewModel>(source);

            // Assert
            result.Should().NotBeNull();
            result.EmployeeIdA.Should().Be(1);
            result.EmployeeIdB.Should().Be(2);
            result.TotalDays.Should().Be(366);
            result.Projects.Should().HaveCount(2);
        }

        [Fact]
        public void ProjectDetail_ShouldMapTo_PairProjectViewModel()
        {
            // Arrange
            var source = new ProjectDetail
            {
                EmployeeId1 = 1,
                EmployeeId2 = 2,
                ProjectId = 10,
                DaysWorked = 100,
                StartDate = new DateTime(2020, 1, 1),
                EndDate = new DateTime(2020, 4, 9)
            };

            // Act
            var result = _mapper.Map<PairProjectViewModel>(source);

            // Assert
            result.Should().NotBeNull();
            result.EmployeeId1.Should().Be(1);
            result.EmployeeId2.Should().Be(2);
            result.ProjectId.Should().Be(10);
            result.DaysWorked.Should().Be(100);
        }

        [Fact]
        public void PairResult_WithEmptyProjects_ShouldMapCorrectly()
        {
            // Arrange
            var source = new PairResult
            {
                EmployeeIdA = 5,
                EmployeeIdB = 10,
                TotalDays = 0,
                Projects = new List<ProjectDetail>()
            };

            // Act
            var result = _mapper.Map<PairResultViewModel>(source);

            // Assert
            result.Should().NotBeNull();
            result.EmployeeIdA.Should().Be(5);
            result.EmployeeIdB.Should().Be(10);
            result.TotalDays.Should().Be(0);
            result.Projects.Should().BeEmpty();
        }

        [Fact]
        public void PairResult_WithMultipleProjects_ShouldMapAllProjects()
        {
            // Arrange
            var source = new PairResult
            {
                EmployeeIdA = 1,
                EmployeeIdB = 2,
                TotalDays = 300,
                Projects = new List<ProjectDetail>
                {
                    new() { EmployeeId1 = 1, EmployeeId2 = 2, ProjectId = 10, DaysWorked = 100 },
                    new() { EmployeeId1 = 1, EmployeeId2 = 2, ProjectId = 15, DaysWorked = 100 },
                    new() { EmployeeId1 = 1, EmployeeId2 = 2, ProjectId = 20, DaysWorked = 100 }
                }
            };

            // Act
            var result = _mapper.Map<PairResultViewModel>(source);

            // Assert
            result.Projects.Should().HaveCount(3);
            result.Projects.Should().AllSatisfy(p => p.DaysWorked.Should().Be(100));
        }

        [Fact]
        public void MappingProfile_ShouldPreserveEmployeeIdOrder()
        {
            // Arrange
            var source = new PairResult
            {
                EmployeeIdA = 5,
                EmployeeIdB = 3, // B is less than A
                TotalDays = 100,
                Projects = new List<ProjectDetail>()
            };

            // Act
            var result = _mapper.Map<PairResultViewModel>(source);

            // Assert
            result.EmployeeIdA.Should().Be(5);
            result.EmployeeIdB.Should().Be(3);
        }
    }
}