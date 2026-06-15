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

        // 1. ZWOLNIENIE REZERWACJI
        public async Task<IActionResult> OnPostCancelReservationAsync(int id)
        {
            var reservation = await _context.Reservations.Include(r => r.ParkingSpot).FirstOrDefaultAsync(r => r.Id == id);
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

        // 2. DODANIE MIEJSCA
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

        // 3. EDYCJA MIEJSCA (ZMIANA NUMERU)
        public async Task<IActionResult> OnPostEditSpotAsync(int spotId, string newSpotNumber)
        {
            var spot = await _context.ParkingSpots.FindAsync(spotId);
            if (spot != null && !string.IsNullOrEmpty(newSpotNumber))
            {
                spot.SpotNumber = newSpotNumber;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        // 4. USUWANIE MIEJSCA PARKINGOWEGO
        public async Task<IActionResult> OnPostDeleteSpotAsync(int spotId)
        {
            var spot = await _context.ParkingSpots.FindAsync(spotId);
            if (spot != null)
            {
                // Najpierw anulujemy aktywne rezerwacje na to miejsce, żeby baza się nie posypała (Klucze obce)
                var activeRes = await _context.Reservations.Where(r => r.ParkingSpotId == spotId).ToListAsync();
                foreach (var res in activeRes)
                {
                    res.Status = "Cancelled";
                }

                _context.ParkingSpots.Remove(spot);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        // 5. TWORZENIE UŻYTKOWNIKA PRZEZ ADMINA
        public async Task<IActionResult> OnPostCreateUserAsync(string firstName, string lastName, string email, string password, string role)
        {
            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
            {
                var exists = await _context.Users.AnyAsync(u => u.Email == email);
                if (!exists)
                {
                    var newUser = new User
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        Email = email,
                        PasswordHash = password,
                        Role = role
                    };
                    _context.Users.Add(newUser);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToPage();
        }

        // 6. ZMIANA UPRAWNIEŃ / ROLI UŻYTKOWNIKA
        public async Task<IActionResult> OnPostChangeRoleAsync(int userId, string newRole)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                // Zabezpieczenie: Admin nie może odebrać uprawnień samemu sobie
                var currentAdminId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (user.Id == currentAdminId) return RedirectToPage();

                user.Role = newRole;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        // 7. USUWANIE UŻYTKOWNIKA
        public async Task<IActionResult> OnPostDeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                var currentAdminId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (user.Id == currentAdminId) return RedirectToPage(); // Nie usuwamy samego siebie

                // Czyścimy powiązane rezerwacje użytkownika
                var userRes = await _context.Reservations.Where(r => r.UserId == userId).ToListAsync();
                _context.Reservations.RemoveRange(userRes);

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}