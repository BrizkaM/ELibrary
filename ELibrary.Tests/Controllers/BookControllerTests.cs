using ELibrary.Shared.DTOs;
using ELibrary.Shared.Entities;
using ELibrary.Shared.Enums;
using ELibrary.Shared.Interfaces;
using ELibrary.Tests.Helpers;
using ELibrary.WebApp.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace ELibrary.Tests.Controllers
{
    [TestClass]
    public class BookControllerTests
    {
        private Mock<IBookRepository> _bookRepositoryMock = null!;
        private Mock<IBookService> _bookServiceMock = null!;
        private Mock<ILogger<BookController>> _loggerMock = null!;
        private BookController _controller = null!;

        [TestInitialize]
        public void Initialize()
        {
            _bookRepositoryMock = new Mock<IBookRepository>();
            _bookServiceMock = new Mock<IBookService>();
            _loggerMock = new Mock<ILogger<BookController>>();
            _controller = new BookController(
                _bookRepositoryMock.Object,
                _bookServiceMock.Object,
                _loggerMock.Object);
        }

        [TestMethod]
        public async Task GetAllBooks_ShouldReturnOkWithBooks()
        {
            // Arrange
            var books = TestDataBuilder.CreateTestBooks();
            _bookRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(books);

            // Act
            var result = await _controller.GetAllBooks();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var returnedBooks = okResult!.Value as IEnumerable<BookDto>;
            returnedBooks.Should().HaveCount(3);
        }

        [TestMethod]
        public async Task FindBooksByCriteria_WithValidCriteria_ShouldReturnOkWithBooks()
        {
            // Arrange
            var books = new List<Book> { TestDataBuilder.CreateTestBook(name: "Specific Book") };
            _bookRepositoryMock.Setup(r => r.GetFilteredBooksAsync("Specific", null, null))
                .ReturnsAsync(books);

            // Act
            var result = await _controller.FindBooksByCriteria("Specific", null, null);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var returnedBooks = okResult!.Value as IEnumerable<BookDto>;
            returnedBooks.Should().HaveCount(1);
            returnedBooks!.First().Name.Should().Be("Specific Book");
        }

        [TestMethod]
        public async Task FindBooksByCriteria_WithNoMatches_ShouldReturnNotFound()
        {
            // Arrange
            _bookRepositoryMock.Setup(r => r.GetFilteredBooksAsync("NonExistent", null, null))
                .ReturnsAsync(new List<Book>());

            // Act
            var result = await _controller.FindBooksByCriteria("NonExistent", null, null);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [TestMethod]
        public async Task CreateBook_WithValidBook_ShouldReturnCreatedAtAction()
        {
            // Arrange
            var bookDto = new BookDto
            {
                Name = "New Book",
                Author = "New Author",
                ISBN = "1234567890123",
                Year = new DateTime(2020, 1, 1),
                ActualQuantity = 5
            };

            var createdBook = new Book
            {
                ID = Guid.NewGuid(),
                Name = bookDto.Name,
                Author = bookDto.Author,
                ISBN = bookDto.ISBN,
                Year = bookDto.Year,
                ActualQuantity = bookDto.ActualQuantity
            };

            _bookServiceMock.Setup(s => s.CreateBookAsync(It.IsAny<Book>()))
                .ReturnsAsync(createdBook);

            // Act
            var result = await _controller.CreateBook(bookDto);

            // Assert
            result.Result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result.Result as CreatedAtActionResult;
            var returnedBook = createdResult!.Value as BookDto;
            returnedBook.Should().NotBeNull();
            returnedBook!.Name.Should().Be("New Book");
        }

        [TestMethod]
        public async Task CreateBook_WithInvalidModel_ShouldReturnBadRequest()
        {
            // Arrange
            var bookDto = new BookDto();
            _controller.ModelState.AddModelError("Name", "Name is required");

            // Act
            var result = await _controller.CreateBook(bookDto);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [TestMethod]
        public async Task BorrowBook_WithValidBook_ShouldReturnOkWithUpdatedBook()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            var updatedBook = TestDataBuilder.CreateTestBook(id: bookId, quantity: 4);
            
            _bookServiceMock.Setup(s => s.BorrowBookAsync(bookId, "John Doe"))
                .ReturnsAsync((CustomerBookOperationResult.Success, updatedBook));

            // Act
            var result = await _controller.BorrowBook(bookId, "John Doe");

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var returnedBook = okResult!.Value as BookDto;
            returnedBook.Should().NotBeNull();
            returnedBook!.ActualQuantity.Should().Be(4);
        }

        [TestMethod]
        public async Task BorrowBook_WithNonExistentBook_ShouldReturnNotFound()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            _bookServiceMock.Setup(s => s.BorrowBookAsync(bookId, "John Doe"))
                .ReturnsAsync((CustomerBookOperationResult.NotFound, null));

            // Act
            var result = await _controller.BorrowBook(bookId, "John Doe");

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult!.Value.Should().BeOfType<string>();
        }

        [TestMethod]
        public async Task BorrowBook_WithOutOfStockBook_ShouldReturnBadRequest()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            _bookServiceMock.Setup(s => s.BorrowBookAsync(bookId, "John Doe"))
                .ReturnsAsync((CustomerBookOperationResult.OutOfStock, null));

            // Act
            var result = await _controller.BorrowBook(bookId, "John Doe");

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [TestMethod]
        public async Task BorrowBook_WithConflict_ShouldReturnConflict()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            _bookServiceMock.Setup(s => s.BorrowBookAsync(bookId, "John Doe"))
                .ReturnsAsync((CustomerBookOperationResult.Conflict, null));

            // Act
            var result = await _controller.BorrowBook(bookId, "John Doe");

            // Assert
            result.Result.Should().BeOfType<ConflictObjectResult>();
        }

        [TestMethod]
        public async Task BorrowBook_WithNullCustomerName_ShouldUseAnonymous()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            var updatedBook = TestDataBuilder.CreateTestBook(id: bookId, quantity: 4);
            
            _bookServiceMock.Setup(s => s.BorrowBookAsync(bookId, "anonym"))
                .ReturnsAsync((CustomerBookOperationResult.Success, updatedBook));

            // Act
            var result = await _controller.BorrowBook(bookId, null);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            _bookServiceMock.Verify(s => s.BorrowBookAsync(bookId, "anonym"), Times.Once);
        }

        [TestMethod]
        public async Task ReturnBook_WithValidBook_ShouldReturnOkWithUpdatedBook()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            var updatedBook = TestDataBuilder.CreateTestBook(id: bookId, quantity: 6);
            
            _bookServiceMock.Setup(s => s.ReturnBookAsync(bookId, "Jane Doe"))
                .ReturnsAsync((CustomerBookOperationResult.Success, updatedBook));

            // Act
            var result = await _controller.ReturnBook(bookId, "Jane Doe");

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var returnedBook = okResult!.Value as BookDto;
            returnedBook.Should().NotBeNull();
            returnedBook!.ActualQuantity.Should().Be(6);
        }

        [TestMethod]
        public async Task ReturnBook_WithNonExistentBook_ShouldReturnNotFound()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            _bookServiceMock.Setup(s => s.ReturnBookAsync(bookId, "Jane Doe"))
                .ReturnsAsync((CustomerBookOperationResult.NotFound, null));

            // Act
            var result = await _controller.ReturnBook(bookId, "Jane Doe");

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [TestMethod]
        public async Task ReturnBook_WithConflict_ShouldReturnConflict()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            _bookServiceMock.Setup(s => s.ReturnBookAsync(bookId, "Jane Doe"))
                .ReturnsAsync((CustomerBookOperationResult.Conflict, null));

            // Act
            var result = await _controller.ReturnBook(bookId, "Jane Doe");

            // Assert
            result.Result.Should().BeOfType<ConflictObjectResult>();
        }

        [TestMethod]
        public async Task ReturnBook_WithNullCustomerName_ShouldUseAnonymous()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            var updatedBook = TestDataBuilder.CreateTestBook(id: bookId, quantity: 6);
            
            _bookServiceMock.Setup(s => s.ReturnBookAsync(bookId, "anonym"))
                .ReturnsAsync((CustomerBookOperationResult.Success, updatedBook));

            // Act
            var result = await _controller.ReturnBook(bookId, null);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            _bookServiceMock.Verify(s => s.ReturnBookAsync(bookId, "anonym"), Times.Once);
        }
    }
}
