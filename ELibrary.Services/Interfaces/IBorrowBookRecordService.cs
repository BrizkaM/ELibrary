using ELibrary.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ELibrary.Services.Interfaces
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
