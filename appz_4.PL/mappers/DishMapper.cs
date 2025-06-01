using appz_4.BLL.DTO;
using appz_4.PL.models;

namespace appz_4.PL;

public static class DishMapper
{
    public static DishDto ToDto(DishModel model) => new DishDto
    {
        Id = model.Id,
        Name = model.Name,
        Description = model.Description,
        Price = model.Price,
        DishType = model.DishType
    };

    public static DishModel ToModel(DishDto dto) => new DishModel
    {
        Id = dto.Id,
        Name = dto.Name,
        Description = dto.Description,
        Price = dto.Price,
        DishType = dto.DishType
    };
}