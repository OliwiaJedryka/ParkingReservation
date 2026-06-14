using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ParkingReservation.Data;
using ParkingReservation.Models;

namespace ParkingReservation.Pages
{
    [Authorize]
    public class ReservationsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ReservationsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Reservation> UserReservations { get; set; } = new List<Reservation>();

        public async Task OnGetAsync()
        {

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            UserReservations = await _context.Reservations
                .Include(r => r.ParkingSpot)
                .Where(r => r.UserId == userId && r.Status == "Active")
                .OrderByDescending(r => r.StartTime)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostCancelAsync(int reservationId)
        {

            var reservation = await _context.Reservations
                .Include(r => r.ParkingSpot)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation != null)
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
    }
}