using appz_4.BLL.DTO;
using appz_4.PL.models;

namespace appz_4.PL;

public static class MenuItemMapper
{
    public static MenuItemDto ToDto(MenuItemModel model) => new MenuItemDto
    {
        Id = model.Id,
        DayMenuId = model.DayMenuId,
        DishId = model.DishId,
        IsIncludedInComplex = model.IsIncludedInComplex,
        Dish = model.Dish is not null ? DishMapper.ToDto(model.Dish) : null
    };

    public static MenuItemModel ToModel(MenuItemDto dto) => new MenuItemModel
    {
        Id = dto.Id,
        DayMenuId = dto.DayMenuId,
        DishId = dto.DishId,
        IsIncludedInComplex = dto.IsIncludedInComplex,
        Dish = dto.Dish is not null ? DishMapper.ToModel(dto.Dish) : null
    };
}