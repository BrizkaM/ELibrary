using AutoMapper;
using ELibrary.Application.DTOs;
using ELibrary.Domain.Entities;

namespace ELibrary.Application.Mappings
{
    /// <summary>
    /// AutoMapper profile for BorrowBookRecord entity and DTOs.
    /// </summary>
    public class BorrowBookRecordMappingProfile : Profile
    {
        public BorrowBookRecordMappingProfile()
        {
            // BorrowBookRecord Entity <-> BorrowBookRecordDto
            CreateMap<BorrowBookRecord, BorrowBookRecordDto>()
                .ReverseMap();
        }
    }
}