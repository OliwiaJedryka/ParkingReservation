using System.Linq;
using SystemRef = System; // Alias dla System, aby uniknąć konfliktów z nazwą projektu
using ParkingReservation.Models;

namespace ParkingReservation.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            if (!context.Users.Any())
            {
                var users = new User[]
                {
                    new User { FirstName = "Jan", LastName = "Kowalski", Email = "admin@uczelnia.edu.pl", PasswordHash = "admin123", Role = "Admin" },
                    new User { FirstName = "Anna", LastName = "Nowak", Email = "student@uczelnia.edu.pl", PasswordHash = "student123", Role = "Student" },
                    new User { FirstName = "Piotr", LastName = "Zieliński", Email = "pracownik@uczelnia.edu.pl", PasswordHash = "pracownik123", Role = "Employee" }
                };

                context.Users.AddRange(users);
                context.SaveChanges();
            }

            if (!context.ParkingSpots.Any())
            {
                var spots = new ParkingSpot[]
                {
                    new ParkingSpot { SpotNumber = "A-01", IsOccupied = false },
                    new ParkingSpot { SpotNumber = "A-02", IsOccupied = false },
                    new ParkingSpot { SpotNumber = "A-03", IsOccupied = false },
                    new ParkingSpot { SpotNumber = "B-01", IsOccupied = false },
                    new ParkingSpot { SpotNumber = "B-02", IsOccupied = false }
                };

                context.ParkingSpots.AddRange(spots);
                context.SaveChanges();
            }
        }
    }
}