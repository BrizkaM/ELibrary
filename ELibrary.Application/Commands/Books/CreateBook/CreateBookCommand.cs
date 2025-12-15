using ELibrary.Application.Common;
using ELibrary.Application.DTOs;
using MediatR;

namespace ELibrary.Application.Commands.Books.CreateBook
{
    /// <summary>
    /// Command for creating a new book in the library.
    /// Represents an intent to add a new book to the catalog.
    /// Implements IRequest to work with MediatR pipeline.
    /// </summary>
    /// <param name="Name">The title of the book</param>
    /// <param name="Author">The author of the book</param>
    /// <param name="Year">The publication year</param>
    /// <param name="ISBN">The ISBN identifier</param>
    /// <param name="ActualQuantity">Initial quantity in stock</param>
    public record CreateBookCommand(
        string Name,
        string Author,
        DateTime Year,
        string ISBN,
        int ActualQuantity
    ) : IRequest<ELibraryResult<BookDto>>;
}
