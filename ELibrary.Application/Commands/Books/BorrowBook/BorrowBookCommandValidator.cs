using FluentValidation;

namespace ELibrary.Application.Commands.Books.BorrowBook
{
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
}