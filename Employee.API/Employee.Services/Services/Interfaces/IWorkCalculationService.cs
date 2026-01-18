using Employee.Domain.Models;

namespace Employee.Services.Services.Interfaces
{
    public interface IWorkCalculationService
    {
        Task<PairCalculationResult> CalculatePairsFromCsvAsync(Stream csvStream, CancellationToken cancellationToken = default);
    }
}
