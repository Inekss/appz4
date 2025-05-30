using appz_4.DAL.entities;

namespace appz_4.DAL.interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Dish> Dishes { get; }
    IRepository<DayMenu> DayMenus { get; }
    IRepository<MenuItem> MenuItems { get; }

    Task<int> SaveChangesAsync();
}