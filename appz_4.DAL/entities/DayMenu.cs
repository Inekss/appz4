using System.ComponentModel.DataAnnotations;

namespace appz_4.DAL.entities;

public class DayMenu
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateOnly Date { get; set; }

    public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
}