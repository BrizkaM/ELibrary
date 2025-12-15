using ELibrary.Application.Common;
using ELibrary.Application.DTOs;
using MediatR;

namespace ELibrary.Application.Queries.Books.GetAllBooks
{
    /// <summary>
    /// Query for retrieving all books from the library.
    /// This is a read-only operation with no side effects.
    /// Implements IRequest to work with MediatR pipeline.
    /// </summary>
    public record GetAllBooksQuery() : IRequest<ELibraryResult<IEnumerable<BookDto>>>;
}
