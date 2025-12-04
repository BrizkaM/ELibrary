using ELibrary.Database.Repositories;
using ELibrary.Database.Services;
using ELibrary.Shared.Enums;
using ELibrary.Shared.Interfaces;
using ELibrary.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ELibrary.Tests.Services
{
    /// <summary>
    /// Tests for BookService business logic including book creation, borrowing, returning, and validation.
    /// </summary>
    [TestClass]
    public class BookServiceTests
    {
        private Mock<ILogger<BookService>> _loggerMock = null!;

        [TestInitialize]
        public void Initialize()
        {
            _loggerMock = new Mock<ILogger<BookService>>();
        }

        /// <summary>
        /// Verifies that CreateBookAsync successfully creates a book when all validation passes.
        /// </summary>
        [TestMethod]
        public async Task CreateBookAsync_WithValidBook_ShouldCreateBook()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryContext();
            var bookRepo = new BookRepository(context);
            var borrowRepo = new BorrowBookRepository(context);
            var unitOfWork = new UnitOfWork(context, bookRepo, borrowRepo);
            var service = new BookService(unitOfWork, _loggerMock.Object);

            var newBook = TestDataBuilder.CreateTestBook(
                name: "New Book",
                author: "New Author",
                isbn: "9876543210123",
                quantity: 10);

            // Act
            var result = await service.CreateBookAsync(newBook);

            // Assert
            result.Should().NotBeNull();
            result.ID.Should().NotBeEmpty();
            result.Name.Should().Be("New Book");
            result.ActualQuantity.Should().Be(10);

            var bookInDb = await bookRepo.GetByIdAsync(result.ID);
            bookInDb.Should().NotBeNull();
        }

        /// <summary>
        /// Verifies that CreateBookAsync throws ArgumentException when quantity is negative.
        /// </summary>
        [TestMethod]
        public async Task CreateBookAsync_WithNegativeQuantity_ShouldThrowArgumentException()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryContext();
            var bookRepo = new BookRepository(context);
            var borrowRepo = new BorrowBookRepository(context);
            var unitOfWork = new UnitOfWork(context, bookRepo, borrowRepo);
            var service = new BookService(unitOfWork, _loggerMock.Object);

            var newBook = TestDataBuilder.CreateTestBook(quantity: -1);

            // Act
            Func<Task> act = async () => await service.CreateBookAsync(newBook);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Actual quantity cannot be negative*")
                .WithParameterName("ActualQuantity");
        }

        /// <summary>
        /// Verifies that CreateBookAsync throws ArgumentException when the publication year is in the future.
        /// </summary>
        [TestMethod]
        public async Task CreateBookAsync_WithFutureYear_ShouldThrowArgumentException()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryContext();
            var bookRepo = new BookRepository(context);
            var borrowRepo = new BorrowBookRepository(context);
            var unitOfWork = new UnitOfWork(context, bookRepo, borrowRepo);
            var service = new BookService(unitOfWork, _loggerMock.Object);

            var newBook = TestDataBuilder.CreateTestBook();
            newBook.Year = DateTime.UtcNow.AddYears(1);

            // Act
            Func<Task> act = async () => await service.CreateBookAsync(newBook);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Year cannot be in the future*")
                .WithParameterName("Year");
        }

        /// <summary>
        /// Verifies that CreateBookAsync throws ArgumentException when attempting to create a book with a duplicate ISBN.
        /// </summary>
        [TestMethod]
        public async Task CreateBookAsync_WithDuplicateISBN_ShouldThrowArgumentException()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateSeededContext();
            var bookRepo = new BookRepository(context);
            var borrowRepo = new BorrowBookRepository(context);
            var unitOfWork = new UnitOfWork(context, bookRepo, borrowRepo);
            var service = new BookService(unitOfWork, _loggerMock.Object);

            var newBook = TestDataBuilder.CreateTestBook(isbn: "1234567890123"); // Duplicate ISBN

            // Act
            Func<Task> act = async () => await service.CreateBookAsync(newBook);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Book with the same ISBN already exists*")
                .WithParameterName("ISBN");
        }

        /// <summary>
        /// Verifies that BorrowBookAsync decrements book quantity and creates a borrow record when successful.
        /// </summary>
        [TestMethod]
        public async Task BorrowBookAsync_WithValidBook_ShouldDecrementQuantityAndCreateRecord()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateSeededContext();
            var bookRepo = new BookRepository(context);
            var borrowRepo = new BorrowBookRepository(context);
            var unitOfWork = new UnitOfWork(context, bookRepo, borrowRepo);
            var service = new BookService(unitOfWork, _loggerMock.Object);

            var bookId = TestDataBuilder.TestBookId1;
            var originalQuantity = (await bookRepo.GetByIdAsync(bookId))!.ActualQuantity;

            // Act
            var result = await service.BorrowBookAsync(bookId, "John Doe");

            // Assert
            result.OperationResult.Should().Be(CustomerBookOperationResult.Success);
            result.UpdatedBook.Should().NotBeNull();
            result.UpdatedBook!.ActualQuantity.Should().Be(originalQuantity - 1);

            var records = await borrowRepo.GetAllAsync();
            records.Should().Contain(r =>
                r.BookID == bookId &&
                r.CustomerName == "John Doe" &&
                r.Action == "Borrowed");
        }

        /// <summary>
        /// Verifies that BorrowBookAsync returns NotFound when attempting to borrow a non-existent book.
        /// </summary>
        [TestMethod]
        public async Task BorrowBookAsync_WithNonExistentBook_ShouldReturnNotFound()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryContext();
            var bookRepo = new BookRepository(context);
            var borrowRepo = new BorrowBookRepository(context);
            var unitOfWork = new UnitOfWork(context, bookRepo, borrowRepo);
            var service = new BookService(unitOfWork, _loggerMock.Object);

            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await service.BorrowBookAsync(nonExistentId, "John Doe");

            // Assert
            result.OperationResult.Should().Be(CustomerBookOperationResult.NotFound);
            result.UpdatedBook.Should().BeNull();
        }

        /// <summary>
        /// Verifies that BorrowBookAsync returns OutOfStock when attempting to borrow a book with zero quantity.
        /// </summary>
        [TestMethod]
        public async Task BorrowBookAsync_WithOutOfStockBook_ShouldReturnOutOfStock()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateSeededContext();
            var bookRepo = new BookRepository(context);
            var borrowRepo = new BorrowBookRepository(context);
            var unitOfWork = new UnitOfWork(context, bookRepo, borrowRepo);
            var service = new BookService(unitOfWork, _loggerMock.Object);

            var bookId = TestDataBuilder.TestBookId3; // This book has 0 quantity

            // Act
            var result = await service.BorrowBookAsync(bookId, "John Doe");

            // Assert
            result.OperationResult.Should().Be(CustomerBookOperationResult.OutOfStock);
            result.UpdatedBook.Should().BeNull();
        }

        /// <summary>
        /// Verifies that ReturnBookAsync increments book quantity and creates a return record when successful.
        /// </summary>
        [TestMethod]
        public async Task ReturnBookAsync_WithValidBook_ShouldIncrementQuantityAndCreateRecord()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateSeededContext();
            var bookRepo = new BookRepository(context);
            var borrowRepo = new BorrowBookRepository(context);
            var unitOfWork = new UnitOfWork(context, bookRepo, borrowRepo);
            var service = new BookService(unitOfWork, _loggerMock.Object);

            var bookId = TestDataBuilder.TestBookId1;
            var originalQuantity = (await bookRepo.GetByIdAsync(bookId))!.ActualQuantity;

            // Act
            var result = await service.ReturnBookAsync(bookId, "Jane Doe");

            // Assert
            result.OperationResult.Should().Be(CustomerBookOperationResult.Success);
            result.UpdatedBook.Should().NotBeNull();
            result.UpdatedBook!.ActualQuantity.Should().Be(originalQuantity + 1);

            var records = await borrowRepo.GetAllAsync();
            records.Should().Contain(r =>
                r.BookID == bookId &&
                r.CustomerName == "Jane Doe" &&
                r.Action == "Returned");
        }

        /// <summary>
        /// Verifies that ReturnBookAsync returns NotFound when attempting to return a non-existent book.
        /// </summary>
        [TestMethod]
        public async Task ReturnBookAsync_WithNonExistentBook_ShouldReturnNotFound()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryContext();
            var bookRepo = new BookRepository(context);
            var borrowRepo = new BorrowBookRepository(context);
            var unitOfWork = new UnitOfWork(context, bookRepo, borrowRepo);
            var service = new BookService(unitOfWork, _loggerMock.Object);

            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await service.ReturnBookAsync(nonExistentId, "Jane Doe");

            // Assert
            result.OperationResult.Should().Be(CustomerBookOperationResult.NotFound);
            result.UpdatedBook.Should().BeNull();
        }

        /// <summary>
        /// Verifies that ReturnBookAsync creates a return record with the correct customer name and action.
        /// </summary>
        [TestMethod]
        public async Task ReturnBookAsync_ShouldCreateReturnRecord()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateSeededContext();
            var bookRepo = new BookRepository(context);
            var borrowRepo = new BorrowBookRepository(context);
            var unitOfWork = new UnitOfWork(context, bookRepo, borrowRepo);
            var service = new BookService(unitOfWork, _loggerMock.Object);

            var bookId = TestDataBuilder.TestBookId2;

            // Act
            var result = await service.ReturnBookAsync(bookId, "Customer Name");

            // Assert
            result.OperationResult.Should().Be(CustomerBookOperationResult.Success);

            var records = await borrowRepo.GetAllAsync();
            var returnRecord = records.FirstOrDefault(r =>
                r.BookID == bookId &&
                r.Action == "Returned");

            returnRecord.Should().NotBeNull();
            returnRecord!.CustomerName.Should().Be("Customer Name");
        }

        /// <summary>
        /// Verifies that multiple customers can borrow the same book sequentially while maintaining correct quantity.
        /// </summary>
        [TestMethod]
        public async Task BorrowBookAsync_MultipleCustomers_ShouldMaintainCorrectQuantity()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateSeededContext();
            var bookRepo = new BookRepository(context);
            var borrowRepo = new BorrowBookRepository(context);
            var unitOfWork = new UnitOfWork(context, bookRepo, borrowRepo);
            var service = new BookService(unitOfWork, _loggerMock.Object);

            var bookId = TestDataBuilder.TestBookId1;
            var originalQuantity = (await bookRepo.GetByIdAsync(bookId))!.ActualQuantity;

            // Act
            await service.BorrowBookAsync(bookId, "Customer 1");
            await service.BorrowBookAsync(bookId, "Customer 2");
            var finalResult = await service.BorrowBookAsync(bookId, "Customer 3");

            // Assert
            finalResult.UpdatedBook!.ActualQuantity.Should().Be(originalQuantity - 3);

            var records = await borrowRepo.GetAllAsync();
            records.Should().HaveCount(3);
        }
    }
}