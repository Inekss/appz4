using appz_4.DAL.entities;

namespace appz_4.BLL.factories;

public interface IDishFactory
{
    Dish CreateDish(string name, string description, decimal price, DishType  dishType);
}