using appz_4.BLL;
using appz_4.BLL.factories;
using appz_4.BLL.interfaces;
using appz_4.BLL.services;
using appz_4.DAL;
using appz_4.DAL.context;
using appz_4.DAL.interfaces;
using appz_4.DAL.entities;
using appz_4.PL;
using appz_4.PL.models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                      ?? "Host=localhost;Port=5432;Database=LunchDb;Username=postgres;Password=your_password";

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// UnitOfWork + Services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IMenuService, MenuService>();

// Register DishFactory
builder.Services.AddScoped<IDishFactory, StarterDishFactory>();

// AutoMapper
builder.Services.AddAutoMapper(cfg => {
    cfg.AddProfile<MappingProfile>();
});

// Swagger
// http://localhost:5192/swagger/index.html
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await SeedDishesAsync(dbContext, new StarterDishFactory());
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

#region Routes

app.MapGet("/ping", () => Results.Ok("pong")).WithName("Ping");

app.MapGet("/dishes", async (IMenuService menuService) =>
{
    var dishes = await menuService.GetAllDishesAsync();
    return Results.Ok(dishes.Select(DishMapper.ToModel));
}).WithName("GetAllDishes");

app.MapGet("/daymenu/{date}", async (IMenuService menuService, DateOnly date) =>
{
    var dayMenu = await menuService.GetDayMenuByDateAsync(date);
    return dayMenu is null
        ? Results.NotFound()
        : Results.Ok(DayMenuMapper.ToModel(dayMenu));
}).WithName("GetDayMenuByDate");

app.MapPost("/daymenu/{date}/dish/{dishId}", async (IMenuService menuService, DateOnly date, int dishId, bool includeInComplex) =>
{
    var result = await menuService.AddDishToDayMenuAsync(date, dishId, includeInComplex);
    return result ? Results.Ok() : Results.BadRequest();
}).WithName("AddDishToDayMenu");

app.MapDelete("/daymenu/{date}/dish/{dishId}", async (IMenuService menuService, DateOnly date, int dishId) =>
{
    var result = await menuService.RemoveDishFromDayMenuAsync(date, dishId);
    return result ? Results.Ok() : Results.BadRequest();
}).WithName("RemoveDishFromDayMenu");

app.MapGet("/daymenu/{date}/complex", async (IMenuService menuService, DateOnly date) =>
{
    var dishes = await menuService.GetComplexLunchDishesAsync(date);
    return Results.Ok(dishes.Select(DishMapper.ToModel));
}).WithName("GetComplexLunchDishes");

app.MapPut("/dishes/{dishId}", async (IMenuService menuService, int dishId, DishModel updatedModel) =>
{
    var updatedDto = DishMapper.ToDto(updatedModel);
    var result = await menuService.UpdateDishAsync(dishId, updatedDto);
    return result ? Results.Ok() : Results.NotFound();
}).WithName("UpdateDish");

#endregion

// dotnet run --project ./appz_4.PL
app.Run();

static async Task SeedDishesAsync(ApplicationDbContext context, IDishFactory dishFactory)
{
    if (!context.Dishes.Any())
    {
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