
using appz_4.BLL.interfaces;
using appz_4.BLL.services;
using appz_4.DAL.context;
using appz_4.DAL.entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

internal class Program
    {
        private static IMenuService _menuService;

        static async Task Main(string[] args)
        {
            // Налаштування DI та DbContext
            var services = new ServiceCollection();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("LunchDb")); // або UseSqlServer(...) для SQL

            services.AddScoped<IMenuService, MenuService>();

            var serviceProvider = services.BuildServiceProvider();
            _menuService = serviceProvider.GetRequiredService<IMenuService>();
            
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
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
            var dishes = await _menuService.GetDishesByDateAsync(date);

            Console.WriteLine($"\nСтрави на {date}:");
            foreach (var dish in dishes)
            {
                Console.WriteLine($"- {dish.Name} ({dish.DishType})");
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
                var dishes = new List<Dish>
                {
                    new Dish { Name = "Борщ", Description = "Традиційний український суп", Price = 50, DishType = DishType.Starter },
                    new Dish { Name = "Картопляне пюре", Description = "З вершковим маслом", Price = 30, DishType = DishType.SideDish },
                    new Dish { Name = "Котлета по-київськи", Description = "Класична страва", Price = 80, DishType = DishType.MainCourse },
                    new Dish { Name = "Салат Олів'є", Description = "Салат з овочами та ковбасою", Price = 35, DishType = DishType.Salad },
                    new Dish { Name = "Компот", Description = "Фруктовий напій", Price = 20, DishType = DishType.Drink }
                };

                context.Dishes.AddRange(dishes);
                await context.SaveChangesAsync();
            }
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
    }
    
    