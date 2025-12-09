using ELibrary.Application.Interfaces;
using ELibrary.Domain.DTOs;
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
        
        /// <summary>
        /// Cerates a new instance of the BorrowBookRecordController class.
        /// </summary>
        /// <param name="borrowBookRecordService">The borrow book record repository.</param>
        public BorrowBookRecordController(IBorrowBookRecordService borrowBookRecordService)
        {
            _borrowBookRecordService = borrowBookRecordService;
        }

        /// <summary>
        /// Gets all borrow book records.
        /// </summary>
        /// <returns>A collection of Borrow book record dtos.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<BorrowBookRecordDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BorrowBookRecordDto>>> GetAllBorrowBookRecords()
        {
            return Ok(await _borrowBookRecordService.GetAllBorrowBookRecordsAsync());
        }
    }
}
