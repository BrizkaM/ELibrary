using ELibrary.Domain.DTOs;

namespace ELibrary.Application.Interfaces
{
    public interface IBorrowBookRecordService
    {
        /// <summary>
        /// Gets all borrow book records asynchronously.
        /// </summary>
        /// <returns>All borrow books records.</returns>
        Task<IEnumerable<BorrowBookRecordDto>> GetAllBorrowBookRecordsAsync();
    }
}
