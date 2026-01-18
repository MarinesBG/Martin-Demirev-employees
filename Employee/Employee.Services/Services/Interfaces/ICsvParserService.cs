using Employee.Domain.Models;

namespace Employee.Services.Services.Interfaces
{
    public interface ICsvParserService
    {
        Task<IEnumerable<EmployeeProjectRecord>> ParseAsync(Stream csvStream, CancellationToken cancellationToken = default);
    }
}
