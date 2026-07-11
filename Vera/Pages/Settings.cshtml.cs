using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Vera.Data;
using Vera.Models;

namespace Vera.Pages
{
    public class SettingsModel : PageModel
    {
        private readonly CoreDbContext _context;

        public SettingsModel(CoreDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string FullName { get; set; }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string About { get; set; }

        [BindProperty]
        public string Title { get; set; }

        [BindProperty]
        public string Skills { get; set; }

        [BindProperty]
        public string IBAN { get; set; }

        // ŞİFRE DEĞİŞTİRME ALANLARI
        [BindProperty]
        public string CurrentPassword { get; set; }

        [BindProperty]
        public string NewPassword { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // 1. MANUEL GÜVENLİK (Dashboard'daki gibi kurşun geçirmez)
            if (!User.Identity.IsAuthenticated) return RedirectToPage("/Login");

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null) return RedirectToPage("/Logout");

            // Kullanıcı bilgilerini kutulara doldur
            FullName = user.FullName;
            Email = user.Email;
            About = user.About;
            Title = user.Title;
            Skills = user.Skills;
            IBAN = user.IBAN;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Form gönderildiğinde de güvenliği kontrol edelim
            if (!User.Identity.IsAuthenticated) return RedirectToPage("/Login");

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user != null)
            {
                // 1. Şifre Değiştirme İsteği Var Mı?
                if (!string.IsNullOrEmpty(CurrentPassword) || !string.IsNullOrEmpty(NewPassword))
                {
                    if (string.IsNullOrEmpty(CurrentPassword) || string.IsNullOrEmpty(NewPassword) || string.IsNullOrEmpty(ConfirmPassword))
                    {
                        ErrorMessage = "Şifre değiştirmek için tüm şifre alanlarını doldurmalısınız.";
                        return Page();
                    }

                    if (NewPassword != ConfirmPassword)
                    {
                        ErrorMessage = "Yeni şifreler eşleşmiyor.";
                        return Page();
                    }

                    var hasher = new PasswordHasher<User>();
                    var result = hasher.VerifyHashedPassword(user, user.Password, CurrentPassword);
                    
                    if (result == PasswordVerificationResult.Failed)
                    {
                        ErrorMessage = "Mevcut şifreniz yanlış.";
                        return Page();
                    }

                    // Şifreyi güncelle
                    user.Password = hasher.HashPassword(user, NewPassword);
                }

                // 2. Profil Bilgilerini Güncelle
                user.FullName = FullName;
                user.About = About;
                user.Title = Title;
                user.Skills = Skills;
                user.IBAN = IBAN;

                await _context.SaveChangesAsync();

                // Başarı mesajını ve güncel verileri sayfaya yolla
                SuccessMessage = "Profil bilgileriniz başarıyla güncellendi! 😎";
                FullName = user.FullName;
                Email = user.Email;
                About = user.About;
                Title = user.Title;
                Skills = user.Skills;
                IBAN = user.IBAN;
            }
            else
            {
                ErrorMessage = "Bir hata oluştu, lütfen tekrar deneyin.";
            }

            return Page();
        }
    }
}