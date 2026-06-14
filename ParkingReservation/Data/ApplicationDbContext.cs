using Microsoft.EntityFrameworkCore;
using ParkingReservation.Models;

namespace ParkingReservation.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<ParkingSpot> ParkingSpots { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
    }
}