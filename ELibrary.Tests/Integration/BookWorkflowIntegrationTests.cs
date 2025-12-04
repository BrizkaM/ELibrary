using ELibrary.Database.Repositories;
using ELibrary.Database.Services;
using ELibrary.Shared.Enums;
using ELibrary.Shared.Interfaces;
using ELibrary.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ELibrary.Tests.Integration
{
    /// <summary>
    /// Integration tests for complete book workflow including creation, borrowing, returning, and filtering operations.
    /// </summary>
    [TestClass]
    public class BookWorkflowIntegrationTests
    {
        /// <summary>
        /// Verifies the complete book lifecycle from creation through borrowing and returning works correctly.
        /// </summary>
        [TestMethod]
        public async Task CompleteBookLifecycle_CreateBorrowReturn_ShouldWorkCorrectly()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryContext();
            var bookRepo = new BookRepository(context);
            var borrowRepo = new BorrowBookRepository(context);
            var logger = new Mock<ILogger<BookService>>().Object;
            var unitOfWork = new UnitOfWork(context, bookRepo, borrowRepo);
            var service = new BookService(unitOfWork, logger);

            // Create a new book
            var newBook = TestDataBuilder.CreateTestBook(
                name: "Integration Test Book",
                author: "Integration Author",
                isbn: "9999999999999",
                quantity: 5);

            // Act & Assert - Create Book
            var createdBook = await service.CreateBookAsync(newBook);
            createdBook.Should().NotBeNull();
            createdBook.ActualQuantity.Should().Be(5);

            // Act & Assert - Borrow Book
            var borrowResult = await service.BorrowBookAsync(createdBook.ID, "Customer 1");
            borrowResult.OperationResult.Should().Be(CustomerBookOperationResult.Success);
            borrowResult.UpdatedBook!.ActualQuantity.Should().Be(4);

            // Act & Assert - Borrow Again
            var borrowResult2 = await service.BorrowBookAsync(createdBook.ID, "Customer 2");
            borrowResult2.OperationResult.Should().Be(CustomerBookOperationResult.Success);
            borrowResult2.UpdatedBook!.ActualQuantity.Should().Be(3);

            // Act & Assert - Return Book
            var returnResult = await service.ReturnBookAsync(createdBook.ID, "Customer 1");
            returnResult.OperationResult.Should().Be(CustomerBookOperationResult.Success);
            returnResult.UpdatedBook!.ActualQuantity.Should().Be(4);

            // Verify borrow records
            var records = await borrowRepo.GetAllAsync();
            records.Should().HaveCount(3);
            records.Count(r => r.Action == "Borrowed").Should().Be(2);
            records.Count(r => r.Action == "Returned").Should().Be(1);
        }

        /// <summary>
        /// Verifies that borrowing returns OutOfStock when all copies are borrowed and no more are available.
        /// </summary>
        [TestMethod]
        public async Task BorrowAllCopies_ThenTryBorrowAgain_ShouldReturnOutOfStock()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryContext();
            var bookRepo = new BookRepository(context);
            var borrowRepo = new BorrowBookRepository(context);
            var logger = new Mock<ILogger<BookService>>().Object;
            var unitOfWork = new UnitOfWork(context, bookRepo, borrowRepo);
            var service = new BookService(unitOfWork, logger);

            var newBook = TestDataBuilder.CreateTestBook(quantity: 2);
            var createdBook = await service.CreateBookAsync(newBook);

            // Act - Borrow all copies
            await service.BorrowBookAsync(createdBook.ID, "Customer 1");
            await service.BorrowBookAsync(createdBook.ID, "Customer 2");

            // Try to borrow when out of stock
            var outOfStockResult = await service.BorrowBookAsync(createdBook.ID, "Customer 3");

            // Assert
            outOfStockResult.OperationResult.Should().Be(CustomerBookOperationResult.OutOfStock);
            outOfStockResult.UpdatedBook.Should().BeNull();

            var book = await bookRepo.GetByIdAsync(createdBook.ID);
            book!.ActualQuantity.Should().Be(0);
        }

        /// <summary>
        /// Verifies that multiple customers can borrow and return books simultaneously while maintaining data integrity.
        /// </summary>
        [TestMethod]
        public async Task MultipleCustomers_BorrowAndReturnSimultaneously_ShouldMaintainDataIntegrity()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryContext();
            var bookRepo = new BookRepository(context);
            var borrowRepo = new BorrowBookRepository(context);
            var logger = new Mock<ILogger<BookService>>().Object;
            var unitOfWork = new UnitOfWork(context, bookRepo, borrowRepo);
            var service = new BookService(unitOfWork, logger);

            var newBook = TestDataBuilder.CreateTestBook(quantity: 10);
            var createdBook = await service.CreateBookAsync(newBook);

            // Act - Multiple operations
            await service.BorrowBookAsync(createdBook.ID, "Customer 1");
            await service.BorrowBookAsync(createdBook.ID, "Customer 2");
            await service.BorrowBookAsync(createdBook.ID, "Customer 3");
            await service.ReturnBookAsync(createdBook.ID, "Customer 1");
            await service.BorrowBookAsync(createdBook.ID, "Customer 4");
            await service.ReturnBookAsync(createdBook.ID, "Customer 2");

            // Assert
            var finalBook = await bookRepo.GetByIdAsync(createdBook.ID);
            finalBook!.ActualQuantity.Should().Be(8); // 10 - 4 borrowed + 2 returned

            var records = await borrowRepo.GetAllAsync();
            records.Should().HaveCount(6);
            records.Count(r => r.Action == "Borrowed").Should().Be(4);
            records.Count(r => r.Action == "Returned").Should().Be(2);
        }

        /// <summary>
        /// Verifies that book filtering by author, name, and ISBN returns correct results after creating multiple books.
        /// </summary>
        [TestMethod]
        public async Task FilterBooks_AfterCreatingMultiple_ShouldReturnCorrectResults()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryContext();
            var bookRepo = new BookRepository(context);
            var borrowRepo = new BorrowBookRepository(context);
            var logger = new Mock<ILogger<BookService>>().Object;
            var unitOfWork = new UnitOfWork(context, bookRepo, borrowRepo);
            var service = new BookService(unitOfWork, logger);

            // Create multiple books
            await service.CreateBookAsync(TestDataBuilder.CreateTestBook(
                name: "Fantasy Book 1", author: "Fantasy Author", isbn: "1111111111111"));
            await service.CreateBookAsync(TestDataBuilder.CreateTestBook(
                name: "Fantasy Book 2", author: "Fantasy Author", isbn: "2222222222222"));
            await service.CreateBookAsync(TestDataBuilder.CreateTestBook(
                name: "SciFi Book", author: "SciFi Author", isbn: "3333333333333"));

            // Act & Assert - Filter by author
            var fantasyBooks = await bookRepo.GetFilteredBooksAsync(null, "Fantasy Author", null);
            fantasyBooks.Should().HaveCount(2);

            // Act & Assert - Filter by name
            var scifiBooks = await bookRepo.GetFilteredBooksAsync("SciFi", null, null);
            scifiBooks.Should().HaveCount(1);

            // Act & Assert - Filter by ISBN
            var specificBook = await bookRepo.GetFilteredBooksAsync(null, null, "1111111111111");
            specificBook.Should().HaveCount(1);
            specificBook.First().Name.Should().Be("Fantasy Book 1");
        }

        /// <summary>
        /// Verifies that attempting to create a book with a duplicate ISBN throws an ArgumentException.
        /// </summary>
        [TestMethod]
        public async Task CreateBookWithDuplicateISBN_ShouldThrowException()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryContext();
            var bookRepo = new BookRepository(context);
            var borrowRepo = new BorrowBookRepository(context);
            var logger = new Mock<ILogger<BookService>>().Object;
            var unitOfWork = new UnitOfWork(context, bookRepo, borrowRepo);
            var service = new BookService(unitOfWork, logger);

            var book1 = TestDataBuilder.CreateTestBook(isbn: "DUPLICATE123");
            await service.CreateBookAsync(book1);

            var book2 = TestDataBuilder.CreateTestBook(
                name: "Different Book",
                isbn: "DUPLICATE123");

            // Act
            Func<Task> act = async () => await service.CreateBookAsync(book2);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Book with the same ISBN already exists*");
        }

        /// <summary>
        /// Verifies that borrow records are returned ordered by date in descending order (newest first).
        /// </summary>
        [TestMethod]
        public async Task BorrowRecords_ShouldBeOrderedByDateDescending()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryContext();
            var bookRepo = new BookRepository(context);
            var borrowRepo = new BorrowBookRepository(context);
            var logger = new Mock<ILogger<BookService>>().Object;
            var unitOfWork = new UnitOfWork(context, bookRepo, borrowRepo);
            var service = new BookService(unitOfWork, logger);

            var book = TestDataBuilder.CreateTestBook(quantity: 5);
            var createdBook = await service.CreateBookAsync(book);

            // Act - Create records over time
            await service.BorrowBookAsync(createdBook.ID, "Customer 1");
            await Task.Delay(10); // Small delay to ensure different timestamps
            await service.BorrowBookAsync(createdBook.ID, "Customer 2");
            await Task.Delay(10);
            await service.ReturnBookAsync(createdBook.ID, "Customer 1");

            // Assert
            var records = (await borrowRepo.GetAllAsync()).ToList();
            records.Should().HaveCount(3);
            records.Should().BeInDescendingOrder(r => r.Date);
            records[0].Action.Should().Be("Returned"); // Most recent
        }

        /// <summary>
        /// Verifies that GetAllBooks returns books ordered by author in descending alphabetical order.
        /// </summary>
        [TestMethod]
        public async Task GetAllBooks_ShouldBeOrderedByAuthorDescending()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryContext();
            var bookRepo = new BookRepository(context);
            var borrowRepo = new BorrowBookRepository(context);
            var logger = new Mock<ILogger<BookService>>().Object;
            var unitOfWork = new UnitOfWork(context, bookRepo, borrowRepo);
            var service = new BookService(unitOfWork, logger);

            // Create books with different authors
            await service.CreateBookAsync(TestDataBuilder.CreateTestBook(
                author: "Alpha Author", isbn: "1111111111111"));
            await service.CreateBookAsync(TestDataBuilder.CreateTestBook(
                author: "Zulu Author", isbn: "2222222222222"));
            await service.CreateBookAsync(TestDataBuilder.CreateTestBook(
                author: "Beta Author", isbn: "3333333333333"));

            // Act
            var allBooks = (await bookRepo.GetAllAsync()).ToList();

            // Assert
            allBooks.Should().HaveCount(3);
            allBooks.Should().BeInDescendingOrder(b => b.Author);
            allBooks[0].Author.Should().Be("Zulu Author");
            allBooks[1].Author.Should().Be("Beta Author");
            allBooks[2].Author.Should().Be("Alpha Author");
        }
    }
}