using ELibrary.Application.Common;
using ELibrary.Application.DTOs;

namespace ELibrary.Application.Interfaces
{
    public interface IBorrowBookRecordService
    {
        /// <summary>
        /// Gets all borrow book records asynchronously.
        /// </summary>
        /// <returns>Result containing all borrow book records.</returns>
        Task<ELibraryResult<IEnumerable<BorrowBookRecordDto>>> GetAllBorrowBookRecordsAsync();
    }
}