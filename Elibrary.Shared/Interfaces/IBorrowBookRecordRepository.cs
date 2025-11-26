using ELibrary.Shared.Entities;

namespace ELibrary.Shared.Interfaces
{
    public interface IBorrowBookRecordRepository
    {
        Task<IEnumerable<BorrowBookRecord>> GetAllAsync();

        Task<BorrowBookRecord> AddAsync(BorrowBookRecord book);
    }
}
