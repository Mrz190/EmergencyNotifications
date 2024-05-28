using API.Dto;
using API.Entity;
using AutoMapper;

namespace API.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<RegDto, AppUser>();
            CreateMap<GetUsersDto, AppUser>();
            CreateMap<NewContactDto, Contact>();
            CreateMap<Contact, NewContactDto>();
            CreateMap<EditContactDto, Contact>();
            CreateMap<Contact, EditContactDto>();
        }
    }
}
