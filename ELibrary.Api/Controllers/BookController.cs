using Asp.Versioning;
using ELibrary.Api.Extensions;
using ELibrary.Application.Commands.Books.BorrowBook;
using ELibrary.Application.Commands.Books.CreateBook;
using ELibrary.Application.Commands.Books.ReturnBook;
using ELibrary.Application.DTOs;
using ELibrary.Application.Queries.Books.GetAllBooks;
using ELibrary.Application.Queries.Books.SearchBooks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELibrary.Api.Controllers
{
    /// <summary>
    /// API Controller for managing books in the library using MediatR and CQRS pattern.
    /// Separates read operations (Queries) from write operations (Commands).
    /// Uses MediatR for clean separation of concerns and pipeline behaviors.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    public class BookController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<BookController> _logger;

        public BookController(
            IMediator mediator,
            ILogger<BookController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ============================================================
        // QUERY ENDPOINTS (Read Operations)
        // ============================================================

        /// <summary>
        /// Gets all books in the library
        /// </summary>
        /// <returns>Collection of all books</returns>
        /// <response code="200">Returns the list of books</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<BookDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetAllBooks(CancellationToken cancellationToken)
        {
            _logger.LogInformation("GET /api/v1/book - Retrieving all books");

            var query = new GetAllBooksQuery();
            var result = await _mediator.Send(query, cancellationToken);

            return result.ToActionResult(this);
        }

        /// <summary>
        /// Searches books by name, author, or ISBN
        /// </summary>
        /// <param name="query">Search criteria</param>
        /// <returns>Collection of matching books</returns>
        /// <response code="200">Returns matching books</response>
        /// <response code="400">Invalid search criteria</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("search")]
        [ProducesResponseType(typeof(IEnumerable<BookDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<BookDto>>> SearchBooks([FromBody] SearchBooksQuery query, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "POST /api/v1/book/search - Searching: Name={Name}, Author={Author}, ISBN={ISBN}",
                query.Name ?? "null", query.Author ?? "null", query.ISBN ?? "null");

            var result = await _mediator.Send(query, cancellationToken);

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
        /// <response code="400">Invalid book data</response>
        /// <response code="409">Book with same ISBN already exists</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BookDto>> CreateBook([FromBody] CreateBookCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "POST /api/v1/book - Creating book: Name={Name}, Author={Author}, ISBN={ISBN}",
                command.Name, command.Author, command.ISBN);

            var result = await _mediator.Send(command, cancellationToken);

            return result.ToCreatedResult(
                this,
                nameof(GetAllBooks),
                new { id = result.Value?.ID });
        }

        /// <summary>
        /// Borrows a book from the library
        /// </summary>
        /// <param name="command">The borrow book command</param>
        /// <returns>The updated book</returns>
        /// <response code="200">Book borrowed successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="404">Book not found</response>
        /// <response code="409">Book out of stock or concurrency conflict</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("borrow")]
        [Authorize]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BookDto>> BorrowBook([FromBody] BorrowBookCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "POST /api/v1/book/borrow - Customer {CustomerName} borrowing book {BookId}",
                command.CustomerName, command.BookId);

            var result = await _mediator.Send(command, cancellationToken);

            return result.ToActionResult(this);
        }

        /// <summary>
        /// Returns a borrowed book to the library
        /// </summary>
        /// <param name="command">The return book command</param>
        /// <returns>The updated book</returns>
        /// <response code="200">Book returned successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="404">Book not found</response>
        /// <response code="409">Concurrency conflict</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("return")]
        [Authorize]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BookDto>> ReturnBook([FromBody] ReturnBookCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "POST /api/v1/book/return - Customer {CustomerName} returning book {BookId}",
                command.CustomerName, command.BookId);

            var result = await _mediator.Send(command, cancellationToken);

            return result.ToActionResult(this);
        }
    }
}
