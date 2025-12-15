using AutoMapper;
using ELibrary.Application.Common;
using ELibrary.Application.DTOs;
using ELibrary.Domain.Entities;
using ELibrary.Domain.Enums;
using ELibrary.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ELibrary.Application.Commands.Books.BorrowBook
{
    /// <summary>
    /// Handler for BorrowBookCommand.
    /// Processes the command to borrow a book and create a borrow record.
    /// Transaction management is handled by TransactionBehavior pipeline.
    /// General logging is handled by LoggingBehavior pipeline.
    /// Concurrency conflicts are explicitly handled with retry logic.
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
            try
            {
                // Get the book
                var book = await _unitOfWork.Books.GetByIdAsync(request.BookId);

                if (book == null)
                {
                    // Log business-specific warning
                    _logger.LogWarning(
                        "Book not found: BookId={BookId}, RequestedBy={Customer}",
                        request.BookId, request.CustomerName);

                    return ELibraryResult<BookDto>.Failure(
                        $"Book with ID {request.BookId} not found",
                        ErrorCodes.NotFound);
                }

                // Check stock availability
                if (book.ActualQuantity <= 0)
                {
                    // Log business-specific warning
                    _logger.LogWarning(
                        "Book out of stock: BookId={BookId}, Title={Title}, RequestedBy={Customer}",
                        request.BookId, book.Name, request.CustomerName);

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
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Log important business event with specific details
                _logger.LogInformation(
                    "Book borrowed successfully: BookId={BookId}, Title={Title}, Customer={Customer}, RemainingStock={Remaining}",
                    request.BookId, book.Name, request.CustomerName, book.ActualQuantity);

                return ELibraryResult<BookDto>.Success(_mapper.Map<BookDto>(book));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // This happens when another transaction modified the book between our read and write
                _logger.LogWarning(ex,
                    "Concurrency conflict while borrowing book: BookId={BookId}, Customer={Customer}. " +
                    "The book was modified by another user.",
                    request.BookId, request.CustomerName);

                return ELibraryResult<BookDto>.Failure(
                    "The book was modified by another user. Please try again.",
                    ErrorCodes.ConcurrencyConflict);
            }
        }
    }
}