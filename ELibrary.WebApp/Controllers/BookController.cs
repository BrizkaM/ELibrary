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
        private readonly ILogger<BookController> _logger;

        public BookController(IBookRepository bookRepository, ILogger<BookController> logger)
        {
            _bookRepository = bookRepository;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<BookDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Book>>> GetAllBooks()
        {
            var books = (await _bookRepository.GetAllAsync()).Select(MapToDto);
            return Ok(books);
        }

        [HttpGet("criteria")]
        [ProducesResponseType(typeof(IEnumerable<BookDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Book>>> FindBooksByCriteria(string? name, string? author, string? isbn)
        {
            var books = (await _bookRepository.GetFilteredBooksAsync(name, author, isbn)).Select(MapToDto);

            if (!books.Any())
            {
                return NotFound();
            }

            return Ok(books);
        }


        [HttpPatch("borrow")]
        [ProducesResponseType(typeof(IEnumerable<BookDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BookDto>> BorrowBook(Guid bookId, string? customerName)
        {
            var startOperation = DateTime.UtcNow;
            var book = await _bookRepository.GetByIdAsync(bookId);
            if (book == null)
            {
                _logger.LogWarning("Book with ID {BookId} not found.", bookId);
                return NotFound();
            }

            if (book.ActualQuantity <= 0)
            {
                _logger.LogInformation("Book with ID {BookId} is out of stock.", bookId);
                return BadRequest("Book is out of stock.");
            }

            book.ActualQuantity -= 1;

            if (book.Udate >= startOperation)
            {
                _logger.LogInformation("Quantity of Book with ID {BookId} has been just updated by another user.", bookId);
                return BadRequest($"Quantity of Book {bookId} has been just updated by another user.");
            }

            var result = await _bookRepository.UpdateAsync(book, startOperation);

            if (!result.Success)
            {
                return Conflict($"Quantity of Book {bookId} has been just updated by another user.");
            }

            return Ok(MapToDto(result.UpdatedBook));
        }

        [HttpPatch("return")]
        [ProducesResponseType(typeof(IEnumerable<BookDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookDto>> ReturnBook(Guid bookId, string? customerName)
        {
            var startOperation = DateTime.UtcNow;
            var book = await _bookRepository.GetByIdAsync(bookId);
            if (book == null)
            {
                _logger.LogWarning("Book with ID {BookId} not found.", bookId);
                return NotFound();
            }

            book.ActualQuantity += 1;

            if (book.Udate >= startOperation)
            { 
                _logger.LogInformation("Quantity of Book with ID {BookId} has been just updated by another user.", bookId);
                return BadRequest($"Quantity of Book {bookId} has been just updated by another user.");
            }

            var result = await _bookRepository.UpdateAsync(book, startOperation);

            if (!result.Success)
            {
                return Conflict($"Quantity of Book {bookId} has been just updated by another user.");
            }

            return Ok(MapToDto(result.UpdatedBook));
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
