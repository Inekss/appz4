using appz_4.DAL.entities;

namespace appz_4.BLL.interfaces;

public interface IMenuService
{
    Task<IEnumerable<Dish>> GetDishesByDateAsync(DateOnly date);
    Task<IEnumerable<Dish>> GetComplexLunchDishesAsync(DateOnly date);
    Task<IEnumerable<Dish>> GetAvailableDishesAsync(DateOnly date);
    Task<bool> AddDishToDayMenuAsync(DateOnly date, int dishId, bool includeInComplex);
    Task<bool> RemoveDishFromDayMenuAsync(DateOnly date, int dishId);
    Task<IEnumerable<Dish>> GetAllDishesAsync();
}