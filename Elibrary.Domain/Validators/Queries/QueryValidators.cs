using FluentValidation;

namespace ELibrary.Domain.Validators.Queries
{
    /// <summary>
    /// Query for searching books by criteria
    /// </summary>
    public record SearchBooksQuery(string? Name, string? Author, string? ISBN);

    /// <summary>
    /// Validator for SearchBooksQuery
    /// </summary>
    public class SearchBooksQueryValidator : AbstractValidator<SearchBooksQuery>
    {
        public SearchBooksQueryValidator()
        {
            // At least one search criterion must be provided
            RuleFor(x => x)
                .Must(query => !string.IsNullOrWhiteSpace(query.Name) ||
                              !string.IsNullOrWhiteSpace(query.Author) ||
                              !string.IsNullOrWhiteSpace(query.ISBN))
                .WithMessage("At least one search criterion must be provided");

            When(x => !string.IsNullOrWhiteSpace(x.Name), () =>
            {
                RuleFor(x => x.Name)
                    .MaximumLength(1000)
                    .WithMessage("Book name cannot exceed 1000 characters")
                    .MinimumLength(2)
                    .WithMessage("Book name must be at least 2 characters");
            });

            When(x => !string.IsNullOrWhiteSpace(x.Author), () =>
            {
                RuleFor(x => x.Author)
                    .MaximumLength(1000)
                    .WithMessage("Author name cannot exceed 1000 characters")
                    .MinimumLength(2)
                    .WithMessage("Author name must be at least 2 characters");
            });

            When(x => !string.IsNullOrWhiteSpace(x.ISBN), () =>
            {
                RuleFor(x => x.ISBN)
                    .MaximumLength(1000)
                    .WithMessage("ISBN cannot exceed 1000 characters")
                    .MinimumLength(10)
                    .WithMessage("ISBN must be at least 10 characters");
            });
        }
    }

    /// <summary>
    /// Query for getting a book by ID
    /// </summary>
    public record GetBookByIdQuery(Guid BookId);

    /// <summary>
    /// Validator for GetBookByIdQuery
    /// </summary>
    public class GetBookByIdQueryValidator : AbstractValidator<GetBookByIdQuery>
    {
        public GetBookByIdQueryValidator()
        {
            RuleFor(x => x.BookId)
                .NotEmpty()
                .WithMessage("Book ID is required")
                .NotEqual(Guid.Empty)
                .WithMessage("Book ID cannot be empty");
        }
    }

    /// <summary>
    /// Query for getting all books
    /// </summary>
    public record GetAllBooksQuery();

    /// <summary>
    /// Validator for GetAllBooksQuery (no validation needed but kept for consistency)
    /// </summary>
    public class GetAllBooksQueryValidator : AbstractValidator<GetAllBooksQuery>
    {
        public GetAllBooksQueryValidator()
        {
            // No specific validation rules needed for this query
        }
    }
}
