using Microsoft.EntityFrameworkCore;
using appz_4.BLL.interfaces;
using appz_4.DAL.context;
using appz_4.DAL.entities;

namespace appz_4.BLL.services;

public class MenuService : IMenuService
    {
        private readonly ApplicationDbContext _context;

        public MenuService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Dish>> GetDishesByDateAsync(DateOnly date)
        {
            return await _context.MenuItems
                .Include(mi => mi.Dish)
                .Include(mi => mi.DayMenu)
                .Where(mi => mi.DayMenu.Date == date)
                .Select(mi => mi.Dish)
                .ToListAsync();
        }

        public async Task<IEnumerable<Dish>> GetComplexLunchDishesAsync(DateOnly date)
        {
            return await _context.MenuItems
                .Include(mi => mi.Dish)
                .Include(mi => mi.DayMenu)
                .Where(mi => mi.DayMenu.Date == date && mi.IsIncludedInComplex)
                .Select(mi => mi.Dish)
                .ToListAsync();
        }

        public async Task<IEnumerable<Dish>> GetAvailableDishesAsync(DateOnly date)
        {
            return await GetDishesByDateAsync(date);
        }

        public async Task<bool> AddDishToDayMenuAsync(DateOnly date, int dishId, bool includeInComplex)
        {
            var dayMenu = await _context.DayMenus
                .Include(dm => dm.MenuItems)
                .FirstOrDefaultAsync(dm => dm.Date == date);

            if (dayMenu == null)
            {
                dayMenu = new DayMenu { Date = date };
                _context.DayMenus.Add(dayMenu);
                await _context.SaveChangesAsync(); // Для отримання Id
            }

            var alreadyExists = dayMenu.MenuItems.Any(mi => mi.DishId == dishId);
            if (alreadyExists)
                return false;

            var newItem = new MenuItem
            {
                DayMenuId = dayMenu.Id,
                DishId = dishId,
                IsIncludedInComplex = includeInComplex
            };

            _context.MenuItems.Add(newItem);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveDishFromDayMenuAsync(DateOnly date, int dishId)
        {
            var item = await _context.MenuItems
                .Include(mi => mi.DayMenu)
                .FirstOrDefaultAsync(mi => mi.DayMenu.Date == date && mi.DishId == dishId);

            if (item == null)
                return false;

            _context.MenuItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<IEnumerable<Dish>> GetAllDishesAsync()
        {
            return await _context.Dishes.ToListAsync();
        }
    }