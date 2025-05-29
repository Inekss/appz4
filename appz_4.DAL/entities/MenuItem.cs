using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace appz_4.DAL.entities;

public class MenuItem
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(DayMenu))]
    public int DayMenuId { get; set; }
    public DayMenu DayMenu { get; set; }

    [ForeignKey(nameof(Dish))]
    public int DishId { get; set; }
    public Dish Dish { get; set; }

    public bool IsIncludedInComplex { get; set; }  // Чи входить у комплексний обід
}