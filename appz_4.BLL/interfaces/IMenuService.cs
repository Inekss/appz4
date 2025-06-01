using appz_4.BLL.DTO;

namespace appz_4.BLL.interfaces;

public interface IMenuService
{
    Task<IEnumerable<DishDto>> GetComplexLunchDishesAsync(DateOnly date);
    Task<bool> AddDishToDayMenuAsync(DateOnly date, int dishId, bool includeInComplex);
    Task<bool> RemoveDishFromDayMenuAsync(DateOnly date, int dishId);
    Task<IEnumerable<DishDto>> GetAllDishesAsync();
    Task<DayMenuDto?> GetDayMenuByDateAsync(DateOnly date);
    Task<bool> UpdateDishAsync(int dishId, DishDto updatedDish);
}