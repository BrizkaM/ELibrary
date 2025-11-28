using ELibrary.Database.Repositories;
using ELibrary.Shared.Entities;
using ELibrary.Tests.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ELibrary.Tests.Repositories
{
    [TestClass]
    public class BorrowBookRepositoryTests
    {
        [TestMethod]
        public async Task GetAllAsync_ShouldReturnAllRecords_OrderedByDateDescending()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateSeededContext();
            var repository = new BorrowBookRepository(context);
            var book = context.Books.First();

            var record1 = TestDataBuilder.CreateBorrowRecord(book, "Customer 1", "Borrowed");
            record1.Date = DateTime.UtcNow.AddDays(-2);
            context.BorrowBookRecords.Add(record1);

            var record2 = TestDataBuilder.CreateBorrowRecord(book, "Customer 2", "Returned");
            record2.Date = DateTime.UtcNow.AddDays(-1);
            context.BorrowBookRecords.Add(record2);

            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeInDescendingOrder(r => r.Date);
            result.First().CustomerName.Should().Be("Customer 2");
        }

        [TestMethod]
        public async Task GetAllAsync_WithNoRecords_ShouldReturnEmptyList()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryContext();
            var repository = new BorrowBookRepository(context);

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [TestMethod]
        public async Task AddAsync_ShouldAddNewRecord()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateSeededContext();
            var repository = new BorrowBookRepository(context);
            var book = context.Books.First();
            var newRecord = TestDataBuilder.CreateBorrowRecord(book, "Test Customer", "Borrowed");

            // Act
            var result = await repository.AddAsync(newRecord);
            await context.SaveChangesAsync();

            // Assert
            result.Should().NotBeNull();
            result.ID.Should().Be(newRecord.ID);
            result.CustomerName.Should().Be("Test Customer");
            
            var allRecords = await repository.GetAllAsync();
            allRecords.Should().Contain(r => r.ID == newRecord.ID);
        }

        [TestMethod]
        public async Task AddAsync_WithBorrowedAction_ShouldCreateCorrectRecord()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateSeededContext();
            var repository = new BorrowBookRepository(context);
            var book = context.Books.First();
            var record = TestDataBuilder.CreateBorrowRecord(book, "John Doe", "Borrowed");

            // Act
            var result = await repository.AddAsync(record);
            await context.SaveChangesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Action.Should().Be("Borrowed");
            result.CustomerName.Should().Be("John Doe");
            result.BookID.Should().Be(book.ID);
        }

        [TestMethod]
        public async Task AddAsync_WithReturnedAction_ShouldCreateCorrectRecord()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateSeededContext();
            var repository = new BorrowBookRepository(context);
            var book = context.Books.First();
            var record = TestDataBuilder.CreateBorrowRecord(book, "Jane Doe", "Returned");

            // Act
            var result = await repository.AddAsync(record);
            await context.SaveChangesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Action.Should().Be("Returned");
            result.CustomerName.Should().Be("Jane Doe");
        }

        [TestMethod]
        public void Constructor_WithNullContext_ShouldThrowArgumentNullException()
        {
            // Act
            Action act = () => new BorrowBookRepository(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("context");
        }
    }
}
