using ELibrary.Application.Common;
using ELibrary.Application.DTOs;
using MediatR;

namespace ELibrary.Application.Commands.Books.BorrowBook
{
    /// <summary>
    /// Command for borrowing a book from the library.
    /// Represents an intent to decrease book inventory by 1.
    /// Implements IRequest to work with MediatR pipeline.
    /// </summary>
    /// <param name="BookId">The unique identifier of the book to borrow</param>
    /// <param name="CustomerName">The name of the customer borrowing the book</param>
    public record BorrowBookCommand(
        Guid BookId,
        string CustomerName
    ) : IRequest<ELibraryResult<BookDto>>;
}
