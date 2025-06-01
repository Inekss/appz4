namespace appz_4.PL.models;

public class DayMenuModel
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }

    public List<MenuItemModel> MenuItems { get; set; } = new();
}