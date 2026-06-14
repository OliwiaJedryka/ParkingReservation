using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ParkingReservation.Data;

namespace ParkingReservation.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public LoginModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Proszę uzupełnić wszystkie pola.";
                return Page();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == Email);


            if (user == null || user.PasswordHash != Password)
            {
                ErrorMessage = "Nieprawidłowy e-mail lub hasło.";
                return Page();
            }

             var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

          
            return RedirectToPage("/Index");
        }

        public async Task<IActionResult> OnPostLogoutAsync()
            {
    
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    
                return RedirectToPage("/Index");
            }

    }
}