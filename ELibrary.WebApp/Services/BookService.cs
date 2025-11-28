using ELibrary.Database;
using ELibrary.Shared.Entities;
using ELibrary.Shared.Enums;
using ELibrary.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ELibrary.WebApp.Services
{
    /// <summary>
    /// Book service implementation.
    /// </summary>
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepo;
        private readonly IBorrowBookRecordRepository _borrowBookRepo;
        private readonly ILogger<BookService> _logger;
        private readonly ELibraryDbContext _context;

        /// <summary>
        /// Creates a new instance of the BookService class.
        /// </summary>
        /// <param name="bookRepo">Book repository.</param>
        /// <param name="borrowBookRepo">Borrow book repository.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="context">DB context.</param>
        public BookService(IBookRepository bookRepo, IBorrowBookRecordRepository borrowBookRepo, ILogger<BookService> logger, ELibraryDbContext context)
        {
            _bookRepo = bookRepo;
            _borrowBookRepo = borrowBookRepo;
            _logger = logger;
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<Book> CreateBookAsync(Book book)
        {
            if(book.ActualQuantity < 0)
                throw new ArgumentException("Actual quantity cannot be negative.", nameof(book.ActualQuantity));

            if (book.Year > DateTime.UtcNow)
                throw new ArgumentException("Year cannot be in the future.", nameof(book.Year));

            var existingISBN = await _bookRepo.GetByISBNAsync(book.ISBN);
            if (existingISBN != null)
                throw new ArgumentException("Book with the same ISBN already exists.", nameof(book.ISBN));

            // Id automaticaly created by EF
            var addedBook = await _bookRepo.AddAsync(book);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Book created successfully with ID: {BookId}", addedBook.ID);

            return addedBook;
        }

        /// <inheritdoc/>
        public async Task<(CustomerBookOperationResult OperationResult, Book? UpdatedBook)> BorrowBookAsync(Guid bookId, string customerName)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var book = await _bookRepo.GetByIdAsync(bookId);

                if (book == null)
                {
                    await transaction.RollbackAsync();
                    return (CustomerBookOperationResult.NotFound, null);
                }

                if (book.ActualQuantity <= 0)
                {
                    await transaction.RollbackAsync();
                    return (CustomerBookOperationResult.OutOfStock, null);
                }

                book.ActualQuantity -= 1;
                _bookRepo.Update(book);

                var bookRecord = new BorrowBookRecord
                {
                    BookID = bookId,
                    Book = book,
                    CustomerName = customerName,
                    Action = BookActionType.Borrowed.ToString(),
                    Date = DateTime.UtcNow
                };

                _ = await _borrowBookRepo.AddAsync(bookRecord);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (CustomerBookOperationResult.Success, book);
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                return (CustomerBookOperationResult.Conflict, null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex,
                    "Unexpected error while borrowing book {BookId} for customer {CustomerName}",
                    bookId, customerName);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<(CustomerBookOperationResult OperationResult, Book? UpdatedBook)> ReturnBookAsync(Guid bookId, string customerName)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var book = await _bookRepo.GetByIdAsync(bookId);

                if (book == null)
                {
                    await transaction.RollbackAsync();
                    return (CustomerBookOperationResult.NotFound, null);
                }

                book.ActualQuantity += 1;
                _bookRepo.Update(book);

                var bookRecord = new BorrowBookRecord
                {
                    BookID = bookId,
                    Book = book,
                    CustomerName = customerName,
                    Action = BookActionType.Returned.ToString(),
                    Date = DateTime.UtcNow
                };

                _ = await _borrowBookRepo.AddAsync(bookRecord);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (CustomerBookOperationResult.Success, book);
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                return (CustomerBookOperationResult.Conflict, null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex,
                    "Unexpected error while returning book {BookId} from customer {CustomerName}",
                    bookId, customerName);
                throw;
            }
        }
    }
}
