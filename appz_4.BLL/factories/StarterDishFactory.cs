using appz_4.DAL.entities;


namespace appz_4.BLL.factories;

public class StarterDishFactory : IDishFactory
{
    public Dish CreateDish(string name, string description, decimal price, DishType  dishType)
    {
        return new Dish
        {
            Name = name,
            Description = description,
            Price = price,
            DishType = dishType
        };
    }
}