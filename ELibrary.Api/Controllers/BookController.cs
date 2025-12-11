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
    /// Controllers now accept Commands/Queries directly for proper FluentValidation.
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
        /// <param name="command">The create book command</param>
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
            // FluentValidation automatically validates command here!

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
        [HttpPost("borrow")]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BookDto>> BorrowBook([FromBody] BorrowBookCommand command)
        {
            // FluentValidation automatically validates command here!

            _logger.LogInformation(
                "POST /api/v1/book/borrow - Customer {CustomerName} borrowing book {BookId}",
                command.CustomerName, command.BookId);

            var result = await _bookService.HandleAsync(command);

            return result.ToActionResult(this);
        }

        /// <summary>
        /// Returns a borrowed book
        /// </summary>
        [HttpPost("return")]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BookDto>> ReturnBook([FromBody] ReturnBookCommand command)
        {
            // FluentValidation automatically validates command here!

            _logger.LogInformation(
                "POST /api/v1/book/return - Customer {CustomerName} returning book {BookId}",
                command.CustomerName, command.BookId);

            var result = await _bookService.HandleAsync(command);

            return result.ToActionResult(this);
        }
    }
}