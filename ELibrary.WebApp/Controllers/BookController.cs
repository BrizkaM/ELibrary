using ELibrary.Shared.DTOs;
using ELibrary.Shared.Entities;
using ELibrary.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ELibrary.WebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class BookController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly IBorrowBookRecordRepository _borrowBookRecordRepository;
        private readonly ILogger<BookController> _logger;

        public BookController(IBookRepository bookRepository, IBorrowBookRecordRepository borrowBookRecordRepository, ILogger<BookController> logger)
        {
            _bookRepository = bookRepository;
            _borrowBookRecordRepository = borrowBookRecordRepository;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<BookDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetAllBooks()
        {
            var books = (await _bookRepository.GetAllAsync()).Select(MapToDto);
            return Ok(books);
        }

        [HttpGet("criteria")]
        [ProducesResponseType(typeof(IEnumerable<BookDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<BookDto>>> FindBooksByCriteria(string? name, string? author, string? isbn)
        {
            var books = (await _bookRepository.GetFilteredBooksAsync(name, author, isbn)).Select(MapToDto);

            if (books != null && !books.Any())
            {
                return NotFound();
            }

            return Ok(books);
        }

        [HttpPatch("borrow")]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BookDto>> BorrowBook(Guid bookId, string? customerName)
        {
            var updatedBook = await _bookRepository.BorrowBookAsync(bookId);

            var actionResult = EvaluateCustomerOperation(bookId, updatedBook);

            if (actionResult.Result is OkObjectResult)
            {
                var borrowRecord = new BorrowBookRecord
                {
                    ID = Guid.NewGuid(),
                    Book = updatedBook.UpdatedBook!,
                    BookID = bookId,
                    CustomerName = customerName ?? "anonym",
                    Action = "Borrowed",
                    Date = DateTime.UtcNow
                };

                await _borrowBookRecordRepository.AddAsync(borrowRecord);
            }

            return actionResult;
        }


        [HttpPatch("return")]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BookDto>> ReturnBook(Guid bookId, string? customerName)
        {
            var updatedBook = await _bookRepository.ReturnBookAsync(bookId);
            var actionResult = EvaluateCustomerOperation(bookId, updatedBook);

            if (actionResult.Result is OkObjectResult)
            {
                var borrowRecord = new BorrowBookRecord
                {
                    ID = Guid.NewGuid(),
                    Book = updatedBook.UpdatedBook!,
                    BookID = bookId,
                    CustomerName = customerName ?? "anonym",
                    Action = "Returned",
                    Date = DateTime.UtcNow
                };

                await _borrowBookRecordRepository.AddAsync(borrowRecord);
            }

            return actionResult;
        }

        internal ActionResult<BookDto> EvaluateCustomerOperation(Guid bookId, (Shared.Enums.CustomerBookOperationResult OperationResult, Book? UpdatedBook) updatedBook)
        {
            if (updatedBook.OperationResult == Shared.Enums.CustomerBookOperationResult.NotFound)
            {
                var message = $"Book with ID {bookId} not found.";
                _logger.LogWarning(message);
                return NotFound(message);
            }

            if (updatedBook.OperationResult == Shared.Enums.CustomerBookOperationResult.Conflict)
            {
                var message = $"Book with ID {bookId} updated by another user.";
                _logger.LogWarning(message);
                return Conflict(message);
            }

            if (updatedBook.OperationResult == Shared.Enums.CustomerBookOperationResult.OutOfStock)
            {
                var message = $"Book with ID {bookId} is out of stock.";
                return BadRequest(message);
            }

            if (updatedBook.UpdatedBook == null)
                return BadRequest("Unable to update the book.");

            return Ok(MapToDto(updatedBook.UpdatedBook));
        }

        private BookDto MapToDto(Book book)
        {
            return new BookDto
            {
                ID = book.ID,
                Name = book.Name,
                Author = book.Author,
                Year = book.Year,
                ISBN = book.ISBN,
                ActualQuantity = book.ActualQuantity
            };
        }
    }
}
 