using AutoMapper;
using ELibrary.Application.Common;
using ELibrary.Application.DTOs;
using ELibrary.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ELibrary.Application.Queries.BorrowRecords.GetAllBorrowRecords
{
    /// <summary>
    /// Handler for GetAllBorrowRecordsQuery.
    /// Retrieves all borrow/return records from the library.
    /// </summary>
    public class GetAllBorrowRecordsQueryHandler
        : IRequestHandler<GetAllBorrowRecordsQuery, ELibraryResult<IEnumerable<BorrowBookRecordDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllBorrowRecordsQueryHandler> _logger;

        public GetAllBorrowRecordsQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetAllBorrowRecordsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ELibraryResult<IEnumerable<BorrowBookRecordDto>>> Handle(
            GetAllBorrowRecordsQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling GetAllBorrowRecordsQuery");

            var entities = await _unitOfWork.BorrowRecords.GetAllAsync();
            var dtos = entities.Select(e => _mapper.Map<BorrowBookRecordDto>(e));

            _logger.LogInformation("Retrieved {Count} borrow book records", dtos.Count());

            return ELibraryResult<IEnumerable<BorrowBookRecordDto>>.Success(dtos);
        }
    }
}
