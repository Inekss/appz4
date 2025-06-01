using appz_4.BLL.DTO;
using appz_4.PL.models;

namespace appz_4.PL;

public static class DayMenuMapper
{
    public static DayMenuDto ToDto(DayMenuModel model) => new DayMenuDto
    {
        Id = model.Id,
        Date = model.Date,
        MenuItems = model.MenuItems.Select(MenuItemMapper.ToDto).ToList()
    };

    public static DayMenuModel ToModel(DayMenuDto dto) => new DayMenuModel
    {
        Id = dto.Id,
        Date = dto.Date,
        MenuItems = dto.MenuItems.Select(MenuItemMapper.ToModel).ToList()
    };
}
