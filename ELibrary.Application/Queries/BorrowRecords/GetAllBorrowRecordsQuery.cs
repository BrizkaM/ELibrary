using ELibrary.Application.Common;
using ELibrary.Application.DTOs;
using MediatR;

namespace ELibrary.Application.Queries.BorrowRecords.GetAllBorrowRecords
{
    /// <summary>
    /// Query for retrieving all borrow/return records from the library.
    /// This is a read-only operation with no side effects.
    /// Returns the complete history of book borrowing and returning activities.
    /// Implements IRequest to work with MediatR pipeline.
    /// </summary>
    public record GetAllBorrowRecordsQuery() : IRequest<ELibraryResult<IEnumerable<BorrowBookRecordDto>>>;
}
