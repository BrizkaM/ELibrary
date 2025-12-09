using ELibrary.Domain.DTOs;
using FluentValidation;

namespace ELibrary.Domain.Validators
{
    /// <summary>
    /// Validator for BorrowBookRecordDto
    /// </summary>
    public class BorrowBookRecordDtoValidator : AbstractValidator<BorrowBookRecordDto>
    {
        public BorrowBookRecordDtoValidator()
        {
            RuleFor(x => x.BookID)
                .NotEmpty()
                .WithMessage("Book ID is required");

            RuleFor(x => x.CustomerName)
                .NotEmpty()
                .WithMessage("Customer name is required")
                .MaximumLength(1000)
                .WithMessage("Customer name cannot exceed 1000 characters")
                .MinimumLength(2)
                .WithMessage("Customer name must be at least 2 characters");

            RuleFor(x => x.Action)
                .NotEmpty()
                .WithMessage("Action is required")
                .MaximumLength(200)
                .WithMessage("Action cannot exceed 200 characters")
                .Must(BeValidAction)
                .WithMessage("Action must be either 'Borrowed' or 'Returned'");

            RuleFor(x => x.Date)
                .NotEmpty()
                .WithMessage("Date is required")
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("Date cannot be in the future");
        }

        /// <summary>
        /// Validates that the action is one of the allowed values
        /// </summary>
        private bool BeValidAction(string action)
        {
            return action == "Borrowed" || action == "Returned";
        }
    }
}
