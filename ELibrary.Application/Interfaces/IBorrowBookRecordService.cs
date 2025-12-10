using ELibrary.Application.Common;
using ELibrary.Application.DTOs;
using ELibrary.Application.Queries.BorrowRecords;

namespace ELibrary.Application.Interfaces
{
    /// <summary>
    /// Borrow book record service interface using CQRS pattern.
    /// Currently only supports query operations (read-only).
    /// </summary>
    public interface IBorrowBookRecordService
    {
        /// <summary>
        /// Handles query to get all borrow/return records.
        /// </summary>
        /// <param name="query">The get all borrow records query</param>
        /// <returns>Result containing collection of all borrow records</returns>
        Task<ELibraryResult<IEnumerable<BorrowBookRecordDto>>> HandleAsync(GetAllBorrowRecordsQuery query);
    }
}