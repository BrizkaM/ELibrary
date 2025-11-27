using ELibrary.Shared.Entities;
using ELibrary.Shared.Enums;

namespace ELibrary.Shared.Interfaces
{
    public interface IBookService
    {
        Task<(CustomerBookOperationResult OperationResult, Book? UpdatedBook)> BorrowBookAsync(Guid bookId, string customerName);

        Task<(CustomerBookOperationResult OperationResult, Book? UpdatedBook)> ReturnBookAsync(Guid bookId, string customerName);
    }
}
