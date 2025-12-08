using ELibrary.Services.Interfaces;
using ELibrary.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ELibrary.WebApp.Controllers
{
    /// <summary>
    /// Implements API endpoints for managing borrow book records.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class BorrowBookRecordController : ControllerBase
    {
        private readonly IBorrowBookRecordService _borrowBookRecordRepositoryService;
        /// <summary>
        /// Cerates a new instance of the BorrowBookRecordController class.
        /// </summary>
        /// <param name="borrowBookRecordRepository">The borrow book record repository.</param>
        public BorrowBookRecordController(IBorrowBookRecordService borrowBookRecordRepository)
        {
            _borrowBookRecordRepositoryService = borrowBookRecordRepository;
        }

        /// <summary>
        /// Gets all borrow book records.
        /// </summary>
        /// <returns>A collection of Borrow book record dtos.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<BorrowBookRecordDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BorrowBookRecordDto>>> GetAllBorrowBookRecords()
        {
            return Ok(await _borrowBookRecordRepositoryService.GetAllBorrowBookRecordsAsync());
        }
    }
}
