using AutoMapper;
using ELibrary.Application.Commands.Books;
using ELibrary.Application.Common;
using ELibrary.Application.DTOs;
using ELibrary.Application.Interfaces;
using ELibrary.Application.Queries.Books;
using ELibrary.Domain.Entities;
using ELibrary.Domain.Enums;
using ELibrary.Domain.Interfaces;
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
            _logger.LogInformation("Handling GetAllBooksQuery");

            var entities = await _unitOfWork.Books.GetAllAsync();
            var dtos = entities.Select(e => _mapper.Map<BookDto>(e));

            _logger.LogInformation("Retrieved {Count} books", dtos.Count());

            return ELibraryResult<IEnumerable<BookDto>>.Success(dtos);
        }

        /// <summary>
        /// Handles SearchBooksQuery to find books by criteria.
        /// </summary>
        public async Task<ELibraryResult<IEnumerable<BookDto>>> HandleAsync(SearchBooksQuery query)
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

        // ============================================================
        // COMMAND HANDLERS (Write Operations)
        // ============================================================

        /// <summary>
        /// Handles CreateBookCommand to add a new book to the library.
        /// </summary>
        public async Task<ELibraryResult<BookDto>> HandleAsync(CreateBookCommand command)
        {
            _logger.LogInformation(
                "Handling CreateBookCommand: Name={Name}, Author={Author}, ISBN={ISBN}",
                command.Name, command.Author, command.ISBN);

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

            // Create book entity
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

        /// <summary>
        /// Handles BorrowBookCommand to borrow a book from the library.
        /// </summary>
        public async Task<ELibraryResult<BookDto>> HandleAsync(BorrowBookCommand command)
        {
            await _unitOfWork.BeginTransactionAsync();

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

            // Business logic
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

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation(
                "BorrowBookCommand succeeded. BookId={BookId}, Customer={CustomerName}, RemainingQuantity={Quantity}",
                command.BookId, command.CustomerName, book.ActualQuantity);

            return ELibraryResult<BookDto>.Success(_mapper.Map<BookDto>(book));
        }

        /// <summary>
        /// Handles ReturnBookCommand to return a borrowed book.
        /// </summary>
        public async Task<ELibraryResult<BookDto>> HandleAsync(ReturnBookCommand command)
        {
            await _unitOfWork.BeginTransactionAsync();

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

            // Business logic
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

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation(
                "ReturnBookCommand succeeded. BookId={BookId}, Customer={CustomerName}, NewQuantity={Quantity}",
                command.BookId, command.CustomerName, book.ActualQuantity);

            return ELibraryResult<BookDto>.Success(_mapper.Map<BookDto>(book));
        }


    }
}