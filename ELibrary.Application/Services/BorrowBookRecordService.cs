using AutoMapper;
using ELibrary.Application.DTOs;
using ELibrary.Application.Interfaces;
using ELibrary.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ELibrary.Application.Services
{
    /// <summary>
    /// Book service implementation.
    /// </summary>
    public class BorrowBookRecordService : IBorrowBookRecordService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BorrowBookRecordService> _logger;
        private readonly IMapper _mapper;

        /// <summary>
        /// Creates a new instance of the BorrowBookRecordService class.
        /// </summary>
        /// <param name="unitOfWork">Borrow book repository.</param>
        /// <param name="logger">The logger.</param>
        public BorrowBookRecordService(IUnitOfWork unitOfWork, ILogger<BorrowBookRecordService> logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<BorrowBookRecordDto>> GetAllBorrowBookRecordsAsync()
        {
            var entities = await _unitOfWork.BorrowRecords.GetAllAsync();
            return entities.Select(e => _mapper.Map<BorrowBookRecordDto>(e));
        }
    }
}