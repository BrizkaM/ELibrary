using AutoMapper;
using ELibrary.Application.Common;
using ELibrary.Application.DTOs;
using ELibrary.Domain.Entities;
using ELibrary.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ELibrary.Application.Commands.Books.CreateBook
{
    /// <summary>
    /// Handler for CreateBookCommand.
    /// Processes the command to create a new book in the library.
    /// Transaction management is handled by TransactionBehavior pipeline.
    /// General logging is handled by LoggingBehavior pipeline.
    /// </summary>
    public class CreateBookCommandHandler : IRequestHandler<CreateBookCommand, ELibraryResult<BookDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateBookCommandHandler> _logger;

        public CreateBookCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CreateBookCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ELibraryResult<BookDto>> Handle(
            CreateBookCommand request,
            CancellationToken cancellationToken)
        {
            // Check for duplicate ISBN
            var existingISBN = await _unitOfWork.Books.GetByISBNAsync(request.ISBN);

            if (existingISBN != null)
            {
                // Log business-specific warning
                _logger.LogWarning(
                    "Duplicate ISBN detected: ISBN={ISBN}, ExistingBookId={ExistingBookId}",
                    request.ISBN, existingISBN.ID);

                return ELibraryResult<BookDto>.Failure(
                    $"Book with ISBN '{request.ISBN}' already exists",
                    ErrorCodes.DuplicateIsbn);
            }

            // Create book entity
            var book = new Book
            {
                Name = request.Name,
                Author = request.Author,
                Year = request.Year,
                ISBN = request.ISBN,
                ActualQuantity = request.ActualQuantity
            };

            var addedBook = await _unitOfWork.Books.AddAsync(book);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Log important business event with specific details
            _logger.LogInformation(
                "Book created successfully: BookId={BookId}, Title={Title}, Author={Author}, ISBN={ISBN}, InitialStock={Stock}",
                addedBook.ID, addedBook.Name, addedBook.Author, addedBook.ISBN, addedBook.ActualQuantity);

            return ELibraryResult<BookDto>.Success(_mapper.Map<BookDto>(addedBook));
        }
    }
}