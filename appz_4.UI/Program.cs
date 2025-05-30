
using appz_4.BLL.interfaces;
using appz_4.BLL.factories;
using appz_4.BLL.services;
using appz_4.DAL.context;
using appz_4.DAL.entities;
using Microsoft.EntityFrameworkCore;
using Autofac;
using AutoMapper;
using appz_4.BLL;
using appz_4.DAL;
using appz_4.DAL.interfaces;

internal class Program
{
    private static IMenuService _menuService = null!;

static async Task Main(string[] args)
    {
        var builder = new ContainerBuilder();
        
        var connectionString = "Host=localhost;Port=5432;Database=LunchDb;Username=postgres;Password=your_password";

        // Реєструємо DbContext з цим рядком підключення
        builder.Register(ctx =>
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(connectionString);
            return new ApplicationDbContext(optionsBuilder.Options);
        }).AsSelf().InstancePerLifetimeScope();

        builder.RegisterType<UnitOfWork>()
            .As<IUnitOfWork>()
            .InstancePerLifetimeScope();
        
        builder.Register(ctx =>
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(connectionString);
            return new ApplicationDbContext(optionsBuilder.Options);
        }).AsSelf().InstancePerLifetimeScope();

        // AutoMapper
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        builder.RegisterInstance(mapperConfig.CreateMapper()).As<IMapper>().SingleInstance();

        // Реєстрація сервісів
        builder.RegisterType<MenuService>().As<IMenuService>().InstancePerLifetimeScope();
        builder.RegisterType<StarterDishFactory>().As<IDishFactory>().InstancePerLifetimeScope();

        // Побудова контейнера
        var container = builder.Build();

        // Створення області життєвого циклу
        using var scope = container.BeginLifetimeScope();

        _menuService = scope.Resolve<IMenuService>();
        var dbContext = scope.Resolve<ApplicationDbContext>();

        // Seed даних
        await SeedDishesAsync(dbContext);

        Console.WriteLine("Вітаємо у системі замовлення комплексних обідів!\n");

        bool exit = false;
        while (!exit)
        {
            Console.WriteLine("Оберіть дію:");
            Console.WriteLine("1 - Показати страви на дату");
            Console.WriteLine("2 - Показати комплексний обід на дату");
            Console.WriteLine("3 - Додати страву до меню на дату");
            Console.WriteLine("4 - Видалити страву з меню на дату");
            Console.WriteLine("5 - Показати всі доступні страви");
            Console.WriteLine("0 - Вихід");

            var input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    await ShowDishesByDateAsync();
                    break;
                case "2":
                    await ShowComplexLunchAsync();
                    break;
                case "3":
                    await AddDishToDayMenuAsync();
                    break;
                case "4":
                    await RemoveDishFromDayMenuAsync();
                    break;
                case "5":
                    await ShowAllDishesAsync();
                    break;
                case "0":
                    exit = true;
                    break;
                default:
                    Console.WriteLine("Невірна команда. Спробуйте ще раз.");
                    break;
            }

            Console.WriteLine();
        }
    }
    private static async Task ShowDishesByDateAsync()
    {
        var date = PromptForDate("Введіть дату (рррр-мм-дд): ");
        var dayMenu = await _menuService.GetDayMenuByDateAsync(date);

        if (dayMenu == null || dayMenu.MenuItems == null || !dayMenu.MenuItems.Any())
        {
            Console.WriteLine($"Меню на {date} порожнє.");
            return;
        }

        Console.WriteLine($"\nСтрави на {date}:");
        foreach (var item in dayMenu.MenuItems)
        {
            var complexLabel = item.IsIncludedInComplex ? "[Комплекс]" : "";
            Console.WriteLine($"- {item.Dish.Name} ({item.Dish.DishType}) {complexLabel}");
        }
    }

    private static async Task ShowComplexLunchAsync()
    {
        var date = PromptForDate("Введіть дату (рррр-мм-дд): ");
        var dishes = await _menuService.GetComplexLunchDishesAsync(date);

        Console.WriteLine($"\nКомплексний обід на {date}:");
        foreach (var dish in dishes)
        {
            Console.WriteLine($"- {dish.Name} ({dish.DishType})");
        }
    }

    private static async Task AddDishToDayMenuAsync()
    {
        var date = PromptForDate("Введіть дату (рррр-мм-дд): ");
        Console.Write("Введіть ID страви: ");
        int.TryParse(Console.ReadLine(), out int dishId);

        Console.Write("Включити до комплексного обіду? (y/n): ");
        var includeInComplex = Console.ReadLine()?.Trim().ToLower() == "y";

        var result = await _menuService.AddDishToDayMenuAsync(date, dishId, includeInComplex);
        Console.WriteLine(result ? "Страву додано." : "Не вдалося додати страву.");
    }

    private static async Task RemoveDishFromDayMenuAsync()
    {
        var date = PromptForDate("Введіть дату (рррр-мм-дд): ");
        Console.Write("Введіть ID страви: ");
        int.TryParse(Console.ReadLine(), out int dishId);

        var result = await _menuService.RemoveDishFromDayMenuAsync(date, dishId);
        Console.WriteLine(result ? "Страву видалено." : "Не вдалося видалити страву.");
    }

    private static async Task ShowAllDishesAsync()
    {
        var dishes = await _menuService.GetAllDishesAsync();
        Console.WriteLine("\nВсі доступні страви:");
        foreach (var dish in dishes)
        {
            Console.WriteLine($"ID: {dish.Id} - {dish.Name} ({dish.DishType})");
        }
    }

    private static DateOnly PromptForDate(string message)
    {
        Console.Write(message);
        DateOnly date;
        while (!DateOnly.TryParse(Console.ReadLine(), out date))
        {
            Console.Write("Невірний формат дати. Спробуйте ще раз: ");
        }
        return date;
    }

    private static async Task SeedDishesAsync(ApplicationDbContext context)
    {
        if (!context.Dishes.Any())
        {
            var dishFactory = new StarterDishFactory();

            var dishes = new List<Dish>
            {
                dishFactory.CreateDish("Борщ", "Традиційний український суп", 50, DishType.Starter),
                dishFactory.CreateDish("Картопляне пюре", "З вершковим маслом", 30, DishType.SideDish),
                dishFactory.CreateDish("Котлета по-київськи", "Класична страва", 80, DishType.MainCourse),
                dishFactory.CreateDish("Салат Олів'є", "Салат з овочами та ковбасою", 35, DishType.Salad),
                dishFactory.CreateDish("Компот", "Фруктовий напій", 20, DishType.Drink)
            };

            context.Dishes.AddRange(dishes);
            await context.SaveChangesAsync();
        }
    }
}