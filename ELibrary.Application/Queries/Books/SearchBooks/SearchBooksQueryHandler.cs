using AutoMapper;
using ELibrary.Application.Common;
using ELibrary.Application.DTOs;
using ELibrary.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ELibrary.Application.Queries.Books.SearchBooks
{
    /// <summary>
    /// Handler for SearchBooksQuery.
    /// Searches books based on provided criteria.
    /// </summary>
    public class SearchBooksQueryHandler : IRequestHandler<SearchBooksQuery, ELibraryResult<IEnumerable<BookDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SearchBooksQueryHandler> _logger;

        public SearchBooksQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<SearchBooksQueryHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ELibraryResult<IEnumerable<BookDto>>> Handle(
            SearchBooksQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Handling SearchBooksQuery with criteria: Name={Name}, Author={Author}, ISBN={ISBN}",
                request.Name ?? "null", request.Author ?? "null", request.ISBN ?? "null");

            var entities = await _unitOfWork.Books.GetFilteredBooksAsync(
                request.Name,
                request.Author,
                request.ISBN);

            var dtos = entities.Select(e => _mapper.Map<BookDto>(e));

            _logger.LogInformation("Search completed. Found {Count} books", dtos.Count());

            return ELibraryResult<IEnumerable<BookDto>>.Success(dtos);
        }
    }
}
