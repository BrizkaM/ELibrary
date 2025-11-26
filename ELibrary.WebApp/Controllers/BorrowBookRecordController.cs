using ELibrary.Shared.DTOs;
using ELibrary.Shared.Entities;
using ELibrary.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ELibrary.WebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class BorrowBookRecordController : ControllerBase
    {
        private readonly IBorrowBookRecordRepository _borrowBookRecordRepository;

        public BorrowBookRecordController(IBorrowBookRecordRepository borrowBookRecordRepository)
        {
            _borrowBookRecordRepository = borrowBookRecordRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<BorrowBookRecordDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BorrowBookRecordDto>>> GetAllBorrowBookRecords()
        {
            var books = await _borrowBookRecordRepository.GetAllAsync();
            return Ok(books.Select(MapToDto));
        }

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
