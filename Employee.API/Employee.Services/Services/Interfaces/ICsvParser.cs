using Employee.Domain.Models;

namespace Employee.Services.Services.Interfaces
{
    public interface ICsvParser
    {
        Task<IEnumerable<EmployeeProjectRecord>> ParseAsync(Stream csvStream, CancellationToken cancellationToken = default);
    }
}
