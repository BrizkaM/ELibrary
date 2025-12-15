using MediatR;
using Microsoft.Extensions.Logging;

namespace ELibrary.Application.Behaviors
{
    /// <summary>
    /// Pipeline behavior that logs every request and response.
    /// Logs request name before execution and after execution.
    /// </summary>
    /// <typeparam name="TRequest">The request type</typeparam>
    /// <typeparam name="TResponse">The response type</typeparam>
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;

            // Log before handling
            _logger.LogInformation(
                "Handling {RequestName}",
                requestName);

            try
            {
                // Execute the handler
                var response = await next();

                // Log after successful handling
                _logger.LogInformation(
                    "Successfully handled {RequestName}",
                    requestName);

                return response;
            }
            catch (Exception ex)
            {
                // Log error
                _logger.LogError(ex,
                    "Error handling {RequestName}: {ErrorMessage}",
                    requestName,
                    ex.Message);

                throw;
            }
        }
    }
}
