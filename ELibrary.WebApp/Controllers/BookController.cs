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
        private readonly IBookService _bookService;
        private readonly ILogger<BookController> _logger;

        public BookController(IBookRepository bookRepository, IBookService bookService, ILogger<BookController> logger)
        {
            _bookRepository = bookRepository;
            _bookService = bookService;
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

        [HttpPost]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BookDto>> CreateBook([FromBody] BookDto bookDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdBook = await _bookService.CreateBookAsync(MapFromDto(bookDto));

            return CreatedAtAction(
                nameof(GetAllBooks),
                new { id = createdBook.ID },
                MapToDto(createdBook));
        }

        [HttpPost("borrow")]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BookDto>> BorrowBook(Guid bookId, string? customerName)
        {
            var updatedBook = await _bookService.BorrowBookAsync(bookId, customerName ?? "anonym");

            return EvaluateCustomerOperation(bookId, updatedBook);
        }


        [HttpPost("return")]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BookDto>> ReturnBook(Guid bookId, string? customerName)
        {
            var updatedBook = await _bookService.ReturnBookAsync(bookId, customerName ?? "anonym");

            return EvaluateCustomerOperation(bookId, updatedBook);
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

        private Book MapFromDto(BookDto bookDto)
        {
            return new Book
            {
                Name = bookDto.Name,
                Author = bookDto.Author,
                Year = bookDto.Year,
                ISBN = bookDto.ISBN,
                ActualQuantity = bookDto.ActualQuantity
            };
        }
    }
}
 