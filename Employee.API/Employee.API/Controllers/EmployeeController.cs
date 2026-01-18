using Employee.Services.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Employee.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly ILogger<EmployeeController> _logger;
        private readonly IWorkCalculationService _workService;

        public EmployeeController(ILogger<EmployeeController> logger, IWorkCalculationService workService)
        {
            _logger = logger;
            _workService = workService;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> UploadCsv(IFormFile file, CancellationToken cancellationToken)
        {
            using var stream = file.OpenReadStream();
            var calculationResult = await _workService.CalculatePairsFromCsvAsync(stream, cancellationToken);

            return Ok(calculationResult);
        }
    }
}
