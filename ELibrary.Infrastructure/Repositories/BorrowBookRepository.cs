using ELibrary.Domain.Entities;
using ELibrary.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ELibrary.Infrastructure.Repositories
{
    /// <summary>
    /// Borrow book repository implementation.
    /// </summary>
    public class BorrowBookRepository : IBorrowBookRecordRepository
    {
        private readonly ELibraryDbContext _context;

        /// <summary>
        /// Initializes a new instance of the BorrowBookRepository class
        /// </summary>
        /// <param name="context">The database context</param>
        /// <exception cref="ArgumentNullException">Thrown when context is null</exception>
        public BorrowBookRepository(ELibraryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<BorrowBookRecord>> GetAllAsync()
        {
            return await _context.BorrowBookRecords
                .OrderByDescending(b => b.Date)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<BorrowBookRecord> AddAsync(BorrowBookRecord bookRecord)
        {
            var entry = await _context.BorrowBookRecords.AddAsync(bookRecord);
            return entry.Entity;
        }
    }
}
