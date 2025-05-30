using appz_4.DAL.interfaces;
using appz_4.DAL.entities;
using appz_4.DAL.repositories;
using appz_4.DAL.context;

namespace appz_4.DAL;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public IRepository<Dish> Dishes { get; }
    public IRepository<DayMenu> DayMenus { get; }
    public IRepository<MenuItem> MenuItems { get; }

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Dishes = new Repository<Dish>(_context);
        DayMenus = new Repository<DayMenu>(_context);
        MenuItems = new Repository<MenuItem>(_context);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}