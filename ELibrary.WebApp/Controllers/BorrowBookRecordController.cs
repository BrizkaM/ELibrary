using ELibrary.Shared.DTOs;
using ELibrary.Shared.Entities;
using ELibrary.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ELibrary.WebApp.Controllers
{
    /// <summary>
    /// Implements API endpoints for managing borrow book records.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class BorrowBookRecordController : ControllerBase
    {
        private readonly IBorrowBookRecordRepository _borrowBookRecordRepository;

        /// <summary>
        /// Cerates a new instance of the BorrowBookRecordController class.
        /// </summary>
        /// <param name="borrowBookRecordRepository">The borrow book record repository.</param>
        public BorrowBookRecordController(IBorrowBookRecordRepository borrowBookRecordRepository)
        {
            _borrowBookRecordRepository = borrowBookRecordRepository;
        }

        /// <summary>
        /// Gets all borrow book records.
        /// </summary>
        /// <returns>A collection of Borrow book record dtos.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<BorrowBookRecordDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BorrowBookRecordDto>>> GetAllBorrowBookRecords()
        {
            var books = await _borrowBookRecordRepository.GetAllAsync();
            return Ok(books.Select(MapToDto));
        }

        /// <summary>
        /// Maps BorrowBookRecord entity to BorrowBookRecordDto.
        /// </summary>
        /// <param name="record">The borrow book record.</param>
        /// <returns>The borrow book recortd dto.</returns>
        private static BorrowBookRecordDto MapToDto(BorrowBookRecord record)
        {
            return new BorrowBookRecordDto
            {
                ID = record.ID,
                BookID = record.BookID,
                CustomerName = record.CustomerName,
                Action = record.Action,
                Date = record.Date
            };
        }
    }
}
