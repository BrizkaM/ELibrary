using ELibrary.Services.Interfaces;
using ELibrary.Shared.DTOs;
using ELibrary.Shared.Entities;
using ELibrary.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace ELibrary.Services.Services
{
    /// <summary>
    /// Book service implementation.
    /// </summary>
    public class BorrowBookRecordService : IBorrowBookRecordService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BorrowBookRecordService> _logger;

        /// <summary>
        /// Creates a new instance of the BorrowBookRecordService class.
        /// </summary>
        /// <param name="unitOfWork">Borrow book repository.</param>
        /// <param name="logger">The logger.</param>
        public BorrowBookRecordService(IUnitOfWork unitOfWork, ILogger<BorrowBookRecordService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<BorrowBookRecordDto>> GetAllBorrowBookRecordsAsync()
        {
            var entities = await _unitOfWork.BorrowRecords.GetAllAsync();
            return entities.Select(MapToDto);
        }

        /// <summary>
        /// Maps BorrowBookRecord entity to BorrowBookRecordDto.
        /// </summary>
        /// <param name="record">The borrow book record.</param>
        /// <returns>The borrow book recortd dto.</returns>
        private static BorrowBookRecordDto MapToDto(BorrowBookRecord record)
        {
            return new BorrowBookRecordDto
            {
                ID = record.ID,
                BookID = record.BookID,
                CustomerName = record.CustomerName,
                Action = record.Action,
                Date = record.Date
            };
        }
    }
}
