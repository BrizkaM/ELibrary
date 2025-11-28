using ELibrary.Shared.DTOs;
using ELibrary.Shared.Interfaces;
using ELibrary.Tests.Helpers;
using ELibrary.WebApp.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ELibrary.Tests.Controllers
{
    [TestClass]
    public class BorrowBookRecordControllerTests
    {
        private Mock<IBorrowBookRecordRepository> _repositoryMock = null!;
        private BorrowBookRecordController _controller = null!;

        [TestInitialize]
        public void Initialize()
        {
            _repositoryMock = new Mock<IBorrowBookRecordRepository>();
            _controller = new BorrowBookRecordController(_repositoryMock.Object);
        }

        [TestMethod]
        public async Task GetAllBorrowBookRecords_ShouldReturnOkWithRecords()
        {
            // Arrange
            var books = TestDataBuilder.CreateTestBooks();
            var records = new[]
            {
                TestDataBuilder.CreateBorrowRecord(books[0], "Customer 1", "Borrowed"),
                TestDataBuilder.CreateBorrowRecord(books[1], "Customer 2", "Returned")
            };

            _repositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(records);

            // Act
            var result = await _controller.GetAllBorrowBookRecords();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var returnedRecords = okResult!.Value as IEnumerable<BorrowBookRecordDto>;
            returnedRecords.Should().NotBeNull();
            returnedRecords.Should().HaveCount(2);
        }

        [TestMethod]
        public async Task GetAllBorrowBookRecords_WithNoRecords_ShouldReturnOkWithEmptyList()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Shared.Entities.BorrowBookRecord>());

            // Act
            var result = await _controller.GetAllBorrowBookRecords();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var returnedRecords = okResult!.Value as IEnumerable<BorrowBookRecordDto>;
            returnedRecords.Should().NotBeNull();
            returnedRecords.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GetAllBorrowBookRecords_ShouldMapPropertiesCorrectly()
        {
            // Arrange
            var book = TestDataBuilder.CreateTestBook();
            var record = TestDataBuilder.CreateBorrowRecord(book, "John Doe", "Borrowed");
            record.Date = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

            _repositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new[] { record });

            // Act
            var result = await _controller.GetAllBorrowBookRecords();

            // Assert
            var okResult = result.Result as OkObjectResult;
            var returnedRecords = (okResult!.Value as IEnumerable<BorrowBookRecordDto>)!.ToList();
            
            var returnedRecord = returnedRecords.First();
            returnedRecord.ID.Should().Be(record.ID);
            returnedRecord.BookID.Should().Be(record.BookID);
            returnedRecord.CustomerName.Should().Be("John Doe");
            returnedRecord.Action.Should().Be("Borrowed");
            returnedRecord.Date.Should().Be(new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc));
        }

        [TestMethod]
        public async Task GetAllBorrowBookRecords_ShouldCallRepositoryOnce()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Shared.Entities.BorrowBookRecord>());

            // Act
            await _controller.GetAllBorrowBookRecords();

            // Assert
            _repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
        }
    }
}
