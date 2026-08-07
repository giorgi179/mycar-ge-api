using Microsoft.EntityFrameworkCore;
using CarProject.Models;

namespace CarProject.Data;

public class Base : DbContext
{
    public Base(DbContextOptions<Base> options) : base(options) { }

    public DbSet<Car> Cars { get; set; }
    public DbSet<CarDetals> CarDetals { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<CarImage> CarImages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<CarDetals>()
            .HasOne(d => d.Car)
            .WithOne(c => c.CarDetals)
            .HasForeignKey<CarDetals>(d => d.CarId);



        modelBuilder.Entity<CarImage>()
            .HasOne(i => i.Car)
            .WithMany(c => c.Images)
            .HasForeignKey(i => i.CarId);

    }
}