using ELibrary.Shared.DTOs;
using FluentValidation;

namespace ELibrary.Shared.Validators
{
    /// <summary>
    /// Validator for BookDto to ensure data integrity
    /// </summary>
    public class BookDtoValidator : AbstractValidator<BookDto>
    {
        public BookDtoValidator()
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
        /// </summary>
        private bool BeValidISBN(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
                return false;

            // Remove hyphens and spaces for validation
            var cleanIsbn = isbn.Replace("-", "").Replace(" ", "");

            // ISBN-10: 10 digits
            // ISBN-13: 13 digits (starts with 978 or 979)
            if (cleanIsbn.Length == 10 || cleanIsbn.Length == 13)
            {
                return cleanIsbn.All(char.IsDigit) ||
                       (cleanIsbn.Length == 10 && cleanIsbn.Take(9).All(char.IsDigit));
            }

            return false;
        }
    }
}
