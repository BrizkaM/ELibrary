using AutoMapper;
using ELibrary.Application.Common;
using ELibrary.Application.DTOs;
using ELibrary.Application.Interfaces;
using ELibrary.Application.Queries.BorrowRecords;
using ELibrary.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ELibrary.Application.Services
{
    /// <summary>
    /// Borrow book record service implementation using CQRS pattern.
    /// Handles queries for borrow/return history.
    /// </summary>
    public class BorrowBookRecordService : IBorrowBookRecordService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BorrowBookRecordService> _logger;
        private readonly IMapper _mapper;

        public BorrowBookRecordService(IUnitOfWork unitOfWork, ILogger<BorrowBookRecordService> logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Handles GetAllBorrowRecordsQuery to retrieve all borrow/return records.
        /// </summary>
        public async Task<ELibraryResult<IEnumerable<BorrowBookRecordDto>>> HandleAsync(GetAllBorrowRecordsQuery query)
        {
            try
            {
                _logger.LogInformation("Handling GetAllBorrowRecordsQuery");

                var entities = await _unitOfWork.BorrowRecords.GetAllAsync();
                var dtos = entities.Select(e => _mapper.Map<BorrowBookRecordDto>(e));

                _logger.LogInformation("Retrieved {Count} borrow book records", dtos.Count());

                return ELibraryResult<IEnumerable<BorrowBookRecordDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling GetAllBorrowRecordsQuery");
                return ELibraryResult<IEnumerable<BorrowBookRecordDto>>.Failure(
                    "An error occurred while retrieving borrow book records",
                    ErrorCodes.InvalidOperation);
            }
        }
    }
}