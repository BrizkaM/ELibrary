using FluentValidation;
using MediatR;

namespace ELibrary.Application.Behaviors
{
    /// <summary>
    /// Pipeline behavior that automatically validates requests using FluentValidation.
    /// Executes before the handler and throws ValidationException if validation fails.
    /// </summary>
    /// <typeparam name="TRequest">The request type</typeparam>
    /// <typeparam name="TResponse">The response type</typeparam>
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // Skip validation if no validators are registered
            if (!_validators.Any())
            {
                return await next();
            }

            // Create validation context
            var context = new ValidationContext<TRequest>(request);

            // Execute all validators
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            // Collect all failures
            var failures = validationResults
                .SelectMany(result => result.Errors)
                .Where(f => f != null)
                .ToList();

            // Throw exception if there are any validation failures
            if (failures.Any())
            {
                throw new ValidationException(failures);
            }

            // Continue with the pipeline
            return await next();
        }
    }
}
