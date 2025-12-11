using ELibrary.Application.Commands.Books;
using FluentValidation;

namespace ELibrary.Application.Validators.Commands
{
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

        /// <summary>
        /// Validates that the year is within acceptable range
        /// </summary>
        private bool BeValidYear(DateTime year)
        {
            return year.Year >= 1000 && year.Year <= DateTime.UtcNow.Year;
        }

        /// <summary>
        /// Validates ISBN format (basic validation)
        /// ISBN-10: 10 digits (last can be X)
        /// ISBN-13: 13 digits starting with 978 or 979
        /// </summary>
        private bool BeValidISBN(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
                return false;

            // Remove hyphens and spaces for validation
            var cleanIsbn = isbn.Replace("-", "").Replace(" ", "");

            // ISBN-10: 10 digits (last can be X)
            if (cleanIsbn.Length == 10)
            {
                // All digits OR first 9 digits + X at the end
                return cleanIsbn.All(char.IsDigit) ||
                       (cleanIsbn.Take(9).All(char.IsDigit) && cleanIsbn[9] == 'X');
            }

            // ISBN-13: 13 digits
            if (cleanIsbn.Length == 13)
            {
                return cleanIsbn.All(char.IsDigit);
            }

            return false;
        }
    }
}