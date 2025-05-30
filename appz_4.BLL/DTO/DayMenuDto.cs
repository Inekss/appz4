namespace appz_4.BLL.DTO;

public class DayMenuDto
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public List<MenuItemDto> MenuItems { get; set; } = new();
}