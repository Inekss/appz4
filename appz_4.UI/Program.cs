using System.Text;
using System.Text.Json;
using appz_4.BLL.DTO;

internal class Program
{
    private static readonly HttpClient _httpClient = new() { BaseAddress = new Uri("http://localhost:5192/") };

    static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("===============================================");
        Console.WriteLine("     СИСТЕМА ЗАМОВЛЕННЯ КОМПЛЕКСНИХ ОБІДІВ");
        Console.WriteLine("===============================================\n");

        bool exit = false;
        while (!exit)
        {
            Console.WriteLine("ОБЕРІТЬ ДІЮ:");
            Console.WriteLine("1 - Показати страви на дату");
            Console.WriteLine("2 - Показати комплексний обід на дату");
            Console.WriteLine("3 - Додати страву до меню на дату");
            Console.WriteLine("4 - Видалити страву з меню на дату");
            Console.WriteLine("5 - Показати всі доступні страви");
            Console.WriteLine("0 - Вихід");
            Console.Write("Ваш вибір: ");

            var input = Console.ReadLine()?.Trim();
            Console.WriteLine();

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
                    Console.WriteLine("Завершення роботи...");
                    break;
                default:
                    Console.WriteLine("Невірна команда. Спробуйте ще раз.");
                    break;
            }

            Console.WriteLine("\n-----------------------------------------------\n");
        }
    }

    private static async Task ShowAllDishesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/dishes");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Помилка при отриманні страв.");
                return;
            }

            var content = await response.Content.ReadAsStringAsync();
            var dishes = JsonSerializer.Deserialize<List<DishDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Console.WriteLine("ВСІ ДОСТУПНІ СТРАВИ:");
            if (dishes is { Count: > 0 })
            {
                foreach (var dish in dishes)
                {
                    Console.WriteLine($"- ID: {dish.Id}, Назва: {dish.Name}, Тип: {dish.DishType}");
                }
            }
            else
            {
                Console.WriteLine("Немає доступних страв.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка: {ex.Message}");
        }
    }

    private static async Task ShowDishesByDateAsync()
    {
        var date = PromptForDate("Введіть дату (рррр-мм-дд): ");
        var response = await _httpClient.GetAsync($"/daymenu/{date:yyyy-MM-dd}");

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("Помилка при отриманні меню на вказану дату.");
            return;
        }

        var content = await response.Content.ReadAsStringAsync();
        var dayMenu = JsonSerializer.Deserialize<DayMenuDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Console.WriteLine($"\nСТРАВИ НА {date:yyyy-MM-dd}:");
        if (dayMenu?.MenuItems is { Count: > 0 })
        {
            foreach (var item in dayMenu.MenuItems)
            {
                var complexLabel = item.IsIncludedInComplex ? " (входить до комплексу)" : "";
                Console.WriteLine($"- {item.Dish.Name} ({item.Dish.DishType}){complexLabel}");
            }
        }
        else
        {
            Console.WriteLine("Меню порожнє.");
        }
    }

    private static async Task ShowComplexLunchAsync()
    {
        var date = PromptForDate("Введіть дату (рррр-мм-дд): ");
        var response = await _httpClient.GetAsync($"/daymenu/{date:yyyy-MM-dd}/complex");

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("Помилка при отриманні комплексного обіду.");
            return;
        }

        var content = await response.Content.ReadAsStringAsync();
        var dishes = JsonSerializer.Deserialize<List<DishDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Console.WriteLine($"\nКОМПЛЕКСНИЙ ОБІД НА {date:yyyy-MM-dd}:");
        if (dishes is { Count: > 0 })
        {
            foreach (var dish in dishes)
            {
                Console.WriteLine($"- {dish.Name} ({dish.DishType})");
            }
        }
        else
        {
            Console.WriteLine("Комплексний обід відсутній.");
        }
    }

    private static async Task AddDishToDayMenuAsync()
    {
        var date = PromptForDate("Введіть дату (рррр-мм-дд): ");

        Console.Write("Введіть ID страви: ");
        if (!int.TryParse(Console.ReadLine(), out int dishId))
        {
            Console.WriteLine("Невірний формат ID.");
            return;
        }

        Console.Write("Включити до комплексного обіду? (y/n): ");
        var includeInComplex = Console.ReadLine()?.Trim().ToLower() == "y";
        
        var url = $"/daymenu/{date:yyyy-MM-dd}/dish/{dishId}?includeInComplex={includeInComplex.ToString().ToLower()}";

        var response = await _httpClient.PostAsync(url, null); // Тіло запиту не потрібне

        Console.WriteLine(response.IsSuccessStatusCode
            ? "Страву додано до меню."
            : $"Не вдалося додати страву. Код: {response.StatusCode}");
    }

    private static async Task RemoveDishFromDayMenuAsync()
    {
        var date = PromptForDate("Введіть дату (рррр-мм-дд): ");

        Console.Write("Введіть ID страви: ");
        if (!int.TryParse(Console.ReadLine(), out int dishId))
        {
            Console.WriteLine("Невірний формат ID.");
            return;
        }

        // Формуємо URL з параметрами маршруту
        var url = $"/daymenu/{date:yyyy-MM-dd}/dish/{dishId}";

        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        var response = await _httpClient.SendAsync(request);

        Console.WriteLine(response.IsSuccessStatusCode
            ? "Страву видалено з меню."
            : $"Не вдалося видалити страву. Код: {response.StatusCode}");
    }

    private static DateOnly PromptForDate(string message)
    {
        Console.Write(message);
        while (true)
        {
            var input = Console.ReadLine();
            if (DateOnly.TryParse(input, out var date))
                return date;

            Console.Write("Невірний формат дати. Спробуйте ще раз: ");
        }
    }
}