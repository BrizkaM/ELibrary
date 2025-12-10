using ELibrary.Application.DTOs;
using ELibrary.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ELibrary.Api.Controllers
{
    /// <summary>
    /// Implements API endpoints for managing borrow book records.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class BorrowBookRecordController : ControllerBase
    {
        private readonly IBorrowBookRecordService _borrowBookRecordService;
        private readonly ILogger<BorrowBookRecordController> _logger;

        /// <summary>
        /// Creates a new instance of the BorrowBookRecordController class.
        /// </summary>
        /// <param name="borrowBookRecordService">The borrow book record service.</param>
        /// <param name="logger">The logger.</param>
        public BorrowBookRecordController(
            IBorrowBookRecordService borrowBookRecordService,
            ILogger<BorrowBookRecordController> logger)
        {
            _borrowBookRecordService = borrowBookRecordService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all borrow book records.
        /// </summary>
        /// <returns>A collection of Borrow book record dtos.</returns>
        /// <response code="200">Returns the list of borrow book records</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<BorrowBookRecordDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<BorrowBookRecordDto>>> GetAllBorrowBookRecords()
        {
            _logger.LogInformation("Retrieving all borrow book records");

            var result = await _borrowBookRecordService.GetAllBorrowBookRecordsAsync();

            if (result.IsFailure)
            {
                _logger.LogError("Failed to retrieve borrow book records: {Error}", result.Error);
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = result.Error,
                    errorCode = result.ErrorCode
                });
            }

            return Ok(result.Value);
        }
    }
}