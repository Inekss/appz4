using System.ComponentModel.DataAnnotations;

namespace appz_4.DAL.entities;

public class Dish
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }

    public decimal Price { get; set; }

    public DishType DishType { get; set; }

    public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
}

public enum DishType
{
    Starter,
    MainCourse,
    SideDish,
    Dessert,
    Drink,
    Salad
}