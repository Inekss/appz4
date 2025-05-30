namespace appz_4.BLL.DTO;

public class MenuItemDto
{
    public int Id { get; set; }

    public int DayMenuId { get; set; }
    public DayMenuDto? DayMenu { get; set; }

    public int DishId { get; set; }
    public DishDto? Dish { get; set; }

    public bool IsIncludedInComplex { get; set; }
}