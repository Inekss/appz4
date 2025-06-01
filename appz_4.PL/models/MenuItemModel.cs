namespace appz_4.PL.models;

public class MenuItemModel
{
    public int Id { get; set; }

    public int DayMenuId { get; set; }

    public int DishId { get; set; }

    public bool IsIncludedInComplex { get; set; }

    public DishModel? Dish { get; set; }
}