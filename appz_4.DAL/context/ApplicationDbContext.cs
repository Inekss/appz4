using Microsoft.EntityFrameworkCore;
using appz_4.DAL.entities;

namespace appz_4.DAL.context
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<DayMenu> DayMenus { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Конфігурація зв’язків MenuItem
            modelBuilder.Entity<MenuItem>()
                .HasKey(mi => mi.Id);

            modelBuilder.Entity<MenuItem>()
                .HasOne(mi => mi.Dish)
                .WithMany(d => d.MenuItems)
                .HasForeignKey(mi => mi.DishId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MenuItem>()
                .HasOne(mi => mi.DayMenu)
                .WithMany(dm => dm.MenuItems)
                .HasForeignKey(mi => mi.DayMenuId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}