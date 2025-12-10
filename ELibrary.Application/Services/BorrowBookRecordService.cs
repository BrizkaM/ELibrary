using AutoMapper;
using ELibrary.Application.Common;
using ELibrary.Application.DTOs;
using ELibrary.Application.Interfaces;
using ELibrary.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ELibrary.Application.Services
{
    /// <summary>
    /// Borrow book record service implementation.
    /// </summary>
    public class BorrowBookRecordService : IBorrowBookRecordService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BorrowBookRecordService> _logger;
        private readonly IMapper _mapper;

        /// <summary>
        /// Creates a new instance of the BorrowBookRecordService class.
        /// </summary>
        /// <param name="unitOfWork">Unit of work.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="mapper">The mapper.</param>
        public BorrowBookRecordService(IUnitOfWork unitOfWork, ILogger<BorrowBookRecordService> logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <inheritdoc/>
        public async Task<ELibraryResult<IEnumerable<BorrowBookRecordDto>>> GetAllBorrowBookRecordsAsync()
        {
            try
            {
                var entities = await _unitOfWork.BorrowRecords.GetAllAsync();
                var dtos = entities.Select(e => _mapper.Map<BorrowBookRecordDto>(e));

                _logger.LogInformation("Retrieved {Count} borrow book records", dtos.Count());

                return ELibraryResult<IEnumerable<BorrowBookRecordDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving borrow book records");
                return ELibraryResult<IEnumerable<BorrowBookRecordDto>>.Failure(
                    "An error occurred while retrieving borrow book records",
                    ErrorCodes.InvalidOperation);
            }
        }
    }
}