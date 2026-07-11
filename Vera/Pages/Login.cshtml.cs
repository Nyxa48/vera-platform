using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Vera.Data;
using Vera.Models;

namespace Vera.Pages
{
    /// <summary>
    /// [GÜVENLİK & YETKİLENDİRME] 
    /// Platformun giriş (Authentication) mekanizmasını yöneten arka plan modeli.
    /// Şifre hash'leme kontrolü ve Session (Cookie) yönetimleri burada güvenli bir şekilde yapılır.
    /// </summary>
    public class LoginModel : PageModel
    {
        private readonly CoreDbContext _context;

        // Dependency Injection ile veritabanı bağlamımızı alıyoruz.
        public LoginModel(CoreDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public bool RememberMe { get; set; }

        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Sayfa ilk yüklendiğinde (GET request) çalışır.
        /// Güvenlik amacı ile "Hayalet Çerez Temizleyici" olarak çalışır.
        /// Giriş sayfasına gelindiğinde eski oturumları temizleyerek çakışmaları ve Session Fixation saldırılarını önleriz.
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Page();
        }

        /// <summary>
        /// Kullanıcı giriş formunu gönderdiğinde (POST request) çalışır.
        /// [CRUD - READ İşlemi] - Kullanıcıyı veritabanından güvenli bir şekilde sorgular.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            // Kullanıcıyı Email adresine göre veritabanından buluyoruz (Read işlemi)
            var user = _context.Users.FirstOrDefault(u => u.Email == Email);

            if (user != null)
            {
                // [GÜVENLİK KONTROLÜ] - Veritabanındaki şifreler açık metin (plain text) DEĞİLDİR.
                // ASP.NET Core Identity'nin PasswordHasher kütüphanesini kullanarak 
                // kullanıcının girdiği açık metin şifreyi, veritabanındaki hash ile karşılaştırıyoruz.
                var hasher = new PasswordHasher<User>();
                var result = hasher.VerifyHashedPassword(user, user.Password, Password);

                if (result == PasswordVerificationResult.Success)
                {
                    // Giriş başarılıysa kimlik kartını (Claims) oluşturuyoruz.
                    // Rol bazlı yetkilendirme (Role-based Authorization) için "Role" claim'ini eklemeyi unutmuyoruz.
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.FullName),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.Role), // İşveren veya Freelancer ayrımı için kritik!
                        new Claim("UserId", user.Id.ToString()) // İleriki CRUD işlemlerinde eşleştirme yapmak için ID
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    // Kullanıcıya kimlik kartını (Cookie) teslim ediyoruz.
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        new AuthenticationProperties
                        {
                            IsPersistent = RememberMe, // "Beni Hatırla" aktifse tarayıcı kapansa bile oturum kalır
                            ExpiresUtc = RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : null // 30 gün boyunca hatırla
                        }
                    );

                    // Giriş başarılıysa ana panele yönlendir
                    return RedirectToPage("/Dashboard");
                }
            }

            // [UX - KULLANICI DENEYİMİ] - Şifre mi yanlış yoksa e-posta mı yanlış söylemiyoruz.
            // Bu güvenlik standartlarına (OWASP) uyan "Username Enumeration" engelleme taktiğidir.
            ErrorMessage = "E-posta veya şifre hatalı!";
            return Page();
        }
    }
}