using appz_4.BLL.DTO;
using AutoMapper;
using appz_4.DAL.entities;

namespace appz_4.BLL;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Dish, DishDto>().ReverseMap();
        CreateMap<MenuItem, MenuItemDto>().ReverseMap();
        CreateMap<DayMenu, DayMenuDto>().ReverseMap();
    }
}