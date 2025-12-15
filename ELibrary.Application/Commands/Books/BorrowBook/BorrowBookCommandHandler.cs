using AutoMapper;
using ELibrary.Application.Common;
using ELibrary.Application.DTOs;
using ELibrary.Domain.Entities;
using ELibrary.Domain.Enums;
using ELibrary.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ELibrary.Application.Commands.Books.BorrowBook
{
    /// <summary>
    /// Handler for BorrowBookCommand.
    /// Processes the command to borrow a book and create a borrow record.
    /// Uses transaction to ensure data consistency.
    /// </summary>
    public class BorrowBookCommandHandler : IRequestHandler<BorrowBookCommand, ELibraryResult<BookDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<BorrowBookCommandHandler> _logger;

        public BorrowBookCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<BorrowBookCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ELibraryResult<BookDto>> Handle(
            BorrowBookCommand request,
            CancellationToken cancellationToken)
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                _logger.LogInformation(
                    "Handling BorrowBookCommand: BookId={BookId}, Customer={CustomerName}",
                    request.BookId, request.CustomerName);

                // Get the book
                var book = await _unitOfWork.Books.GetByIdAsync(request.BookId);

                if (book == null)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    _logger.LogWarning(
                        "BorrowBookCommand failed: Book not found. BookId={BookId}",
                        request.BookId);

                    return ELibraryResult<BookDto>.Failure(
                        $"Book with ID {request.BookId} not found",
                        ErrorCodes.NotFound);
                }

                // Check stock availability
                if (book.ActualQuantity <= 0)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    _logger.LogWarning(
                        "BorrowBookCommand failed: Out of stock. BookId={BookId}, Title={Title}",
                        request.BookId, book.Name);

                    return ELibraryResult<BookDto>.Failure(
                        $"Book '{book.Name}' is out of stock",
                        ErrorCodes.OutOfStock);
                }

                // Decrease quantity
                book.ActualQuantity -= 1;
                _unitOfWork.Books.Update(book);

                // Create borrow record
                var bookRecord = new BorrowBookRecord
                {
                    BookID = request.BookId,
                    Book = book,
                    CustomerName = request.CustomerName,
                    Action = BookActionType.Borrowed.ToString(),
                    Date = DateTime.UtcNow
                };

                await _unitOfWork.BorrowRecords.AddAsync(bookRecord);

                // Save changes and commit transaction
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation(
                    "BorrowBookCommand succeeded. BookId={BookId}, Customer={CustomerName}, RemainingQuantity={Quantity}",
                    request.BookId, request.CustomerName, book.ActualQuantity);

                return ELibraryResult<BookDto>.Success(_mapper.Map<BookDto>(book));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogError(ex, "BorrowBookCommand failed with exception");
                throw;
            }
        }
    }
}
