using ELibrary.Application.Commands.Books;
using ELibrary.Application.Common;
using ELibrary.Application.DTOs;
using ELibrary.Application.Interfaces;
using ELibrary.Application.Queries.Books;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ELibrary.Api.Controllers
{
    /// <summary>
    /// API Controller for managing books in the library using CQRS pattern.
    /// Separates read operations (Queries) from write operations (Commands).
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly ILogger<BookController> _logger;
        private readonly IValidator<BookDto> _bookValidator;

        public BookController(
            IBookService bookService,
            ILogger<BookController> logger,
            IValidator<BookDto> bookValidator)
        {
            _bookService = bookService;
            _logger = logger;
            _bookValidator = bookValidator;
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

            if (result.IsFailure)
            {
                _logger.LogError("Failed to retrieve books: {Error}", result.Error);
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = result.Error,
                    errorCode = result.ErrorCode
                });
            }

            return Ok(result.Value);
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
            // Validate at least one search criterion is provided
            if (string.IsNullOrWhiteSpace(name) &&
                string.IsNullOrWhiteSpace(author) &&
                string.IsNullOrWhiteSpace(isbn))
            {
                _logger.LogWarning("Search attempted with no criteria");
                return BadRequest(new
                {
                    error = "At least one search criterion must be provided",
                    details = "Provide name, author, or ISBN to search"
                });
            }

            _logger.LogInformation(
                "GET /api/v1/book/search - Searching books: Name={Name}, Author={Author}, ISBN={ISBN}",
                name ?? "null", author ?? "null", isbn ?? "null");

            var query = new SearchBooksQuery(name, author, isbn);
            var result = await _bookService.HandleAsync(query);

            if (result.IsFailure)
            {
                _logger.LogError("Search failed: {Error}", result.Error);
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = result.Error,
                    errorCode = result.ErrorCode
                });
            }

            var bookList = result.Value.ToList();

            if (!bookList.Any())
            {
                _logger.LogInformation("No books found matching search criteria");
            }
            else
            {
                _logger.LogInformation("Found {Count} books matching search criteria", bookList.Count);
            }

            return Ok(bookList);
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
        public async Task<ActionResult<BookDto>> CreateBook([FromBody] BookDto bookDto)
        {
            // Manual validation (FluentValidation also runs automatically)
            var validationResult = await _bookValidator.ValidateAsync(bookDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning(
                    "Book creation failed validation: {Errors}",
                    string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

                return BadRequest(new
                {
                    type = "ValidationError",
                    title = "One or more validation errors occurred",
                    errors = validationResult.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray())
                });
            }

            _logger.LogInformation(
                "POST /api/v1/book - Creating book: Name={Name}, Author={Author}, ISBN={ISBN}",
                bookDto.Name, bookDto.Author, bookDto.ISBN);

            // Create command from DTO
            var command = new CreateBookCommand(
                bookDto.Name,
                bookDto.Author,
                bookDto.Year,
                bookDto.ISBN,
                bookDto.ActualQuantity
            );

            var result = await _bookService.HandleAsync(command);

            if (result.IsFailure)
            {
                if (result.ErrorCode == ErrorCodes.DuplicateIsbn)
                {
                    return Conflict(new
                    {
                        error = "Duplicate ISBN",
                        message = result.Error,
                        errorCode = result.ErrorCode,
                        isbn = bookDto.ISBN
                    });
                }

                if (result.ErrorCode == ErrorCodes.ValidationError)
                {
                    return BadRequest(new
                    {
                        error = "Validation error",
                        message = result.Error,
                        errorCode = result.ErrorCode
                    });
                }

                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = result.Error,
                    errorCode = result.ErrorCode
                });
            }

            _logger.LogInformation(
                "Book created successfully with ID: {BookId}",
                result.Value!.ID);

            return CreatedAtAction(
                nameof(GetAllBooks),
                new { id = result.Value.ID },
                result.Value);
        }

        /// <summary>
        /// Borrows a book for a customer
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
            if (bookId == Guid.Empty)
            {
                _logger.LogWarning("Borrow attempt with empty book ID");
                return BadRequest(new { error = "Book ID cannot be empty" });
            }

            var customer = customerName ?? "anonym";

            _logger.LogInformation(
                "POST /api/v1/book/borrow - Customer {CustomerName} borrowing book {BookId}",
                customer, bookId);

            var command = new BorrowBookCommand(bookId, customer);
            var result = await _bookService.HandleAsync(command);

            if (result.IsFailure)
            {
                if (result.ErrorCode == ErrorCodes.NotFound)
                {
                    return NotFound(new
                    {
                        error = "Book not found",
                        message = result.Error,
                        errorCode = result.ErrorCode,
                        bookId
                    });
                }

                if (result.ErrorCode == ErrorCodes.OutOfStock)
                {
                    return BadRequest(new
                    {
                        error = "Out of stock",
                        message = result.Error,
                        errorCode = result.ErrorCode,
                        bookId
                    });
                }

                if (result.ErrorCode == ErrorCodes.ConcurrencyConflict)
                {
                    return Conflict(new
                    {
                        error = "Concurrency conflict",
                        message = result.Error,
                        errorCode = result.ErrorCode,
                        bookId
                    });
                }

                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = result.Error,
                    errorCode = result.ErrorCode,
                    bookId
                });
            }

            _logger.LogInformation(
                "Book borrowed successfully. BookId: {BookId}, RemainingQuantity: {Quantity}",
                bookId, result.Value!.ActualQuantity);

            return Ok(result.Value);
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
            if (bookId == Guid.Empty)
            {
                _logger.LogWarning("Return attempt with empty book ID");
                return BadRequest(new { error = "Book ID cannot be empty" });
            }

            var customer = customerName ?? "anonym";

            _logger.LogInformation(
                "POST /api/v1/book/return - Customer {CustomerName} returning book {BookId}",
                customer, bookId);

            var command = new ReturnBookCommand(bookId, customer);
            var result = await _bookService.HandleAsync(command);

            if (result.IsFailure)
            {
                if (result.ErrorCode == ErrorCodes.NotFound)
                {
                    return NotFound(new
                    {
                        error = "Book not found",
                        message = result.Error,
                        errorCode = result.ErrorCode,
                        bookId
                    });
                }

                if (result.ErrorCode == ErrorCodes.ConcurrencyConflict)
                {
                    return Conflict(new
                    {
                        error = "Concurrency conflict",
                        message = result.Error,
                        errorCode = result.ErrorCode,
                        bookId
                    });
                }

                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = result.Error,
                    errorCode = result.ErrorCode,
                    bookId
                });
            }

            _logger.LogInformation(
                "Book returned successfully. BookId: {BookId}, NewQuantity: {Quantity}",
                bookId, result.Value!.ActualQuantity);

            return Ok(result.Value);
        }
    }
}