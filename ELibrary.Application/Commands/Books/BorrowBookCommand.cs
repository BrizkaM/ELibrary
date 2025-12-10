namespace ELibrary.Application.Commands.Books
{
    /// <summary>
    /// Command for borrowing a book from the library.
    /// Represents an intent to decrease book inventory by 1.
    /// </summary>
    /// <param name="BookId">The unique identifier of the book to borrow</param>
    /// <param name="CustomerName">The name of the customer borrowing the book</param>
    public record BorrowBookCommand(
        Guid BookId,
        string CustomerName
    );
}