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
            _logger.LogInformation(
                "Handling CreateBookCommand: Name={Name}, Author={Author}, ISBN={ISBN}",
                request.Name, request.Author, request.ISBN);

            // Check for duplicate ISBN
            var existingISBN = await _unitOfWork.Books.GetByISBNAsync(request.ISBN);
            if (existingISBN != null)
            {
                _logger.LogWarning(
                    "CreateBookCommand failed: Duplicate ISBN={ISBN}",
                    request.ISBN);

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

            _logger.LogInformation(
                "CreateBookCommand succeeded. BookId={BookId}, Title={Title}",
                addedBook.ID, addedBook.Name);

            return ELibraryResult<BookDto>.Success(_mapper.Map<BookDto>(addedBook));
        }
    }
}
