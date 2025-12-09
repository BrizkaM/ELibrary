using AutoMapper;
using ELibrary.Application.DTOs;
using ELibrary.Domain.Entities;

namespace ELibrary.Application.Mappings
{
    /// <summary>
    /// AutoMapper profile for Book entity and DTOs.
    /// </summary>
    public class BookMappingProfile : Profile
    {
        public BookMappingProfile()
        {
            // Book Entity <-> BookDto
            CreateMap<Book, BookDto>()
                .ReverseMap();

            // If you need custom mapping, you can do:
            // CreateMap<Book, BookDto>()
            //     .ForMember(dest => dest.SomeProperty, opt => opt.MapFrom(src => src.SomeOtherProperty));
        }
    }
}