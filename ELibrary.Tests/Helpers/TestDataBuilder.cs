using ELibrary.Shared.DTOs;
using ELibrary.Shared.Entities;

namespace ELibrary.Tests.Helpers
{
    /// <summary>
    /// Helper class for building test data
    /// </summary>
    public static class TestDataBuilder
    {
        public static readonly Guid TestBookId1 = new("11111111-1111-1111-1111-111111111111");
        public static readonly Guid TestBookId2 = new("22222222-2222-2222-2222-222222222222");
        public static readonly Guid TestBookId3 = new("33333333-3333-3333-3333-333333333333");

        /// <summary>
        /// Creates a list of test books
        /// </summary>
        public static List<Book> CreateTestBooks()
        {
            return new List<Book>
            {
                new Book
                {
                    ID = TestBookId1,
                    Name = "Test Book 1",
                    Author = "Test Author 1",
                    ISBN = "1234567890123",
                    Year = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    ActualQuantity = 5
                },
                new Book
                {
                    ID = TestBookId2,
                    Name = "Test Book 2",
                    Author = "Test Author 2",
                    ISBN = "1234567890124",
                    Year = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    ActualQuantity = 3
                },
                new Book
                {
                    ID = TestBookId3,
                    Name = "Out of Stock Book",
                    Author = "Test Author 3",
                    ISBN = "1234567890125",
                    Year = new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    ActualQuantity = 0
                }
            };
        }

        /// <summary>
        /// Creates a list of test book dtos.
        /// </summary>
        public static List<BookDto> CreateTestBookDtos()
        {
            return new List<BookDto>
            {
                new BookDto
                {
                    ID = TestBookId1,
                    Name = "Test Book 1",
                    Author = "Test Author 1",
                    ISBN = "1234567890123",
                    Year = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    ActualQuantity = 5
                },
                new BookDto
                {
                    ID = TestBookId2,
                    Name = "Test Book 2",
                    Author = "Test Author 2",
                    ISBN = "1234567890124",
                    Year = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    ActualQuantity = 3
                },
                new BookDto
                {
                    ID = TestBookId3,
                    Name = "Out of Stock Book",
                    Author = "Test Author 3",
                    ISBN = "1234567890125",
                    Year = new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    ActualQuantity = 0
                }
            };
        }

        /// <summary>
        /// Creates a single test book
        /// </summary>
        public static Book CreateTestBook(
            Guid? id = null,
            string name = "Test Book",
            string author = "Test Author",
            string isbn = "9999999999999",
            int quantity = 5)
        {
            return new Book
            {
                ID = id ?? Guid.NewGuid(),
                Name = name,
                Author = author,
                ISBN = isbn,
                Year = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                ActualQuantity = quantity
            };
        }

        /// <summary>
        /// Creates a single test book
        /// </summary>
        public static BookDto CreateTestBookDto(
            Guid? id = null,
            string name = "Test Book",
            string author = "Test Author",
            string isbn = "9999999999999",
            int quantity = 5)
        {
            return new BookDto
            {
                ID = id ?? Guid.NewGuid(),
                Name = name,
                Author = author,
                ISBN = isbn,
                Year = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                ActualQuantity = quantity
            };
        }

        /// <summary>
        /// Creates a test borrow record
        /// </summary>
        public static BorrowBookRecord CreateBorrowRecord(
            Book book,
            string customerName = "Test Customer",
            string action = "Borrowed")
        {
            return new BorrowBookRecord
            {
                ID = Guid.NewGuid(),
                BookID = book.ID,
                Book = book,
                CustomerName = customerName,
                Action = action,
                Date = DateTime.UtcNow
            };
        }
    }
}
