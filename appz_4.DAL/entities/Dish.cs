using System.ComponentModel.DataAnnotations;

namespace appz_4.DAL.entities;

public class Dish
{
    [Key]
    public int Id { get; set; }               // Унікальний ідентифікатор страви

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }          // Назва страви

    [MaxLength(500)]
    public string Description { get; set; }   // Опис страви (опційно)

    public decimal Price { get; set; }        // Ціна

    public DishType DishType { get; set; }        // Тип страви (наприклад, Суп, Гарнір, Основна страва)

    public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
}

// Перелік типів страв (можна зберігати в базі як enum)
public enum DishType
{
    Starter,
    MainCourse,
    SideDish,
    Dessert,
    Drink,
    Salad
}