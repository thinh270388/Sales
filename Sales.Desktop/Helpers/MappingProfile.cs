using AutoMapper;
using Sales.Models.DTOs;
using Sales.Models.Entities;

namespace Sales.Desktop.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UsersDto, Users>();
            CreateMap<Users, UsersDto>();

            //CreateMap<UsersResponse, Users>()
            //    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
            //    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.UserName));
        }
    }
}
