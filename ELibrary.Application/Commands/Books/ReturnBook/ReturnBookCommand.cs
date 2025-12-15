using ELibrary.Application.Common;
using ELibrary.Application.DTOs;
using MediatR;

namespace ELibrary.Application.Commands.Books.ReturnBook
{
    /// <summary>
    /// Command for returning a borrowed book to the library.
    /// Represents an intent to increase book inventory by 1.
    /// Implements IRequest to work with MediatR pipeline.
    /// </summary>
    /// <param name="BookId">The unique identifier of the book to return</param>
    /// <param name="CustomerName">The name of the customer returning the book</param>
    public record ReturnBookCommand(
        Guid BookId,
        string CustomerName
    ) : IRequest<ELibraryResult<BookDto>>;
}
