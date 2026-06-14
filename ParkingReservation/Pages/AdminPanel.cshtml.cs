using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ParkingReservation.Data;
using ParkingReservation.Models;

namespace ParkingReservation.Pages
{
    [Authorize(Roles = "Admin")]
    public class AdminPanelModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AdminPanelModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<User> AllUsers { get; set; } = new List<User>();
        public List<ParkingSpot> AllSpots { get; set; } = new List<ParkingSpot>();
        public List<Reservation> AllReservations { get; set; } = new List<Reservation>();

        public async Task OnGetAsync()
        {
            AllUsers = await _context.Users.ToListAsync();
            AllSpots = await _context.ParkingSpots.OrderBy(s => s.SpotNumber).ToListAsync();
            
            AllReservations = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.ParkingSpot)
                .OrderByDescending(r => r.StartTime)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostCancelReservationAsync(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.ParkingSpot)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation != null && reservation.Status == "Active")
            {
                reservation.Status = "Cancelled";
                if (reservation.ParkingSpot != null)
                {
                    reservation.ParkingSpot.IsOccupied = false;
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddSpotAsync(string spotNumber)
        {
            if (!string.IsNullOrEmpty(spotNumber))
            {
                var newSpot = new ParkingSpot { SpotNumber = spotNumber, IsOccupied = false };
                _context.ParkingSpots.Add(newSpot);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}