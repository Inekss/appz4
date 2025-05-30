using appz_4.BLL.interfaces;
using appz_4.BLL.DTO;
using appz_4.DAL.entities;
using AutoMapper;
using appz_4.DAL.interfaces;

namespace appz_4.BLL.services;

public class MenuService : IMenuService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public MenuService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<DishDto>> GetComplexLunchDishesAsync(DateOnly date)
    {
        var menuItems = await _unitOfWork.MenuItems.GetAllAsync(
            filter: mi => mi.DayMenu.Date == date && mi.IsIncludedInComplex,
            includeProperties: "Dish,DayMenu"
        );

        var dishes = menuItems.Select(mi => mi.Dish);
        return _mapper.Map<IEnumerable<DishDto>>(dishes);
    }

    public async Task<bool> AddDishToDayMenuAsync(DateOnly date, int dishId, bool includeInComplex)
    {
        var dayMenu = (await _unitOfWork.DayMenus.GetAllAsync(
            filter: dm => dm.Date == date,
            includeProperties: "MenuItems"
        )).FirstOrDefault();

        if (dayMenu == null)
        {
            dayMenu = new DayMenu { Date = date };
            await _unitOfWork.DayMenus.AddAsync(dayMenu);
            await _unitOfWork.SaveChangesAsync();
        }

        if (dayMenu.MenuItems.Any(mi => mi.DishId == dishId))
            return false;

        var newItem = new MenuItem
        {
            DayMenuId = dayMenu.Id,
            DishId = dishId,
            IsIncludedInComplex = includeInComplex
        };

        await _unitOfWork.MenuItems.AddAsync(newItem);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveDishFromDayMenuAsync(DateOnly date, int dishId)
    {
        var menuItems = await _unitOfWork.MenuItems.GetAllAsync(
            filter: mi => mi.DayMenu.Date == date && mi.DishId == dishId,
            includeProperties: "DayMenu"
        );

        var item = menuItems.FirstOrDefault();
        if (item == null)
            return false;

        _unitOfWork.MenuItems.Remove(item);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<DishDto>> GetAllDishesAsync()
    {
        var dishes = await _unitOfWork.Dishes.GetAllAsync();
        return _mapper.Map<IEnumerable<DishDto>>(dishes);
    }

    public async Task<DayMenuDto?> GetDayMenuByDateAsync(DateOnly date)
    {
        var dayMenu = (await _unitOfWork.DayMenus.GetAllAsync(
            filter: dm => dm.Date == date,
            includeProperties: "MenuItems.Dish"
        )).FirstOrDefault();

        return dayMenu == null ? null : _mapper.Map<DayMenuDto>(dayMenu);
    }
}