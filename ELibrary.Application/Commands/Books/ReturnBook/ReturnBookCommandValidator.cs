using FluentValidation;

namespace ELibrary.Application.Commands.Books.ReturnBook
{
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
}