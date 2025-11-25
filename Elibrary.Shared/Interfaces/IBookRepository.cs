using ELibrary.Shared.Entities;

namespace ELibrary.Shared.Interfaces
{
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> GetAllAsync();

        Task<Book> GetByIdAsync(Guid id);

        Task<IEnumerable<Book>> GetFilteredBooksAsync(string? name, string? author, string? isbn);

        Task<Book> AddAsync(Book book);

        Task<(bool Success, Book? UpdatedBook)> UpdateAsync(Book book, DateTime? udate);
    }
}
