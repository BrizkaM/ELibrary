using ELibrary.Shared.DTOs;
using ELibrary.Shared.Entities;
using ELibrary.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ELibrary.WebApp.Controllers
{
    /// <summary>
    /// Book controller implementation.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class BookController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly IBookService _bookService;
        private readonly ILogger<BookController> _logger;

        /// <summary>
        /// Creates a new instance of the BookController class.
        /// </summary>
        /// <param name="bookRepository">The book repository.</param>
        /// <param name="bookService">The book services.</param>
        /// <param name="logger">The logger.</param>
        public BookController(IBookRepository bookRepository, IBookService bookService, ILogger<BookController> logger)
        {
            _bookRepository = bookRepository;
            _bookService = bookService;
            _logger = logger;
        }

        /// <summary>
        /// Get all books.
        /// </summary>
        /// <returns>Collection of BookDto.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<BookDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetAllBooks()
        {
            var books = (await _bookRepository.GetAllAsync()).Select(MapToDto);
            return Ok(books);
        }

        /// <summary>
        /// Gets books by criteria.
        /// </summary>
        /// <param name="name">Title of a book.</param>
        /// <param name="author">Author's name.</param>
        /// <param name="isbn">The ISBN of a book.</param>
        /// <returns>Collection of BookDto.</returns>
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

        /// <summary>
        /// Creates a new book.
        /// </summary>
        /// <param name="bookDto">The book dto.</param>
        /// <returns>Created book dto.</returns>
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

        /// <summary>
        /// Lets a customer borrow a book.
        /// </summary>
        /// <param name="bookId">The book identifier.</param>
        /// <param name="customerName">Customer's name.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Lets a customer return a book.
        /// </summary>
        /// <param name="bookId">The book identifier.</param>
        /// <param name="customerName">Customer's name.</param>
        /// <returns>Updated Book Dto in case of success.</returns>
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

        /// <summary>
        /// Evaluetaes the customer operation result and returns appropriate HTTP response.
        /// </summary>
        /// <param name="bookId">The book identifier.</param>
        /// <param name="updatedBook">Operation result from service and book itself.</param>
        /// <returns>Action result.</returns>
        private ActionResult<BookDto> EvaluateCustomerOperation(Guid bookId, (Shared.Enums.CustomerBookOperationResult OperationResult, Book? UpdatedBook) updatedBook)
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

        /// <summary>
        /// Maps book entity to book Dto.
        /// </summary>
        /// <param name="book">The book entity.</param>
        /// <returns>The book Dto for Wevb api communication.</returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bookDto"></param>
        /// <returns></returns>
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
 