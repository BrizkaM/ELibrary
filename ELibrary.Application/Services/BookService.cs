using ELibrary.Application.Interfaces;
using ELibrary.Domain.DTOs;
using ELibrary.Domain.Entities;
using ELibrary.Domain.Enums;
using ELibrary.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ELibrary.Application.Services
{
    /// <summary>
    /// Book service implementation.
    /// </summary>
    public class BookService : IBookService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BookService> _logger;

        /// <summary>
        /// Creates a new instance of the BookService class.
        /// </summary>
        /// <param name="bookRepo">Book repository.</param>
        /// <param name="borrowBookRepo">Borrow book repository.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">DB context.</param>
        public BookService(IUnitOfWork unitOfWork, ILogger<BookService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<BookDto>> GetAllBooksAsync()
        {
            var entities = await _unitOfWork.Books.GetAllAsync();
            return entities.Select(MapToDto);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<BookDto>> SearchBooksAsync(string? name, string? author, string? isbn)
        {
            var entities = await _unitOfWork.Books.GetFilteredBooksAsync(name, author, isbn);
            return entities.Select(MapToDto);
        }

        /// <inheritdoc/>
        public async Task<BookDto> CreateBookAsync(BookDto book)
        {
            // Business rule validation: ISBN must be unique
            var existingISBN = await _unitOfWork.Books.GetByISBNAsync(book.ISBN);
            if (existingISBN != null)
            {
                _logger.LogWarning(
                    "Attempt to create book with duplicate ISBN. ISBN: {ISBN}",
                    book.ISBN);
                throw new ArgumentException($"Book with ISBN '{book.ISBN}' already exists.", nameof(book.ISBN));
            }

            // Id automatically created by EF
            var addedBook = await _unitOfWork.Books.AddAsync(MapFromDto(book));
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Book created successfully. BookId: {BookId}, Title: {Title}, Author: {Author}, ISBN: {ISBN}, Quantity: {Quantity}",
                addedBook.ID, addedBook.Name, addedBook.Author, addedBook.ISBN, addedBook.ActualQuantity);

            return MapToDto(addedBook);
        }

        /// <inheritdoc/>
        public async Task<(CustomerBookOperationResult OperationResult, BookDto? UpdatedBook)> BorrowBookAsync(Guid bookId, string customerName)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var book = await _unitOfWork.Books.GetByIdAsync(bookId);

                if (book == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogWarning("Book not found. BookId: {BookId}", bookId);
                    return (CustomerBookOperationResult.NotFound, null);
                }

                if (book.ActualQuantity <= 0)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogWarning("Book out of stock. BookId: {BookId}, Title: {Title}", bookId, book.Name);
                    return (CustomerBookOperationResult.OutOfStock, null);
                }

                book.ActualQuantity -= 1;
                _unitOfWork.Books.Update(book);

                var bookRecord = new BorrowBookRecord
                {
                    BookID = bookId,
                    Book = book,
                    CustomerName = customerName,
                    Action = BookActionType.Borrowed.ToString(),
                    Date = DateTime.UtcNow
                };

                await _unitOfWork.BorrowRecords.AddAsync(bookRecord);

                _logger.LogInformation(
                    "Book borrowed successfully. BookId: {BookId}, Title: {Title}, Customer: {CustomerName}, RemainingQuantity: {Quantity}",
                    bookId, book.Name, customerName, book.ActualQuantity);

                // Save changes and commit transaction
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return (CustomerBookOperationResult.Success, MapToDto(book));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogWarning(ex,
                    "Concurrency conflict while borrowing book. BookId: {BookId}, Customer: {CustomerName}",
                    bookId, customerName);
                return (CustomerBookOperationResult.Conflict, null);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex,
                    "Unexpected error while borrowing book {BookId} for customer {CustomerName}",
                    bookId, customerName);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<(CustomerBookOperationResult OperationResult, BookDto? UpdatedBook)> ReturnBookAsync(Guid bookId, string customerName)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Get book
                var book = await _unitOfWork.Books.GetByIdAsync(bookId);

                if (book == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogWarning("Book not found. BookId: {BookId}", bookId);
                    return (CustomerBookOperationResult.NotFound, null);
                }

                // Update book quantity
                book.ActualQuantity += 1;
                _unitOfWork.Books.Update(book);

                // Create return record
                var bookRecord = new BorrowBookRecord
                {
                    BookID = bookId,
                    Book = book,
                    CustomerName = customerName,
                    Action = BookActionType.Returned.ToString(),
                    Date = DateTime.UtcNow
                };

                await _unitOfWork.BorrowRecords.AddAsync(bookRecord);

                // Save changes and commit transaction
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation(
                    "Book returned successfully. BookId: {BookId}, Title: {Title}, Customer: {CustomerName}, NewQuantity: {Quantity}",
                    bookId, book.Name, customerName, book.ActualQuantity);

                return (CustomerBookOperationResult.Success, MapToDto(book));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogWarning(ex,
                    "Concurrency conflict while returning book. BookId: {BookId}, Customer: {CustomerName}",
                    bookId, customerName);
                return (CustomerBookOperationResult.Conflict, null);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex,
                    "Unexpected error while returning book. BookId: {BookId}, Customer: {CustomerName}",
                    bookId, customerName);
                throw;
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