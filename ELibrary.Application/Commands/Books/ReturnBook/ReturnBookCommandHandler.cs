using AutoMapper;
using ELibrary.Application.Common;
using ELibrary.Application.DTOs;
using ELibrary.Domain.Entities;
using ELibrary.Domain.Enums;
using ELibrary.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ELibrary.Application.Commands.Books.ReturnBook
{
    /// <summary>
    /// Handler for ReturnBookCommand.
    /// Processes the command to return a book and create a return record.
    /// Uses transaction to ensure data consistency.
    /// </summary>
    public class ReturnBookCommandHandler : IRequestHandler<ReturnBookCommand, ELibraryResult<BookDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ReturnBookCommandHandler> _logger;

        public ReturnBookCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<ReturnBookCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ELibraryResult<BookDto>> Handle(
            ReturnBookCommand request,
            CancellationToken cancellationToken)
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                _logger.LogInformation(
                    "Handling ReturnBookCommand: BookId={BookId}, Customer={CustomerName}",
                    request.BookId, request.CustomerName);

                // Get the book
                var book = await _unitOfWork.Books.GetByIdAsync(request.BookId);

                if (book == null)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    _logger.LogWarning(
                        "ReturnBookCommand failed: Book not found. BookId={BookId}",
                        request.BookId);

                    return ELibraryResult<BookDto>.Failure(
                        $"Book with ID {request.BookId} not found",
                        ErrorCodes.NotFound);
                }

                // Increase quantity
                book.ActualQuantity += 1;
                _unitOfWork.Books.Update(book);

                // Create return record
                var bookRecord = new BorrowBookRecord
                {
                    BookID = request.BookId,
                    Book = book,
                    CustomerName = request.CustomerName,
                    Action = BookActionType.Returned.ToString(),
                    Date = DateTime.UtcNow
                };

                await _unitOfWork.BorrowRecords.AddAsync(bookRecord);

                // Save changes and commit transaction
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation(
                    "ReturnBookCommand succeeded. BookId={BookId}, Customer={CustomerName}, NewQuantity={Quantity}",
                    request.BookId, request.CustomerName, book.ActualQuantity);

                return ELibraryResult<BookDto>.Success(_mapper.Map<BookDto>(book));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogError(ex, "ReturnBookCommand failed with exception");
                throw;
            }
        }
    }
}
