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
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task<IEnumerable<BorrowBookRecord>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a new borrow book record asynchronously.
        /// </summary>
        /// <param name="book">The borrow book record.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Added borrow book record.</returns>
        Task<BorrowBookRecord> AddAsync(BorrowBookRecord book, CancellationToken cancellationToken = default);
    }
}
