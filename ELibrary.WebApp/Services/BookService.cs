using ELibrary.Database;
using ELibrary.Shared.Entities;
using ELibrary.Shared.Enums;
using ELibrary.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ELibrary.WebApp.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepo;
        private readonly IBorrowBookRecordRepository _borrowBookRepo;
        private readonly ILogger<BookService> _logger;
        private readonly ELibraryDbContext _context;

        public BookService(IBookRepository bookRepo, IBorrowBookRecordRepository borrowBookRepo, ILogger<BookService> logger, ELibraryDbContext context)
        {
            _bookRepo = bookRepo;
            _borrowBookRepo = borrowBookRepo;
            _logger = logger;
            _context = context;
        }

        public async Task<Book> CreateBookAsync(Book book)
        {
            try
            {
                book.ID = Guid.NewGuid();
                book.RowVersion = 0;

                var addedBook = await _bookRepo.AddAsync(book);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Book created successfully with ID: {BookId}", addedBook.ID);

                return addedBook;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating book");
                throw;
            }
        }

        public async Task<(CustomerBookOperationResult OperationResult, Book? UpdatedBook)> BorrowBookAsync(Guid bookId, string customerName)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var book = await _bookRepo.GetByIdAsync(bookId);

                if (book == null)
                {
                    return (CustomerBookOperationResult.NotFound, null);
                }

                if (book.ActualQuantity <= 0)
                {
                    return (CustomerBookOperationResult.OutOfStock, null);
                }

                book.ActualQuantity -= 1;
                _bookRepo.Update(book);

                var bookRecord = new BorrowBookRecord
                {
                    BookID = bookId,
                    Book = book,
                    CustomerName = customerName,
                    Action = "Borrowed",
                    Date = DateTime.UtcNow
                };

                _ = await _borrowBookRepo.AddAsync(bookRecord);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (CustomerBookOperationResult.Success, book);
            }
            catch (DbUpdateConcurrencyException)
            {
                return (CustomerBookOperationResult.Conflict, null); ;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error while borrowing book {BookId} for customer {CustomerName}",
                    bookId, customerName);
                throw;
            }
        }

        public async Task<(CustomerBookOperationResult OperationResult, Book? UpdatedBook)> ReturnBookAsync(Guid bookId, string customerName)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var book = await _bookRepo.GetByIdAsync(bookId);

                if (book == null)
                {
                    return (CustomerBookOperationResult.NotFound, null);
                }

                book.ActualQuantity += 1;
                _bookRepo.Update(book);

                var bookRecord = new BorrowBookRecord
                {
                    BookID = bookId,
                    Book = book,
                    CustomerName = customerName,
                    Action = "Returned",
                    Date = DateTime.UtcNow
                };

                _ = await _borrowBookRepo.AddAsync(bookRecord);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (CustomerBookOperationResult.Success, book);
            }
            catch (DbUpdateConcurrencyException)
            {
                return (CustomerBookOperationResult.Conflict, null); ;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error while returning book {BookId} from customer {CustomerName}",
                    bookId, customerName);
                throw;
            }
        }
    }
}
