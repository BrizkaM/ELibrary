using Asp.Versioning;
using ELibrary.Api.Extensions;
using ELibrary.Application.DTOs;
using ELibrary.Application.Queries.BorrowRecords.GetAllBorrowRecords;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ELibrary.Api.Controllers
{
    /// <summary>
    /// API Controller for managing borrow book records using MediatR and CQRS pattern.
    /// Currently supports only query operations (read-only).
    /// Uses MediatR for clean separation of concerns.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    public class BorrowBookRecordController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<BorrowBookRecordController> _logger;

        public BorrowBookRecordController(
            IMediator mediator,
            ILogger<BorrowBookRecordController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all borrow book records.
        /// </summary>
        /// <returns>A collection of borrow book record DTOs.</returns>
        /// <response code="200">Returns the list of borrow book records</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<BorrowBookRecordDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<BorrowBookRecordDto>>> GetAllBorrowBookRecords()
        {
            _logger.LogInformation("GET /api/v1/borrowbookrecord - Retrieving all borrow book records");

            var query = new GetAllBorrowRecordsQuery();
            var result = await _mediator.Send(query);

            return result.ToActionResult(this);
        }
    }
}
