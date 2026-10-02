using Microsoft.EntityFrameworkCore;
using WebApiPrac4.Models;

namespace WebApiPrac4.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Team> Teams { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Team>(entity =>
        {
            entity.Property(team => team.Name).HasMaxLength(120).IsRequired();
            entity.Property(team => team.City).HasMaxLength(100).IsRequired();
            entity.Property(team => team.Description).HasMaxLength(1000);
            entity.HasData(
                new Team { Id = 1, Name = "Almaty Falcons", City = "Almaty", Description = "Команда из Алматы." },
                new Team { Id = 2, Name = "Astana Nomads", City = "Astana", Description = "Команда из Астаны." },
                new Team { Id = 3, Name = "Shymkent Tigers", City = "Shymkent", Description = "Команда из Шымкента." });
        });
    }
}
