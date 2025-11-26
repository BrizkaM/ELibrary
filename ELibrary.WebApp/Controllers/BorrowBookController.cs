using ELibrary.Shared.DTOs;
using ELibrary.Shared.Entities;
using ELibrary.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ELibrary.WebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class BorrowBookController : ControllerBase
    {
        private readonly IBorrowBookRecordRepository _borrowBookRecordRepository;

        public BorrowBookController(IBorrowBookRecordRepository borrowBookRecordRepository)
        {
            _borrowBookRecordRepository = borrowBookRecordRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<BookDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BorrowBookRecord>>> GetAllBorrowBookRecords()
        {
            var books = await _borrowBookRecordRepository.GetAllAsync();
            return Ok(books);
        }
    }
}
