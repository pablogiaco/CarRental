using Microsoft.EntityFrameworkCore;
using DTOs.Models;

namespace DataAccess
{
    public class CarRentalDbContext : DbContext
    {
        public CarRentalDbContext(DbContextOptions<CarRentalDbContext> options) : base(options) 
        {
           
        }

        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Rental> Rentals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed data
            modelBuilder.Entity<Client>().HasData(
                new Client { Id = 1, FirstName = "Alvaro", LastName = "Etcheverry" },
                new Client { Id = 2, FirstName = "Luisina", LastName = "Gonzalez" }
            );

            modelBuilder.Entity<Vehicle>().HasData(
                new Vehicle { Id = 1, Model = "Model S", Brand = "Tesla", DailyPrice = 100},
                new Vehicle { Id = 2, Model = "Mustang", Brand = "Ford", DailyPrice = 80}
            );
        }
    }
}
