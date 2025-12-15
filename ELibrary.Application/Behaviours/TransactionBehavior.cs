using ELibrary.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ELibrary.Application.Behaviors
{
    /// <summary>
    /// Pipeline behavior that automatically wraps Commands in database transactions.
    /// Only applies to Commands (requests ending with "Command").
    /// Automatically commits on success and rolls back on exception.
    /// </summary>
    /// <typeparam name="TRequest">The request type</typeparam>
    /// <typeparam name="TResponse">The response type</typeparam>
    public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

        public TransactionBehavior(
            IUnitOfWork unitOfWork,
            ILogger<TransactionBehavior<TRequest, TResponse>> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;

            // Only for Commands
            // Queries don't need transactions
            if (!requestName.EndsWith("Command"))
            {
                return await next();
            }

            _logger.LogInformation(
                "Beginning transaction for {RequestName}",
                requestName);

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Execute the handler
                var response = await next();

                // Commit transaction on success
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation(
                    "Transaction committed successfully for {RequestName}",
                    requestName);

                return response;
            }
            catch (Exception ex)
            {
                // Rollback transaction on any exception
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                _logger.LogError(ex,
                    "Transaction rolled back for {RequestName} due to error: {ErrorMessage}",
                    requestName,
                    ex.Message);

                throw;
            }
        }
    }
}
