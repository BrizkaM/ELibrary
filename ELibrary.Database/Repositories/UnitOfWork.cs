using ELibrary.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace ELibrary.Database.Repositories
{
    /// <summary>
    /// Unit of Work implementation that coordinates the work of multiple repositories
    /// and manages database transactions.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ELibraryDbContext _context;
        private IDbContextTransaction? _transaction;
        private bool _disposed = false;

        /// <summary>
        /// Gets the book repository.
        /// </summary>
        public IBookRepository Books { get; }

        /// <summary>
        /// Gets the borrow book record repository.
        /// </summary>
        public IBorrowBookRecordRepository BorrowRecords { get; }

        /// <summary>
        /// Initializes a new instance of the UnitOfWork class.
        /// </summary>
        /// <param name="context">The database context</param>
        /// <param name="bookRepository">The book repository</param>
        /// <param name="borrowBookRecordRepository">The borrow book record repository</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
        public UnitOfWork(
            ELibraryDbContext context,
            IBookRepository bookRepository,
            IBorrowBookRecordRepository borrowBookRecordRepository)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Books = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
            BorrowRecords = borrowBookRecordRepository ?? throw new ArgumentNullException(nameof(borrowBookRecordRepository));
        }

        /// <summary>
        /// Saves all changes made in this unit of work to the database.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The number of state entries written to the database</returns>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Begins a new database transaction.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress.");
            }

            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        /// <summary>
        /// Commits the current transaction, persisting all changes to the database.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction is in progress.");
            }

            try
            {
                await _transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        /// <summary>
        /// Rolls back the current transaction, discarding all changes.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction is in progress.");
            }

            try
            {
                await _transaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        /// <summary>
        /// Disposes the unit of work and releases all resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the unit of work and releases all resources.
        /// </summary>
        /// <param name="disposing">True if disposing, false if finalizing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    // Note: We don't dispose _context here as it's managed by DI container
                }

                _disposed = true;
            }
        }
    }
}