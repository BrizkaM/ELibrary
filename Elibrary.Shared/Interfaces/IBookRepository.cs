using ELibrary.Shared.Entities;
using ELibrary.Shared.Enums;

namespace ELibrary.Shared.Interfaces
{
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> GetAllAsync();

        Task<Book?> GetByIdAsync(Guid id);

        Task<IEnumerable<Book>> GetFilteredBooksAsync(string? name, string? author, string? isbn);

        Task<Book> AddAsync(Book book);

        Task<(CustomerBookOperationResult OperationResult, Book? UpdatedBook)> BorrowBookAsync(Guid bookId);

        Task<(CustomerBookOperationResult OperationResult, Book? UpdatedBook)> ReturnBookAsync(Guid bookId);
    }
}
