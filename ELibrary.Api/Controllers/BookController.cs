using Asp.Versioning;
using ELibrary.Api.Extensions;
using ELibrary.Application.Commands.Books;
using ELibrary.Application.DTOs;
using ELibrary.Application.Interfaces;
using ELibrary.Application.Queries.Books;
using Microsoft.AspNetCore.Mvc;

namespace ELibrary.Api.Controllers
{
    /// <summary>
    /// API Controller for managing books in the library using CQRS pattern.
    /// Separates read operations (Queries) from write operations (Commands).
    /// Uses automatic FluentValidation via model binding.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly ILogger<BookController> _logger;

        public BookController(
            IBookService bookService,
            ILogger<BookController> logger)
        {
            _bookService = bookService;
            _logger = logger;
        }

        // ============================================================
        // QUERY ENDPOINTS (Read Operations)
        // ============================================================

        /// <summary>
        /// Gets all books in the library
        /// </summary>
        /// <returns>A collection of all books</returns>
        /// <response code="200">Returns the list of books</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<BookDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetAllBooks()
        {
            _logger.LogInformation("GET /api/v1/book - Retrieving all books");

            var query = new GetAllBooksQuery();
            var result = await _bookService.HandleAsync(query);

            return result.ToActionResult(this);
        }

        /// <summary>
        /// Searches for books using specified criteria
        /// </summary>
        /// <param name="name">Book name to search for (optional)</param>
        /// <param name="author">Author name to search for (optional)</param>
        /// <param name="isbn">ISBN to search for (optional)</param>
        /// <returns>A collection of books matching the criteria</returns>
        /// <response code="200">Returns the matching books</response>
        /// <response code="400">Invalid search criteria</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<BookDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<BookDto>>> SearchBooks(
            [FromQuery] string? name,
            [FromQuery] string? author,
            [FromQuery] string? isbn)
        {
            var query = new SearchBooksQuery(name, author, isbn);
            // auto validation by FluentValidation

            _logger.LogInformation(
                "GET /api/v1/book/search - Searching: Name={Name}, Author={Author}, ISBN={ISBN}",
                name ?? "null", author ?? "null", isbn ?? "null");

            var result = await _bookService.HandleAsync(query);

            return result.ToActionResult(this);
        }

        // ============================================================
        // COMMAND ENDPOINTS (Write Operations)
        // ============================================================

        /// <summary>
        /// Creates a new book in the library
        /// </summary>
        /// <param name="bookDto">The book data to create</param>
        /// <returns>The created book</returns>
        /// <response code="201">Book created successfully</response>
        /// <response code="400">Invalid book data or validation failed</response>
        /// <response code="409">Book with same ISBN already exists</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BookDto>> CreateBook([FromBody] CreateBookCommand command)
        {
            _logger.LogInformation(
                "POST /api/v1/book - Creating book: Name={Name}, Author={Author}, ISBN={ISBN}",
                command.Name, command.Author, command.ISBN);

            var result = await _bookService.HandleAsync(command);

            return result.ToCreatedResult(
                this,
                nameof(GetAllBooks),
                new { id = result.Value?.ID });
        }

        /// <summary>
        /// Borrows a book to a customer
        /// </summary>
        /// <param name="bookId">The ID of the book to borrow</param>
        /// <param name="customerName">The name of the customer (optional, defaults to "anonym")</param>
        /// <returns>The updated book information</returns>
        /// <response code="200">Book borrowed successfully</response>
        /// <response code="400">Book is out of stock or invalid request</response>
        /// <response code="404">Book not found</response>
        /// <response code="409">Concurrency conflict occurred</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("borrow")]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BookDto>> BorrowBook(
            [FromQuery] Guid bookId,
            [FromQuery] string? customerName)
        {
            var command = new BorrowBookCommand(bookId, customerName ?? "anonym");
            // auto validation by FluentValidation

            _logger.LogInformation(
                "POST /api/v1/book/borrow - Customer {CustomerName} borrowing book {BookId}",
                command.CustomerName, bookId);

            var result = await _bookService.HandleAsync(command);

            return result.ToActionResult(this);
        }

        /// <summary>
        /// Returns a borrowed book
        /// </summary>
        /// <param name="bookId">The ID of the book to return</param>
        /// <param name="customerName">The name of the customer (optional, defaults to "anonym")</param>
        /// <returns>The updated book information</returns>
        /// <response code="200">Book returned successfully</response>
        /// <response code="400">Invalid request</response>
        /// <response code="404">Book not found</response>
        /// <response code="409">Concurrency conflict occurred</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("return")]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BookDto>> ReturnBook(
            [FromQuery] Guid bookId,
            [FromQuery] string? customerName)
        {
            var command = new ReturnBookCommand(bookId, customerName ?? "anonym");
            // auto validation by FluentValidation

            _logger.LogInformation(
                "POST /api/v1/book/return - Customer {CustomerName} returning book {BookId}",
                command.CustomerName, bookId);

            var result = await _bookService.HandleAsync(command);

            return result.ToActionResult(this);
        }
    }
}