using ELibrary.Shared.DTOs;
using ELibrary.Shared.Entities;
using ELibrary.Shared.Interfaces;
using ELibrary.Shared.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ELibrary.WebApp.Controllers
{
    /// <summary>
    /// API Controller for managing books in the library
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class BookController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly IBookService _bookService;
        private readonly ILogger<BookController> _logger;
        private readonly IValidator<BookDto> _bookValidator;

        public BookController(
            IBookRepository bookRepository,
            IBookService bookService,
            ILogger<BookController> logger,
            IValidator<BookDto> bookValidator)
        {
            _bookRepository = bookRepository;
            _bookService = bookService;
            _logger = logger;
            _bookValidator = bookValidator;
        }

        /// <summary>
        /// Gets all books in the library
        /// </summary>
        /// <returns>A collection of all books</returns>
        /// <response code="200">Returns the list of books</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<BookDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetAllBooks()
        {
            _logger.LogInformation("Retrieving all books from library");

            var books = (await _bookRepository.GetAllAsync()).Select(MapToDto);

            _logger.LogInformation("Retrieved {Count} books", books.Count());

            return Ok(books);
        }

        /// <summary>
        /// Searches for books using specified criteria
        /// </summary>
        /// <param name="name">Book name to search for (optional)</param>
        /// <param name="author">Author name to search for (optional)</param>
        /// <param name="isbn">ISBN to search for (optional)</param>
        /// <returns>A collection of books matching the criteria</returns>
        /// <response code="200">Returns the matching books</response>
        /// <response code="404">No books found matching the criteria</response>
        /// <response code="400">Invalid search criteria</response>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<BookDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
                "Searching books with criteria: Name={Name}, Author={Author}, ISBN={ISBN}",
                name ?? "null", author ?? "null", isbn ?? "null");

            var books = (await _bookRepository.GetFilteredBooksAsync(name, author, isbn))
                .Select(MapToDto)
                .ToList();

            if (!books.Any())
            {
                _logger.LogInformation("No books found matching search criteria");
                return NotFound(new
                {
                    message = "No books found matching the search criteria"
                });
            }

            _logger.LogInformation("Found {Count} books matching search criteria", books.Count);

            return Ok(books);
        }

        /// <summary>
        /// Creates a new book in the library
        /// </summary>
        /// <param name="bookDto">The book data to create</param>
        /// <returns>The created book</returns>
        /// <response code="201">Book created successfully</response>
        /// <response code="400">Invalid book data or validation failed</response>
        /// <response code="409">Book with same ISBN already exists</response>
        [HttpPost]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
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

            try
            {
                _logger.LogInformation(
                    "Creating new book: {Name} by {Author} (ISBN: {ISBN})",
                    bookDto.Name, bookDto.Author, bookDto.ISBN);

                var createdBook = await _bookService.CreateBookAsync(MapFromDto(bookDto));

                _logger.LogInformation(
                    "Book created successfully with ID: {BookId}",
                    createdBook.ID);

                return CreatedAtAction(
                    nameof(GetAllBooks),
                    new { id = createdBook.ID },
                    MapToDto(createdBook));
            }
            catch (ArgumentException ex) when (ex.Message.Contains("ISBN"))
            {
                _logger.LogWarning(
                    "Book creation failed: Duplicate ISBN {ISBN}",
                    bookDto.ISBN);

                return Conflict(new
                {
                    error = "Duplicate ISBN",
                    message = ex.Message,
                    isbn = bookDto.ISBN
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(
                    "Book creation failed: {Message}",
                    ex.Message);

                return BadRequest(new
                {
                    error = "Invalid book data",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Borrows a book for a customer
        /// </summary>
        /// <param name="bookId">The ID of the book to borrow</param>
        /// <param name="customerName">The name of the customer (optional, defaults to "anonym")</param>
        /// <returns>The updated book information</returns>
        /// <response code="200">Book borrowed successfully</response>
        /// <response code="400">Book is out of stock</response>
        /// <response code="404">Book not found</response>
        /// <response code="409">Concurrency conflict occurred</response>
        [HttpPost("borrow")]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
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
                "Customer {CustomerName} attempting to borrow book {BookId}",
                customer, bookId);

            var updatedBook = await _bookService.BorrowBookAsync(bookId, customer);

            return EvaluateCustomerOperation(bookId, updatedBook, "borrow");
        }

        /// <summary>
        /// Returns a borrowed book
        /// </summary>
        /// <param name="bookId">The ID of the book to return</param>
        /// <param name="customerName">The name of the customer (optional, defaults to "anonym")</param>
        /// <returns>The updated book information</returns>
        /// <response code="200">Book returned successfully</response>
        /// <response code="404">Book not found</response>
        /// <response code="409">Concurrency conflict occurred</response>
        [HttpPost("return")]
        [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
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
                "Customer {CustomerName} attempting to return book {BookId}",
                customer, bookId);

            var updatedBook = await _bookService.ReturnBookAsync(bookId, customer);

            return EvaluateCustomerOperation(bookId, updatedBook, "return");
        }

        /// <summary>
        /// Evaluates the result of a customer book operation (borrow/return)
        /// </summary>
        private ActionResult<BookDto> EvaluateCustomerOperation(
            Guid bookId,
            (Shared.Enums.CustomerBookOperationResult OperationResult, Book? UpdatedBook) result,
            string operationType)
        {
            switch (result.OperationResult)
            {
                case Shared.Enums.CustomerBookOperationResult.NotFound:
                    var notFoundMessage = $"Book with ID {bookId} not found";
                    _logger.LogWarning(notFoundMessage);
                    return NotFound(new
                    {
                        error = "Book not found",
                        message = notFoundMessage,
                        bookId
                    });

                case Shared.Enums.CustomerBookOperationResult.Conflict:
                    var conflictMessage = $"Book with ID {bookId} was updated by another user. Please retry.";
                    _logger.LogWarning(conflictMessage);
                    return Conflict(new
                    {
                        error = "Concurrency conflict",
                        message = conflictMessage,
                        bookId
                    });

                case Shared.Enums.CustomerBookOperationResult.OutOfStock:
                    var outOfStockMessage = $"Book with ID {bookId} is out of stock";
                    _logger.LogWarning(outOfStockMessage);
                    return BadRequest(new
                    {
                        error = "Out of stock",
                        message = outOfStockMessage,
                        bookId
                    });

                case Shared.Enums.CustomerBookOperationResult.Success:
                    if (result.UpdatedBook == null)
                    {
                        _logger.LogError(
                            "Book operation succeeded but returned book is null. BookId: {BookId}",
                            bookId);
                        return StatusCode(500, new
                        {
                            error = "Internal server error",
                            message = "Operation succeeded but book data is unavailable"
                        });
                    }

                    _logger.LogInformation(
                        "Book {OperationType} successful. BookId: {BookId}, RemainingQuantity: {Quantity}",
                        operationType, bookId, result.UpdatedBook.ActualQuantity);

                    return Ok(MapToDto(result.UpdatedBook));

                default:
                    _logger.LogError(
                        "Unknown operation result: {Result}",
                        result.OperationResult);
                    return StatusCode(500, new
                    {
                        error = "Unknown operation result",
                        message = "An unexpected error occurred"
                    });
            }
        }

        /// <summary>
        /// Maps Book entity to BookDto
        /// </summary>
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
        /// Maps BookDto to Book entity
        /// </summary>
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
