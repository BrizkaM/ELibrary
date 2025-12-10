namespace ELibrary.Application.Queries.BorrowRecords
{
    /// <summary>
    /// Query for retrieving all borrow/return records from the library.
    /// This is a read-only operation with no side effects.
    /// Returns the complete history of book borrowing and returning activities.
    /// </summary>
    public record GetAllBorrowRecordsQuery();
}