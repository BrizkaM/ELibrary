using ELibrary.Database.Repositories;
using ELibrary.Shared.Entities;
using ELibrary.Tests.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ELibrary.Tests.Repositories
{
    [TestClass]
    public class BookRepositoryTests
    {
        [TestMethod]
        public async Task GetAllAsync_ShouldReturnAllBooks_OrderedByAuthorDescending()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateSeededContext();
            var repository = new BookRepository(context);

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeInDescendingOrder(b => b.Author);
        }

        [TestMethod]
        public async Task GetByIdAsync_WithValidId_ShouldReturnBook()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateSeededContext();
            var repository = new BookRepository(context);

            // Act
            var result = await repository.GetByIdAsync(TestDataBuilder.TestBookId1);

            // Assert
            result.Should().NotBeNull();
            result!.ID.Should().Be(TestDataBuilder.TestBookId1);
            result.Name.Should().Be("Test Book 1");
        }

        [TestMethod]
        public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateSeededContext();
            var repository = new BookRepository(context);
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await repository.GetByIdAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task GetByISBNAsync_WithValidISBN_ShouldReturnBook()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateSeededContext();
            var repository = new BookRepository(context);

            // Act
            var result = await repository.GetByISBNAsync("1234567890123");

            // Assert
            result.Should().NotBeNull();
            result!.ISBN.Should().Be("1234567890123");
            result.Name.Should().Be("Test Book 1");
        }

        [TestMethod]
        public async Task GetByISBNAsync_WithInvalidISBN_ShouldReturnNull()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateSeededContext();
            var repository = new BookRepository(context);

            // Act
            var result = await repository.GetByISBNAsync("9999999999999");

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task GetFilteredBooksAsync_ByName_ShouldReturnMatchingBooks()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateSeededContext();
            var repository = new BookRepository(context);

            // Act
            var result = await repository.GetFilteredBooksAsync(name: "Test Book 1", null, null);

            // Assert
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Test Book 1");
        }

        [TestMethod]
        public async Task GetFilteredBooksAsync_ByAuthor_ShouldReturnMatchingBooks()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateSeededContext();
            var repository = new BookRepository(context);

            // Act
            var result = await repository.GetFilteredBooksAsync(null, "Test Author 2", null);

            // Assert
            result.Should().HaveCount(1);
            result.First().Author.Should().Be("Test Author 2");
        }

        [TestMethod]
        public async Task GetFilteredBooksAsync_ByISBN_ShouldReturnMatchingBooks()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateSeededContext();
            var repository = new BookRepository(context);

            // Act
            var result = await repository.GetFilteredBooksAsync(null, null, "1234567890123");

            // Assert
            result.Should().HaveCount(1);
            result.First().ISBN.Should().Be("1234567890123");
        }

        [TestMethod]
        public async Task GetFilteredBooksAsync_WithPartialName_ShouldReturnMatchingBooks()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateSeededContext();
            var repository = new BookRepository(context);

            // Act
            var result = await repository.GetFilteredBooksAsync(name: "Book", null, null);

            // Assert
            result.Should().HaveCount(3);
        }

        [TestMethod]
        public async Task GetFilteredBooksAsync_WithNoMatches_ShouldReturnEmptyList()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateSeededContext();
            var repository = new BookRepository(context);

            // Act
            var result = await repository.GetFilteredBooksAsync(name: "NonExistentBook", null, null);

            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GetFilteredBooksAsync_WithMultipleCriteria_ShouldReturnMatchingBooks()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateSeededContext();
            var repository = new BookRepository(context);

            // Act
            var result = await repository.GetFilteredBooksAsync(
                name: "Test Book 1", 
                author: "Test Author 1", 
                isbn: "1234567890123");

            // Assert
            result.Should().HaveCount(1);
            var book = result.First();
            book.Name.Should().Be("Test Book 1");
            book.Author.Should().Be("Test Author 1");
            book.ISBN.Should().Be("1234567890123");
        }

        [TestMethod]
        public async Task AddAsync_ShouldAddNewBook()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryContext();
            var repository = new BookRepository(context);
            var newBook = TestDataBuilder.CreateTestBook();

            // Act
            var result = await repository.AddAsync(newBook);
            await context.SaveChangesAsync();

            // Assert
            result.Should().NotBeNull();
            result.ID.Should().Be(newBook.ID);
            
            var bookInDb = await repository.GetByIdAsync(newBook.ID);
            bookInDb.Should().NotBeNull();
            bookInDb!.Name.Should().Be(newBook.Name);
        }

        [TestMethod]
        public async Task Update_ShouldUpdateExistingBook()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateSeededContext();
            var repository = new BookRepository(context);
            var book = await repository.GetByIdAsync(TestDataBuilder.TestBookId1);
            var originalName = book!.Name;

            // Act
            book.Name = "Updated Book Name";
            repository.Update(book);
            await context.SaveChangesAsync();

            // Assert
            var updatedBook = await repository.GetByIdAsync(TestDataBuilder.TestBookId1);
            updatedBook.Should().NotBeNull();
            updatedBook!.Name.Should().Be("Updated Book Name");
            updatedBook.Name.Should().NotBe(originalName);
        }

        [TestMethod]
        public void Constructor_WithNullContext_ShouldThrowArgumentNullException()
        {
            // Act
            Action act = () => new BookRepository(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("context");
        }
    }
}
