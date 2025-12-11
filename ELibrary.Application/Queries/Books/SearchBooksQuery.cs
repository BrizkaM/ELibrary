namespace ELibrary.Application.Queries.Books
{
    /// <summary>
    /// Query for searching books by various criteria.
    /// This is a read-only operation with no side effects.
    /// At least one search criterion must be provided.
    /// </summary>
    /// <param name="Name">Optional book name/title to search for</param>
    /// <param name="Author">Optional author name to search for</param>
    /// <param name="ISBN">Optional ISBN to search for</param>
    public record SearchBooksQuery(
        string? Name,
        string? Author,
        string? ISBN
    );
}