using ELibrary.Domain.Entities;
using ELibrary.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ELibrary.Infrastructure.Repositories
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

        /// <inheritdoc/>
        public async Task<IEnumerable<Book>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Books
                .OrderByDescending(b => b.Author)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<Book?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Books.FindAsync(id, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<Book?> GetByISBNAsync(
            string isbn,
            CancellationToken cancellationToken = default)
        {
            return await _context.Books
                .FirstOrDefaultAsync(b => b.ISBN == isbn, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Book>> GetFilteredBooksAsync(
            string? name,
            string? author,
            string? isbn,
            CancellationToken cancellationToken = default)
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


            return await query.ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<Book> AddAsync(Book book, CancellationToken cancellationToken = default)
        {
            var entry = await _context.Books.AddAsync(book, cancellationToken);
            return entry.Entity;
        }

        /// <inheritdoc/>
        public void Update(Book book)
        {
            _ = _context.Books.Update(book);
        }
    }
}
