using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ParkingReservation.Data;
using ParkingReservation.Models;

namespace ParkingReservation.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<ParkingSpot> ParkingSpots { get; set; } = new List<ParkingSpot>();

        [BindProperty(SupportsGet = true)]
        public DateTime StartTime { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime EndTime { get; set; }

        public List<int> OccupiedSpotIds { get; set; } = new List<int>();

        public async Task OnGetAsync()
        {
             if (StartTime == DateTime.MinValue)
            {
                StartTime = DateTime.Now;
                EndTime = DateTime.Now.AddHours(2);
            }

            ParkingSpots = await _context.ParkingSpots.OrderBy(s => s.SpotNumber).ToListAsync();

            OccupiedSpotIds = await _context.Reservations
                .Where(r => r.Status == "Active" && 
                            r.StartTime < EndTime && 
                            r.EndTime > StartTime)
                .Select(r => r.ParkingSpotId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostReserveAsync(int spotId, DateTime startTime, DateTime endTime)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Login");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToPage("/Login");
            int userId = int.Parse(userIdClaim.Value);

            // Walidacja biznesowa dat
            if (startTime >= endTime || startTime < DateTime.Now.AddMinutes(-5))
            {
                TempData["ErrorMessage"] = "Wybrano niepoprawny przedział czasowy.";
                return RedirectToPage(new { startTime, endTime });
            }

            bool isAlreadyTaken = await _context.Reservations
                .AnyAsync(r => r.ParkingSpotId == spotId && 
                               r.Status == "Active" && 
                               r.StartTime < endTime && 
                               r.EndTime > startTime);

            if (isAlreadyTaken)
            {
                TempData["ErrorMessage"] = "To miejsce zostało właśnie zarezerwowane w tym terminie przez kogoś innego!";
                return RedirectToPage(new { startTime, endTime });
            }

            var reservation = new Reservation
            {
                UserId = userId,
                ParkingSpotId = spotId,
                StartTime = startTime,
                EndTime = endTime,
                Status = "Active"
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Miejsce zostało pomyślnie zarezerwowane!";
            return RedirectToPage(new { startTime, endTime });
        }
    }
}