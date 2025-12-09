using FluentValidation;

namespace ELibrary.Domain.Validators.Commands
{
    /// <summary>
    /// Command for borrowing a book
    /// </summary>
    public record BorrowBookCommand(Guid BookId, string CustomerName);

    /// <summary>
    /// Validator for BorrowBookCommand
    /// </summary>
    public class BorrowBookCommandValidator : AbstractValidator<BorrowBookCommand>
    {
        public BorrowBookCommandValidator()
        {
            RuleFor(x => x.BookId)
                .NotEmpty()
                .WithMessage("Book ID is required")
                .NotEqual(Guid.Empty)
                .WithMessage("Book ID cannot be empty");

            RuleFor(x => x.CustomerName)
                .NotEmpty()
                .WithMessage("Customer name is required")
                .MinimumLength(2)
                .WithMessage("Customer name must be at least 2 characters")
                .MaximumLength(1000)
                .WithMessage("Customer name cannot exceed 1000 characters")
                .Matches(@"^[a-zA-Z\s\-'\.]+$")
                .WithMessage("Customer name contains invalid characters");
        }
    }

    /// <summary>
    /// Command for returning a book
    /// </summary>
    public record ReturnBookCommand(Guid BookId, string CustomerName);

    /// <summary>
    /// Validator for ReturnBookCommand
    /// </summary>
    public class ReturnBookCommandValidator : AbstractValidator<ReturnBookCommand>
    {
        public ReturnBookCommandValidator()
        {
            RuleFor(x => x.BookId)
                .NotEmpty()
                .WithMessage("Book ID is required")
                .NotEqual(Guid.Empty)
                .WithMessage("Book ID cannot be empty");

            RuleFor(x => x.CustomerName)
                .NotEmpty()
                .WithMessage("Customer name is required")
                .MinimumLength(2)
                .WithMessage("Customer name must be at least 2 characters")
                .MaximumLength(1000)
                .WithMessage("Customer name cannot exceed 1000 characters")
                .Matches(@"^[a-zA-Z\s\-'\.]+$")
                .WithMessage("Customer name contains invalid characters");
        }
    }

    /// <summary>
    /// Command for creating a book
    /// </summary>
    public record CreateBookCommand(
        string Name,
        string Author,
        DateTime Year,
        string ISBN,
        int ActualQuantity
    );

    /// <summary>
    /// Validator for CreateBookCommand
    /// </summary>
    public class CreateBookCommandValidator : AbstractValidator<CreateBookCommand>
    {
        public CreateBookCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Book name is required")
                .MaximumLength(1000)
                .WithMessage("Book name cannot exceed 1000 characters");

            RuleFor(x => x.Author)
                .NotEmpty()
                .WithMessage("Author name is required")
                .MaximumLength(1000)
                .WithMessage("Author name cannot exceed 1000 characters");

            RuleFor(x => x.ISBN)
                .NotEmpty()
                .WithMessage("ISBN is required")
                .MaximumLength(1000)
                .WithMessage("ISBN cannot exceed 1000 characters")
                .Must(BeValidISBN)
                .WithMessage("ISBN format is invalid. Expected format: ISBN-10 or ISBN-13");

            RuleFor(x => x.Year)
                .NotEmpty()
                .WithMessage("Publication year is required")
                .Must(BeValidYear)
                .WithMessage("Publication year must be between 1000 and current year");

            RuleFor(x => x.ActualQuantity)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Quantity cannot be negative");
        }

        private bool BeValidYear(DateTime year)
        {
            return year.Year >= 1000 && year.Year <= DateTime.UtcNow.Year;
        }

        private bool BeValidISBN(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
                return false;

            var cleanIsbn = isbn.Replace("-", "").Replace(" ", "");

            if (cleanIsbn.Length == 10 || cleanIsbn.Length == 13)
            {
                return cleanIsbn.All(char.IsDigit) || 
                       cleanIsbn.Length == 10 && cleanIsbn.Take(9).All(char.IsDigit);
            }

            return false;
        }
    }
}
