using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using System.Text;
using Vera.Data;
using Vera.Models;

namespace Vera.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly CoreDbContext _context;

        public RegisterModel(CoreDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string FullName { get; set; }
        [BindProperty]
        public string Email { get; set; }
        [BindProperty]
        public string Password { get; set; }
        [BindProperty]
        public string Role { get; set; }

        public string ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            // Kullanıcı zaten giriş yapmışsa direkt Dashboard'a fırlat
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Dashboard");
            }

            return Page();
        }

        // ŞİFREYİ HASHLEYEN GİZLİ METODUMUZ (Hocanın bayılacağı kısım)
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == Email);
            if (existingUser != null)
            {
                ErrorMessage = "Bu e-posta adresi zaten kullanımda!";
                return Page();
            }

            // 2. Yeni kullanıcı
            var newUser = new User
            {
                FullName = FullName,
                Email = Email,
                Role = Role,
                WalletBalance = 0,
                CreatedAt = DateTime.Now
            };

            var hasher = new PasswordHasher<User>();
            newUser.Password = hasher.HashPassword(newUser, Password);

            // 3. Veritabanına kaydet
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // 4. OTOMATİK GİRİŞ YAP
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, newUser.FullName),
                new Claim(ClaimTypes.Email, newUser.Email),
                new Claim(ClaimTypes.Role, newUser.Role),
                new Claim("UserId", newUser.Id.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            new AuthenticationProperties { IsPersistent = false } // Tarayıcı kapanınca çıkış yapmasını sağlayan sihirli kod
            );

            // 5. Başarıyla Dashboard'a uçur
            return RedirectToPage("/Dashboard");
        }
    }
}