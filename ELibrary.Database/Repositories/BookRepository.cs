using ELibrary.Shared.Entities;
using ELibrary.Shared.Enums;
using ELibrary.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ELibrary.Database.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly ELibraryDbContext _context;

        /// <summary>
        /// Initializes a new instance of the BookRepoository class
        /// </summary>
        /// <param name="context">The database context</param>
        /// <exception cref="ArgumentNullException">Thrown when context is null</exception>
        public BookRepository(ELibraryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Get all books in database.
        /// </summary>
        /// <returns>Collection of all books.</returns>
        public async Task<IEnumerable<Book>> GetAllAsync()
        {
            return await _context.Books
                .OrderByDescending(b => b.Author)
                .ToListAsync();
        }

        /// <summary>
        /// Get all books in database.
        /// </summary>
        /// <returns>Collection of all books.</returns>
        public async Task<Book?> GetByIdAsync(Guid id)
        {
            return await _context.Books.FindAsync(id);
        }

        /// <summary>
        /// Gets filtered books from the database based on provided criteria.
        /// </summary>
        /// <param name="name">Name of the book.</param>
        /// <param name="author">Author of the book.</param>
        /// <param name="ISBN">ISBN code of the book.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Book>> GetFilteredBooksAsync(string? name, string? author, string? ISBN)
        {
            var query = _context.Books.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(b => b.Name.Contains(name.Trim()));
            }

            if(!string.IsNullOrEmpty(author))
            {
                query = query.Where(b => b.Author.Contains(author.Trim()));
            }   

            if(!string.IsNullOrEmpty(ISBN))
            {
                query = query.Where(b => b.ISBN.Contains(ISBN.Trim()));
            }


            return await query.ToListAsync();
        }

        /// <summary>
        /// Adds a new book to the database
        /// </summary>
        /// <param name="book">The book to add</param>
        /// <returns>The added book with updated information</returns>
        public async Task<Book> AddAsync(Book book)
        {
            var entry = await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();
            return entry.Entity;
        }

        /// <summary>
        /// Updates an existing book in the database
        /// </summary>
        /// <param name="book">The book with updated information</param>
        /// <returns>The updated book with updated information</returns>
        public async Task<Book?> UpdateAsync(Book book)
        {
            try
            {
                var entry = _context.Books.Update(book);
                await _context.SaveChangesAsync();
                return entry.Entity;
            }
            catch (DbUpdateConcurrencyException)
            {
                return null;
            }
        }

        /// <summary>
        /// Decreases quantity of a book.
        /// </summary>
        /// <param name="bookId">The id of the book to borrow</param>
        /// <returns>True if success and updated borrowed book.</returns>
        public async Task<(CustomerBookOperationResult OperationResult, Book? UpdatedBook)> BorrowBookAsync(Guid bookId)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var book = await _context.Books.FindAsync(bookId);

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
                    _context.Books.Update(book);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return (CustomerBookOperationResult.Success, book);
                }
                catch (DbUpdateConcurrencyException)
                {
                    await transaction.RollbackAsync();
                    return (CustomerBookOperationResult.Conflict, null);
                }
            }
        }

        /// <summary>
        /// Increases quantity iof a book.
        /// </summary>
        /// <param name="bookId">The id of the book to return</param>
        /// <returns>True if success and updated returned book.</returns>
        public async Task<(CustomerBookOperationResult OperationResult, Book? UpdatedBook)> ReturnBookAsync(Guid bookId)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var book = await _context.Books.FindAsync(bookId);

                    if (book == null)
                    {
                        await transaction.RollbackAsync();
                        return (CustomerBookOperationResult.NotFound, null);
                    }

                    // Increment quantity
                    book.ActualQuantity += 1;
                    _context.Books.Update(book);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return (CustomerBookOperationResult.Success, book);
                }
                catch (DbUpdateConcurrencyException)
                {
                    await transaction.RollbackAsync();
                    return (CustomerBookOperationResult.Conflict, null);
                }
            }
        }
    }
}
