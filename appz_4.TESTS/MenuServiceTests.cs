using System.Linq.Expressions;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Kernel;
using AutoMapper;
using NSubstitute;
using Xunit;
using appz_4.BLL.services;
using appz_4.DAL.entities;
using appz_4.BLL.DTO;
using appz_4.DAL.interfaces;

namespace appz_4.TESTS;

public class MenuServiceTests
{
    private readonly IFixture _fixture;
    private readonly IUnitOfWork _unitOfWorkSubstitute;
    private readonly IMapper _mapperSubstitute;
    private readonly MenuService _menuService;

    public MenuServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        
        _fixture.Customizations.Add(new DateOnlyBuilder());

        _unitOfWorkSubstitute = _fixture.Freeze<IUnitOfWork>();
        _mapperSubstitute = _fixture.Freeze<IMapper>();

        _menuService = new MenuService(_unitOfWorkSubstitute, _mapperSubstitute);
    }

    [Fact]
    public async Task GetAllDishesAsync()
    {
        var dishes = new List<Dish>
        {
            new Dish { Id = 1, Name = "Borscht" },
            new Dish { Id = 2, Name = "Varenyky" }
        };

        var expected = new List<DishDto>
        {
            new DishDto { Id = 1, Name = "Borscht" },
            new DishDto { Id = 2, Name = "Varenyky" }
        };

        _unitOfWorkSubstitute.Dishes.GetAllAsync()
            .Returns(Task.FromResult<IEnumerable<Dish>>(dishes));

        _mapperSubstitute.Map<IEnumerable<DishDto>>(dishes)
            .Returns(expected);

        var result = await _menuService.GetAllDishesAsync();

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task GetComplexLunchDishesAsync()
    {
        var date = DateOnly.FromDateTime(DateTime.Today);

        var dish1 = new Dish { Id = 1, Name = "Borscht" };
        var dish2 = new Dish { Id = 2, Name = "Varenyky" };

        var dayMenu = new DayMenu { Date = date };

        var menuItems = new List<MenuItem>
        {
            new MenuItem { Dish = dish1, IsIncludedInComplex = true, DayMenu = dayMenu },
            new MenuItem { Dish = dish2, IsIncludedInComplex = true, DayMenu = dayMenu }
        };

        var expectedDtos = new List<DishDto>
        {
            new DishDto { Id = dish1.Id, Name = dish1.Name },
            new DishDto { Id = dish2.Id, Name = dish2.Name }
        };

        _unitOfWorkSubstitute.MenuItems.GetAllAsync(
            Arg.Any<Expression<Func<MenuItem, bool>>>(), "Dish,DayMenu"
        ).Returns(Task.FromResult<IEnumerable<MenuItem>>(menuItems));

        _mapperSubstitute.Map<IEnumerable<DishDto>>(Arg.Any<IEnumerable<Dish>>())
            .Returns(expectedDtos);

        var result = await _menuService.GetComplexLunchDishesAsync(date);

        Assert.Equal(expectedDtos, result);
    }

    [Fact]
    public async Task AddDishToDayMenuAsync()
    {
        var date = _fixture.Create<DateOnly>();
        var dishId = _fixture.Create<int>();
        var dayMenu = new DayMenu { Id = 1, Date = date, MenuItems = new List<MenuItem>() };

        _unitOfWorkSubstitute.DayMenus.GetAllAsync(
            Arg.Any<Expression<Func<DayMenu, bool>>>(), "MenuItems"
        ).Returns(Task.FromResult<IEnumerable<DayMenu>>(new List<DayMenu>()));

        _unitOfWorkSubstitute.DayMenus.AddAsync(Arg.Do<DayMenu>(dm => dayMenu = dm)).Returns(Task.CompletedTask);
        _unitOfWorkSubstitute.SaveChangesAsync().Returns(Task.FromResult(1)); // ✅ Виправлено

        _unitOfWorkSubstitute.MenuItems.AddAsync(Arg.Any<MenuItem>()).Returns(Task.CompletedTask);

        var result = await _menuService.AddDishToDayMenuAsync(date, dishId, true);

        Assert.True(result);
        await _unitOfWorkSubstitute.DayMenus.Received().AddAsync(Arg.Any<DayMenu>());
        await _unitOfWorkSubstitute.MenuItems.Received().AddAsync(Arg.Is<MenuItem>(mi =>
            mi.DishId == dishId &&
            mi.IsIncludedInComplex &&
            mi.DayMenuId == dayMenu.Id));
    }

    [Fact]
    public async Task AddDishToDayMenuAsyncWhenDishAlreadyExists()
    {
        var date = _fixture.Create<DateOnly>();
        var dishId = _fixture.Create<int>();

        var existingItem = new MenuItem { DishId = dishId };
        var dayMenu = new DayMenu { Date = date, MenuItems = new List<MenuItem> { existingItem } };

        _unitOfWorkSubstitute.DayMenus.GetAllAsync(
            Arg.Any<Expression<Func<DayMenu, bool>>>(), "MenuItems"
        ).Returns(Task.FromResult<IEnumerable<DayMenu>>(new[] { dayMenu }));

        var result = await _menuService.AddDishToDayMenuAsync(date, dishId, true);
        Assert.False(result);
    }

    [Fact]
    public async Task RemoveDishFromDayMenuAsync()
    {
        var date = _fixture.Create<DateOnly>();
        var dishId = _fixture.Create<int>();

        var item = new MenuItem { DishId = dishId, DayMenu = new DayMenu { Date = date } };
        _unitOfWorkSubstitute.MenuItems.GetAllAsync(
            Arg.Any<Expression<Func<MenuItem, bool>>>(), "DayMenu"
        ).Returns(Task.FromResult<IEnumerable<MenuItem>>(new[] { item }));

        var result = await _menuService.RemoveDishFromDayMenuAsync(date, dishId);
        Assert.True(result);
        _unitOfWorkSubstitute.MenuItems.Received().Remove(item);
    }

    [Fact]
    public async Task RemoveDishFromDayMenuAsyncWhenItemNotFound()
    {
        var date = _fixture.Create<DateOnly>();
        var dishId = _fixture.Create<int>();

        _unitOfWorkSubstitute.MenuItems.GetAllAsync(
            Arg.Any<Expression<Func<MenuItem, bool>>>(), "DayMenu"
        ).Returns(Task.FromResult<IEnumerable<MenuItem>>(Array.Empty<MenuItem>()));

        var result = await _menuService.RemoveDishFromDayMenuAsync(date, dishId);
        Assert.False(result);
    }

    [Fact]
    public async Task GetDayMenuByDateAsync()
    {
        var date = DateOnly.FromDateTime(DateTime.Today);

        var dish = new Dish { Id = 1, Name = "Borscht" };
        var menuItem = new MenuItem { Dish = dish };

        var dayMenu = new DayMenu
        {
            Date = date,
            MenuItems = new List<MenuItem> { menuItem }
        };

        var dto = new DayMenuDto
        {
            Date = date,
            MenuItems = new List<MenuItemDto>
            {
                new MenuItemDto
                {
                    Dish = new DishDto { Id = dish.Id, Name = dish.Name }
                }
            }
        };

        _unitOfWorkSubstitute.DayMenus.GetAllAsync(
            Arg.Any<Expression<Func<DayMenu, bool>>>(),
            "MenuItems.Dish"
        ).Returns(Task.FromResult<IEnumerable<DayMenu>>(new[] { dayMenu }));

        _mapperSubstitute.Map<DayMenuDto>(dayMenu).Returns(dto);

        var result = await _menuService.GetDayMenuByDateAsync(date);

        Assert.Equal(dto, result);
    }
    
    [Fact]
    public async Task GetDayMenuByDateAsyncWhenNotFound()
    {
        var date = _fixture.Create<DateOnly>();

        _unitOfWorkSubstitute.DayMenus.GetAllAsync(
            Arg.Any<Expression<Func<DayMenu, bool>>>(), "MenuItems.Dish"
        ).Returns(Task.FromResult<IEnumerable<DayMenu>>(Array.Empty<DayMenu>()));

        var result = await _menuService.GetDayMenuByDateAsync(date);
        Assert.Null(result);
    }

    // Кастомний генератор DateOnly
    private class DateOnlyBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            if (request is Type type && type == typeof(DateOnly))
            {
                var dateTime = context.Create<DateTime>();
                return DateOnly.FromDateTime(dateTime);
            }
            return new NoSpecimen();
        }
    }
}
