using ELibrary.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ELibrary.Tests.Helpers
{
    /// <summary>
    /// Factory for creating in-memory database contexts for testing
    /// </summary>
    public static class TestDbContextFactory
    {
        /// <summary>
        /// Creates a new in-memory SQLite database context with a unique connection
        /// </summary>
        public static ELibraryDbContext CreateInMemoryContext()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ELibraryDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new ELibraryDbContext(options);
            context.Database.EnsureCreated();

            // Vymazat seed data z OnModelCreating
            context.Books.RemoveRange(context.Books);
            context.BorrowBookRecords.RemoveRange(context.BorrowBookRecords);
            context.SaveChanges();

            return context;
        }

        /// <summary>
        /// Creates a context and seeds it with default test data
        /// </summary>
        public static ELibraryDbContext CreateSeededContext()
        {
            var context = CreateInMemoryContext();
            SeedTestData(context);
            return context;
        }

        /// <summary>
        /// Seeds the context with test data
        /// </summary>
        private static void SeedTestData(ELibraryDbContext context)
        {
            var books = TestDataBuilder.CreateTestBooks();
            context.Books.AddRange(books);
            context.SaveChanges();
        }
    }
}