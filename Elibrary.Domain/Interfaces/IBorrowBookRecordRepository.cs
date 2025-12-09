using ELibrary.Domain.Entities;

namespace ELibrary.Domain.Interfaces
{
    /// <summary>
    /// Interface for Borrow Book Record Repository.
    /// </summary>
    public interface IBorrowBookRecordRepository
    {
        /// <summary>
        /// Gets all borrow book records asynchronously.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<BorrowBookRecord>> GetAllAsync();

        /// <summary>
        /// Adds a new borrow book record asynchronously.
        /// </summary>
        /// <param name="book">The borrow book record.</param>
        /// <returns>Added borrow book record.</returns>
        Task<BorrowBookRecord> AddAsync(BorrowBookRecord book);
    }
}
