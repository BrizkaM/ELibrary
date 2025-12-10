using AutoMapper;
using ELibrary.Application.Commands.Books;
using ELibrary.Application.Common;
using ELibrary.Application.DTOs;
using ELibrary.Application.Interfaces;
using ELibrary.Application.Queries.Books;
using ELibrary.Domain.Entities;
using ELibrary.Domain.Enums;
using ELibrary.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ELibrary.Application.Services
{
    /// <summary>
    /// Book service implementation using CQRS pattern.
    /// Separates read operations (Queries) from write operations (Commands).
    /// </summary>
    public class BookService : IBookService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BookService> _logger;
        private readonly IMapper _mapper;

        public BookService(IUnitOfWork unitOfWork, ILogger<BookService> logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // ============================================================
        // QUERY HANDLERS (Read Operations)
        // ============================================================

        /// <summary>
        /// Handles GetAllBooksQuery to retrieve all books.
        /// </summary>
        public async Task<ELibraryResult<IEnumerable<BookDto>>> HandleAsync(GetAllBooksQuery query)
        {
            try
            {
                _logger.LogInformation("Handling GetAllBooksQuery");

                var entities = await _unitOfWork.Books.GetAllAsync();
                var dtos = entities.Select(e => _mapper.Map<BookDto>(e));

                _logger.LogInformation("Retrieved {Count} books", dtos.Count());

                return ELibraryResult<IEnumerable<BookDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling GetAllBooksQuery");
                return ELibraryResult<IEnumerable<BookDto>>.Failure(
                    "An error occurred while retrieving books",
                    ErrorCodes.InvalidOperation);
            }
        }

        /// <summary>
        /// Handles SearchBooksQuery to find books by criteria.
        /// </summary>
        public async Task<ELibraryResult<IEnumerable<BookDto>>> HandleAsync(SearchBooksQuery query)
        {
            try
            {
                _logger.LogInformation(
                    "Handling SearchBooksQuery with criteria: Name={Name}, Author={Author}, ISBN={ISBN}",
                    query.Name ?? "null", query.Author ?? "null", query.ISBN ?? "null");

                var entities = await _unitOfWork.Books.GetFilteredBooksAsync(
                    query.Name,
                    query.Author,
                    query.ISBN);

                var dtos = entities.Select(e => _mapper.Map<BookDto>(e));

                _logger.LogInformation("Search completed. Found {Count} books", dtos.Count());

                return ELibraryResult<IEnumerable<BookDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling SearchBooksQuery");
                return ELibraryResult<IEnumerable<BookDto>>.Failure(
                    "An error occurred while searching books",
                    ErrorCodes.InvalidOperation);
            }
        }

        // ============================================================
        // COMMAND HANDLERS (Write Operations)
        // ============================================================

        /// <summary>
        /// Handles CreateBookCommand to add a new book to the library.
        /// </summary>
        public async Task<ELibraryResult<BookDto>> HandleAsync(CreateBookCommand command)
        {
            try
            {
                _logger.LogInformation(
                    "Handling CreateBookCommand: Name={Name}, Author={Author}, ISBN={ISBN}",
                    command.Name, command.Author, command.ISBN);

                // Business rule: ISBN must be unique
                var existingISBN = await _unitOfWork.Books.GetByISBNAsync(command.ISBN);
                if (existingISBN != null)
                {
                    _logger.LogWarning(
                        "CreateBookCommand failed: Duplicate ISBN={ISBN}",
                        command.ISBN);

                    return ELibraryResult<BookDto>.Failure(
                        $"Book with ISBN '{command.ISBN}' already exists",
                        ErrorCodes.DuplicateIsbn);
                }

                // Map command to entity
                var book = new Book
                {
                    Name = command.Name,
                    Author = command.Author,
                    Year = command.Year,
                    ISBN = command.ISBN,
                    ActualQuantity = command.ActualQuantity
                };

                var addedBook = await _unitOfWork.Books.AddAsync(book);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "CreateBookCommand succeeded. BookId={BookId}, Title={Title}",
                    addedBook.ID, addedBook.Name);

                return ELibraryResult<BookDto>.Success(_mapper.Map<BookDto>(addedBook));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling CreateBookCommand");
                return ELibraryResult<BookDto>.Failure(
                    "An error occurred while creating the book",
                    ErrorCodes.InvalidOperation);
            }
        }

        /// <summary>
        /// Handles BorrowBookCommand to borrow a book from the library.
        /// </summary>
        public async Task<ELibraryResult<BookDto>> HandleAsync(BorrowBookCommand command)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _logger.LogInformation(
                    "Handling BorrowBookCommand: BookId={BookId}, Customer={CustomerName}",
                    command.BookId, command.CustomerName);

                var book = await _unitOfWork.Books.GetByIdAsync(command.BookId);

                if (book == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogWarning("BorrowBookCommand failed: Book not found. BookId={BookId}", command.BookId);

                    return ELibraryResult<BookDto>.Failure(
                        $"Book with ID {command.BookId} not found",
                        ErrorCodes.NotFound);
                }

                if (book.ActualQuantity <= 0)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogWarning(
                        "BorrowBookCommand failed: Out of stock. BookId={BookId}, Title={Title}",
                        command.BookId, book.Name);

                    return ELibraryResult<BookDto>.Failure(
                        $"Book '{book.Name}' is out of stock",
                        ErrorCodes.OutOfStock);
                }

                // Update book quantity
                book.ActualQuantity -= 1;
                _unitOfWork.Books.Update(book);

                // Create borrow record
                var bookRecord = new BorrowBookRecord
                {
                    BookID = command.BookId,
                    Book = book,
                    CustomerName = command.CustomerName,
                    Action = BookActionType.Borrowed.ToString(),
                    Date = DateTime.UtcNow
                };

                await _unitOfWork.BorrowRecords.AddAsync(bookRecord);

                // Save and commit
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation(
                    "BorrowBookCommand succeeded. BookId={BookId}, Customer={CustomerName}, RemainingQuantity={Quantity}",
                    command.BookId, command.CustomerName, book.ActualQuantity);

                return ELibraryResult<BookDto>.Success(_mapper.Map<BookDto>(book));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogWarning(ex,
                    "BorrowBookCommand failed: Concurrency conflict. BookId={BookId}",
                    command.BookId);

                return ELibraryResult<BookDto>.Failure(
                    $"Book with ID {command.BookId} was updated by another user. Please retry",
                    ErrorCodes.ConcurrencyConflict);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex,
                    "Error handling BorrowBookCommand. BookId={BookId}",
                    command.BookId);

                return ELibraryResult<BookDto>.Failure(
                    "An unexpected error occurred while borrowing the book",
                    ErrorCodes.InvalidOperation);
            }
        }

        /// <summary>
        /// Handles ReturnBookCommand to return a borrowed book.
        /// </summary>
        public async Task<ELibraryResult<BookDto>> HandleAsync(ReturnBookCommand command)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _logger.LogInformation(
                    "Handling ReturnBookCommand: BookId={BookId}, Customer={CustomerName}",
                    command.BookId, command.CustomerName);

                var book = await _unitOfWork.Books.GetByIdAsync(command.BookId);

                if (book == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogWarning("ReturnBookCommand failed: Book not found. BookId={BookId}", command.BookId);

                    return ELibraryResult<BookDto>.Failure(
                        $"Book with ID {command.BookId} not found",
                        ErrorCodes.NotFound);
                }

                // Update book quantity
                book.ActualQuantity += 1;
                _unitOfWork.Books.Update(book);

                // Create return record
                var bookRecord = new BorrowBookRecord
                {
                    BookID = command.BookId,
                    Book = book,
                    CustomerName = command.CustomerName,
                    Action = BookActionType.Returned.ToString(),
                    Date = DateTime.UtcNow
                };

                await _unitOfWork.BorrowRecords.AddAsync(bookRecord);

                // Save and commit
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation(
                    "ReturnBookCommand succeeded. BookId={BookId}, Customer={CustomerName}, NewQuantity={Quantity}",
                    command.BookId, command.CustomerName, book.ActualQuantity);

                return ELibraryResult<BookDto>.Success(_mapper.Map<BookDto>(book));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogWarning(ex,
                    "ReturnBookCommand failed: Concurrency conflict. BookId={BookId}",
                    command.BookId);

                return ELibraryResult<BookDto>.Failure(
                    $"Book with ID {command.BookId} was updated by another user. Please retry",
                    ErrorCodes.ConcurrencyConflict);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex,
                    "Error handling ReturnBookCommand. BookId={BookId}",
                    command.BookId);

                return ELibraryResult<BookDto>.Failure(
                    "An unexpected error occurred while returning the book",
                    ErrorCodes.InvalidOperation);
            }
        }
    }
}