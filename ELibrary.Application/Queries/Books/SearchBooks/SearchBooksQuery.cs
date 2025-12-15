using ELibrary.Application.Common;
using ELibrary.Application.DTOs;
using MediatR;

namespace ELibrary.Application.Queries.Books.SearchBooks
{
    /// <summary>
    /// Query for searching books by various criteria.
    /// This is a read-only operation with no side effects.
    /// At least one search criterion must be provided.
    /// Implements IRequest to work with MediatR pipeline.
    /// </summary>
    /// <param name="Name">Optional book name/title to search for</param>
    /// <param name="Author">Optional author name to search for</param>
    /// <param name="ISBN">Optional ISBN to search for</param>
    public record SearchBooksQuery(
        string? Name,
        string? Author,
        string? ISBN
    ) : IRequest<ELibraryResult<IEnumerable<BookDto>>>;
}
