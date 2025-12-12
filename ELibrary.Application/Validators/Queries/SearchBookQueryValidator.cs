using ELibrary.Application.Queries.Books;
using FluentValidation;

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
                .WithMessage("Book name cannot exceed 1000 characters");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Author), () =>
        {
            RuleFor(x => x.Author)
                .MaximumLength(1000)
                .WithMessage("Author name cannot exceed 1000 characters");
        });

        When(x => !string.IsNullOrWhiteSpace(x.ISBN), () =>
        {
            RuleFor(x => x.ISBN)
                .MaximumLength(1000)
                .WithMessage("ISBN cannot exceed 1000 characters");
        });
    }
}