using AutoMapper;
using Employee.Contracts.Models;
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
        private readonly IMapper _mapper;

        public EmployeeController(ILogger<EmployeeController> logger, IWorkCalculationService workService, IMapper mapper)
        {
            _logger = logger;
            _workService = workService;
            _mapper = mapper;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> UploadCsv(IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file uploaded" });

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { error = "Only CSV files are allowed" });

            try
            {
                using var stream = file.OpenReadStream();
                var calculationResult = await _workService.CalculatePairsFromCsvAsync(stream, cancellationToken);

                if (calculationResult.TopPair == null || !calculationResult.AllPairs.Any())
                {
                    return Ok(new
                    {
                        message = "No employee pairs found. Employees must work on the same project with overlapping dates.",
                        topPair = (object?)null,
                        allPairs = Array.Empty<object>(),
                        totalPairsFound = 0
                    });
                }

                var result = _mapper.Map<ResultViewModel>(calculationResult);
                return Ok(result);
            }
            catch (FormatException fe)
            {
                _logger.LogError(fe, "CSV format error");
                return BadRequest(new { error = "Invalid CSV format", details = fe.Message });
            }
            catch (InvalidOperationException ioe)
            {
                _logger.LogError(ioe, "Data validation error");
                return BadRequest(new { error = "Invalid data", details = ioe.Message });
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Upload cancelled by client");
                return StatusCode(StatusCodes.Status499ClientClosedRequest, new { error = "Upload cancelled" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during CSV processing");
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Processing failed", details = ex.Message });
            }
        }
    }
}
