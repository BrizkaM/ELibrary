using ELibrary.Shared.Entities;
using ELibrary.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ELibrary.Database.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly ELibraryDbContext _context;

        /// <summary>
        /// Initializes a new instance of the BookRepository class
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
        /// Get book by Id.
        /// </summary>
        /// <returns>Collection of all books.</returns>
        public async Task<Book?> GetByIdAsync(Guid id)
        {
            return await _context.Books.FindAsync(id);
        }

        /// <summary>
        /// Get book by ISBN.
        /// </summary>
        /// <returns>Collection of all books.</returns>
        public async Task<Book?> GetByISBNAsync(string isbn)
        {
            return await _context.Books
                .FirstOrDefaultAsync(b => b.ISBN == isbn);
        }

        /// <summary>
        /// Gets filtered books from the database based on provided criteria.
        /// </summary>
        /// <param name="name">Name of the book.</param>
        /// <param name="author">Author of the book.</param>
        /// <param name="isbn">ISBN code of the book.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Book>> GetFilteredBooksAsync(string? name, string? author, string? isbn)
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

            if(!string.IsNullOrEmpty(isbn))
            {
                query = query.Where(b => b.ISBN.Contains(isbn.Trim()));
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
            return entry.Entity;
        }

        /// <summary>
        /// Updates an existing book in the database
        /// </summary>
        /// <param name="book">The book with updated information</param>
        /// <returns>The updated book with updated information</returns>
        public void Update(Book book)
        {
            _ = _context.Books.Update(book);
        }
    }
}
