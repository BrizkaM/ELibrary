using AutoMapper;
using ELibrary.Application.Common;
using ELibrary.Application.DTOs;
using ELibrary.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ELibrary.Application.Queries.Books.GetAllBooks
{
    /// <summary>
    /// Handler for GetAllBooksQuery.
    /// Retrieves all books from the library.
    /// </summary>
    public class GetAllBooksQueryHandler : IRequestHandler<GetAllBooksQuery, ELibraryResult<IEnumerable<BookDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllBooksQueryHandler> _logger;

        public GetAllBooksQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetAllBooksQueryHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ELibraryResult<IEnumerable<BookDto>>> Handle(
            GetAllBooksQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling GetAllBooksQuery");

            var entities = await _unitOfWork.Books.GetAllAsync();
            var dtos = entities.Select(e => _mapper.Map<BookDto>(e));

            _logger.LogInformation("Retrieved {Count} books", dtos.Count());

            return ELibraryResult<IEnumerable<BookDto>>.Success(dtos);
        }
    }
}
