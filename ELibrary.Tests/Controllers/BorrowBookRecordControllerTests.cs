using ELibrary.Services.Interfaces;
using ELibrary.Shared.DTOs;
using ELibrary.Tests.Helpers;
using ELibrary.WebApp.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ELibrary.Tests.Controllers
{
    /// <summary>
    /// Tests for BorrowBookRecordController API endpoints for retrieving borrow/return records.
    /// </summary>
    [TestClass]
    public class BorrowBookRecordControllerTests
    {
        private Mock<IBorrowBookRecordService> _repositoryMock = null!;
        private BorrowBookRecordController _controller = null!;

        [TestInitialize]
        public void Initialize()
        {
            _repositoryMock = new Mock<IBorrowBookRecordService>();
            _controller = new BorrowBookRecordController(_repositoryMock.Object);
        }

        /// <summary>
        /// Verifies that GetAllBorrowBookRecords returns OK status with a collection of records.
        /// </summary>
        [TestMethod]
        public async Task GetAllBorrowBookRecords_ShouldReturnOkWithRecords()
        {
            // Arrange
            var books = TestDataBuilder.CreateTestBooks();
            var records = new[]
            {
                TestDataBuilder.CreateBorrowRecordDto(books[0].ID, "Customer 1", "Borrowed"),
                TestDataBuilder.CreateBorrowRecordDto(books[1].ID, "Customer 2", "Returned")
            };
            _repositoryMock.Setup(r => r.GetAllBorrowBookRecordsAsync())
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

        /// <summary>
        /// Verifies that GetAllBorrowBookRecords returns OK status with empty list when no records exist.
        /// </summary>
        [TestMethod]
        public async Task GetAllBorrowBookRecords_WithNoRecords_ShouldReturnOkWithEmptyList()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetAllBorrowBookRecordsAsync())
                .ReturnsAsync(new List<BorrowBookRecordDto>());

            // Act
            var result = await _controller.GetAllBorrowBookRecords();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            var returnedRecords = okResult!.Value as IEnumerable<BorrowBookRecordDto>;
            returnedRecords.Should().NotBeNull();
            returnedRecords.Should().BeEmpty();
        }

        /// <summary>
        /// Verifies that GetAllBorrowBookRecords correctly maps entity properties to DTO properties.
        /// </summary>
        [TestMethod]
        public async Task GetAllBorrowBookRecords_ShouldMapPropertiesCorrectly()
        {
            // Arrange
            var book = TestDataBuilder.CreateTestBook();
            var record = TestDataBuilder.CreateBorrowRecordDto(book.ID, "John Doe", "Borrowed");
            record.Date = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
            _repositoryMock.Setup(r => r.GetAllBorrowBookRecordsAsync())
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

        /// <summary>
        /// Verifies that GetAllBorrowBookRecords calls the repository GetAllAsync method exactly once.
        /// </summary>
        [TestMethod]
        public async Task GetAllBorrowBookRecords_ShouldCallRepositoryOnce()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetAllBorrowBookRecordsAsync())
                .ReturnsAsync(new List<BorrowBookRecordDto>());

            // Act
            await _controller.GetAllBorrowBookRecords();

            // Assert
            _repositoryMock.Verify(r => r.GetAllBorrowBookRecordsAsync(), Times.Once);
        }
    }
}