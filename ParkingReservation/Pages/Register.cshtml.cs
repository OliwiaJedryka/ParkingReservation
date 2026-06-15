using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ParkingReservation.Data;
using ParkingReservation.Models;

namespace ParkingReservation.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public RegisterModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string FirstName { get; set; } = string.Empty;

        [BindProperty]
        public string LastName { get; set; } = string.Empty;

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public string ConfirmPassword { get; set; } = string.Empty;

        [BindProperty]
        public string Role { get; set; } = "Student"; // Domyślnie rejestrujemy jako student

        public string ErrorMessage { get; set; } = string.Empty;

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // 1. Sprawdzamy czy hasła są identyczne
            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Podane hasła nie są identyczne.";
                return Page();
            }

            // 2. Sprawdzamy czy użytkownik o takim mailu już istnieje
            var userExists = await _context.Users.AnyAsync(u => u.Email == Email);
            if (userExists)
            {
                ErrorMessage = "Ten adres e-mail jest już zarejestrowany w systemie.";
                return Page();
            }

            // 3. Tworzymy nowy obiekt użytkownika
            var newUser = new User
            {
                FirstName = FirstName,
                LastName = LastName,
                Email = Email,
                PasswordHash = Password, // Na potrzeby projektu szkolnego zapisujemy tekst (haszowanie można dodać później)
                Role = Role
            };

            // 4. Zapisujemy w bazie SQLite
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Po udanej rejestracji przekierowujemy na stronę logowania z komunikatem
            TempData["SuccessMessage"] = "Konto zostało założone pomyślnie! Możesz się teraz zalogować.";
            return RedirectToPage("/Login");
        }
    }
}