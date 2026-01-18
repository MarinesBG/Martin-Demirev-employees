using Employee.Domain.Models;
using Employee.Services.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Employee.Services.Services
{
    public class WorkCalculationService : IWorkCalculationService
    {
        private readonly ICsvParser _csvParser;
        private readonly IDateParser _dateParser;
        private readonly ILogger<WorkCalculationService>? _logger;
        private List<PairResult> _lastComputedPairs = new();

        public WorkCalculationService(
            ICsvParser csvParser,
            IDateParser dateParser,
            ILogger<WorkCalculationService>? logger = null)
        {
            _csvParser = csvParser;
            _dateParser = dateParser;
            _logger = logger;
        }

        public async Task<PairCalculationResult> CalculatePairsFromCsvAsync(
            Stream csvStream,
            CancellationToken cancellationToken = default)
        {
            // Step 1: Parse CSV to domain records
            var records = await _csvParser.ParseAsync(csvStream, cancellationToken);
            _logger?.LogInformation("Parsed {Count} records from CSV", records.Count());

            if (!records.Any())
            {
                return new PairCalculationResult
                {
                    TopPair = null!,
                    AllPairs = new List<PairResult>()
                };
            }

            // Step 2: Convert string dates to DateTime
            var parsedRecords = records.Select(r => new
            {
                r.EmployeeID,
                r.ProjectID,
                DateFrom = ParseDate(r.DateFrom, treatNullAsToday: false),
                DateTo = ParseDate(r.DateTo, treatNullAsToday: true) // NULL means "today"
            }).ToList();

            _logger?.LogInformation("Converted dates for {Count} records", parsedRecords.Count);

            // Step 3: Calculate pair overlaps
            var pairResults = new Dictionary<(int EmployeeA, int EmployeeB), List<ProjectDetail>>();

            // Group by project to find employees working on the same project
            var projectGroups = parsedRecords.GroupBy(r => r.ProjectID);

            foreach (var projectGroup in projectGroups)
            {
                var projectId = projectGroup.Key;
                var employeesOnProject = projectGroup.ToList();

                _logger?.LogDebug("Project {ProjectId}: {Count} employees", projectId, employeesOnProject.Count);

                // Compare every pair of employees on this project
                for (int i = 0; i < employeesOnProject.Count; i++)
                {
                    for (int j = i + 1; j < employeesOnProject.Count; j++)
                    {
                        var employee1 = employeesOnProject[i];
                        var employee2 = employeesOnProject[j];

                        // Calculate overlap period
                        var overlapStart = employee1.DateFrom > employee2.DateFrom ? employee1.DateFrom : employee2.DateFrom;
                        var overlapEnd = employee1.DateTo < employee2.DateTo ? employee1.DateTo : employee2.DateTo;

                        // Check if there is an overlap
                        if (overlapStart <= overlapEnd)
                        {
                            var dateDifference = (overlapEnd - overlapStart).Days;

                            // Ensure days worked is positive and reasonable
                            if (dateDifference < 0 || dateDifference > 36500) // 100 years max
                            {
                                throw new InvalidOperationException(
                                    $"Invalid date range for employees {employee1.EmployeeID} and {employee2.EmployeeID} on project {projectId}: " +
                                    $"{overlapStart:yyyy-MM-dd} to {overlapEnd:yyyy-MM-dd}");
                            }

                            var daysWorked = dateDifference + 1; // +1 to include both start and end day

                            // Create pair key (always put smaller ID first for consistency)
                            var pairKey = employee1.EmployeeID < employee2.EmployeeID
                                ? (employee1.EmployeeID, employee2.EmployeeID)
                                : (employee2.EmployeeID, employee1.EmployeeID);

                            _logger?.LogDebug(
                                "Pair ({EmployeeA},{EmployeeB}) on Project {ProjectId}: {Days} days ({Start} to {End})",
                                pairKey.Item1, pairKey.Item2, projectId, daysWorked,
                                overlapStart.ToString("yyyy-MM-dd"), overlapEnd.ToString("yyyy-MM-dd"));

                            // Add or update the pair's project details
                            if (!pairResults.ContainsKey(pairKey))
                            {
                                pairResults[pairKey] = new List<ProjectDetail>();
                            }

                            pairResults[pairKey].Add(new ProjectDetail
                            {
                                ProjectId = projectId,
                                DaysWorked = daysWorked,
                                StartDate = overlapStart,
                                EndDate = overlapEnd
                            });
                        }
                        else
                        {
                            _logger?.LogDebug(
                                "No overlap: Employee {Employee1} ({From1} to {To1}) and Employee {Employee2} ({From2} to {To2}) on Project {ProjectId}",
                                employee1.EmployeeID, employee1.DateFrom.ToString("yyyy-MM-dd"), employee1.DateTo.ToString("yyyy-MM-dd"),
                                employee2.EmployeeID, employee2.DateFrom.ToString("yyyy-MM-dd"), employee2.DateTo.ToString("yyyy-MM-dd"),
                                projectId);
                        }
                    }
                }
            }

            // Step 4: Convert to PairResult objects and calculate total days
            var results = pairResults.Select(kvp =>
            {
                var (employeeA, employeeB) = kvp.Key;
                var totalDays = kvp.Value.Sum(p => p.DaysWorked);

                _logger?.LogInformation(
                    "Pair ({EmployeeA},{EmployeeB}): {TotalDays} days across {ProjectCount} projects",
                    employeeA, employeeB, totalDays, kvp.Value.Count);

                return new PairResult
                {
                    EmployeeIdA = employeeA,
                    EmployeeIdB = employeeB,
                    TotalDays = totalDays,
                    Projects = kvp.Value.OrderByDescending(p => p.DaysWorked).ToList()
                };
            }).OrderByDescending(pr => pr.TotalDays).ToList();

            // Store for later retrieval
            _lastComputedPairs = results;

            // Get the top pair
            var topPair = results.First();

            _logger?.LogInformation(
                "TOP PAIR: ({EmployeeA},{EmployeeB}) worked together for {TotalDays} days",
                topPair.EmployeeIdA, topPair.EmployeeIdB, topPair.TotalDays);

            return new PairCalculationResult
            {
                TopPair = topPair,
                AllPairs = results
            };
        }

        private DateTime ParseDate(string? dateString, bool treatNullAsToday)
        {
            if (_dateParser.TryParse(dateString, treatNullAsToday, out DateTime result))
                return result;

            throw new FormatException($"Invalid date format: '{dateString}'");
        }
    }
}
