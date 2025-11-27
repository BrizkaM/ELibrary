using ELibrary.Shared.Entities;
using ELibrary.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ELibrary.Database.Repositories
{
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

        /// <summary>
        /// Get all book records in database.
        /// </summary>
        /// <returns>Collection of all book records.</returns>
        public async Task<IEnumerable<BorrowBookRecord>> GetAllAsync()
        {
            return await _context.BorrowBookRecords
                .OrderByDescending(b => b.Date)
                .ToListAsync();
        }

        /// <summary>
        /// Adds a new book record to the database.
        /// </summary>
        /// <param name="bookRecord">The book record to add</param>
        /// <returns>The added book record with updated information</returns>
        public async Task<BorrowBookRecord> AddAsync(BorrowBookRecord bookRecord)
        {
            var entry = await _context.BorrowBookRecords.AddAsync(bookRecord);
            return entry.Entity;
        }
    }
}
