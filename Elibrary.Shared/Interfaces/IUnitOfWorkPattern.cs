namespace ELibrary.Shared.Interfaces
{
    /// <summary>
    /// Unit of Work interface that coordinates the work of multiple repositories
    /// and ensures that all changes are persisted together as a single transaction.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Gets the book repository.
        /// </summary>
        IBookRepository Books { get; }

        /// <summary>
        /// Gets the borrow book record repository.
        /// </summary>
        IBorrowBookRecordRepository BorrowRecords { get; }

        /// <summary>
        /// Saves all changes made in this unit of work to the database.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The number of state entries written to the database</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Begins a new database transaction.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Commits the current transaction, persisting all changes to the database.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rolls back the current transaction, discarding all changes.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}