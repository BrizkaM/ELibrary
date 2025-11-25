using ELibrary.Shared.Entities;

namespace ELibrary.Shared.Interfaces
{
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> GetAllAsync();

        Task<IEnumerable<Book>> GetFilteredBooksAsync(string? name, string? author, string? isbn);

        Task<Book> AddAsync(Book book);

        void Update(Book book);
    }
}
