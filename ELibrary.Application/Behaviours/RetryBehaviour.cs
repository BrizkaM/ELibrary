using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;

namespace ELibrary.Application.Behaviors
{
    /// <summary>
    /// Pipeline behavior that automatically retries Commands when concurrency conflicts occur.
    /// Only applies to Commands that may have concurrency issues (BorrowBook, ReturnBook).
    /// Uses exponential backoff (100ms, 200ms, 400ms) for retry delays.
    /// </summary>
    /// <typeparam name="TRequest">The request type</typeparam>
    /// <typeparam name="TResponse">The response type</typeparam>
    public class RetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<RetryBehavior<TRequest, TResponse>> _logger;
        private const int MaxRetryAttempts = 3;
        private const int BaseDelayMilliseconds = 100;

        public RetryBehavior(ILogger<RetryBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;

            // Only retry for Commands that might have concurrency issues
            // Borrow and Return operations modify the same book record concurrently
            if (!ShouldRetry(requestName))
            {
                return await next();
            }

            _logger.LogDebug(
                "Retry policy enabled for {RequestName}",
                requestName);

            var retryPolicy = Policy
                .Handle<DbUpdateConcurrencyException>()
                .WaitAndRetryAsync(
                    retryCount: MaxRetryAttempts,
                    sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(BaseDelayMilliseconds * Math.Pow(2, attempt - 1)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            exception,
                            "Retry attempt {RetryCount}/{MaxRetries} for {RequestName} after {Delay}ms delay due to concurrency conflict",
                            retryCount,
                            MaxRetryAttempts,
                            requestName,
                            timeSpan.TotalMilliseconds);
                    });

            try
            {
                return await retryPolicy.ExecuteAsync(() => next());
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // All retries exhausted
                _logger.LogError(ex,
                    "All {MaxRetries} retry attempts exhausted for {RequestName}. Concurrency conflict could not be resolved.",
                    MaxRetryAttempts,
                    requestName);

                throw; // Re-throw to be handled by Handler or Global Exception Handler
            }
        }

        /// <summary>
        /// Determines if retry logic should be applied for the given request.
        /// </summary>
        /// <param name="requestName">The name of the request type</param>
        /// <returns>True if retry should be applied, false otherwise</returns>
        private static bool ShouldRetry(string requestName)
        {
            // Apply retry logic to Commands that modify book quantities
            // These are prone to concurrency conflicts
            return requestName.Contains("BorrowBook", StringComparison.OrdinalIgnoreCase)
                   || requestName.Contains("ReturnBook", StringComparison.OrdinalIgnoreCase);
        }
    }
}