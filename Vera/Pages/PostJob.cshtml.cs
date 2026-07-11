using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Vera.Data;
using Vera.Models;

namespace Vera.Pages
{
    /// <summary>
    /// [CRUD İŞLEMLERİ - CREATE] 
    /// İşverenlerin (Employer) sisteme yeni bir iş ilanı (Project) eklemesini sağlayan arka plan kodu.
    /// Aynı zamanda bakiye kontrolü, veri doğrulama ve Activity Log (Aktivite Günlüğü) gibi ekstra iş kurallarını içerir.
    /// </summary>
    public class PostJobModel : PageModel
    {
        private readonly CoreDbContext _context;
        private readonly LogDbContext _logContext;

        // [MİMARİ] - Dependency Injection (Bağımlılık Enjeksiyonu)
        // Hem ana veritabanımızı hem de log veritabanımızı ayrı ayrı içeri alıyoruz (Single Responsibility).
        public PostJobModel(CoreDbContext context, LogDbContext logContext)
        {
            _context = context;
            _logContext = logContext;
        }

        // Ön yüzden (View) gelen form verilerini otomatik olarak C# modeline eşler (Model Binding)
        [BindProperty]
        public Project NewProject { get; set; }

        public User? CurrentUser { get; set; }
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Sayfa ilk açıldığında çalışır (GET)
        /// İşveren yetkisi olmayanları (Örn: Freelancer veya Ziyaretçi) engeller.
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            // [GÜVENLİK & YETKİLENDİRME] - Backend tabanlı rol kontrolü. 
            // Sadece sisteme giriş yapmış "Employer" yetkisine sahip kullanıcılar ilan yayınlayabilir.
            if (!User.Identity.IsAuthenticated || User.FindFirstValue(ClaimTypes.Role) != "Employer")
                return RedirectToPage("/Dashboard");

            // Ön yüzde bakiye banner'ını (Arayüz UI/UX) gösterebilmek için mevcut kullanıcıyı çekiyoruz.
            var email = User.FindFirstValue(ClaimTypes.Email);
            CurrentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            return Page();
        }

        /// <summary>
        /// Form post edildiğinde (submit) çalışır. İlanı veritabanına kaydeder (Create).
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            // [GÜVENLİK] - Çift Kontrol (Double Check). Form post edildiğinde de yetki doğrulanır.
            // Postman veya dışarıdan atılabilecek sahte isteklere (CORS/CSRF) karşı koruma.
            if (!User.Identity.IsAuthenticated || User.FindFirstValue(ClaimTypes.Role) != "Employer")
                return RedirectToPage("/Login");

            var employerId = int.Parse(User.FindFirstValue("UserId") ?? "0");
            var employer = await _context.Users.FindAsync(employerId);

            if (employer == null) return RedirectToPage("/Login");

            // [EKSTRA ÖZELLİK & İŞ MANTIĞI] - Finansal (Cüzdan) Kontrolü
            // Şirketin bakiyesi negatifte ise ilan açması engellenir.
            if (employer.WalletBalance < 0)
            {
                CurrentUser = employer; // View tarafında bakiye uyarısını tekrar çizmek için
                ErrorMessage = "Hesabınızın bakiyesi negatif olduğu için yeni ilan yayınlayamazsınız. Lütfen önce cüzdanınıza bakiye yükleyin.";
                return Page();
            }

            // Gelen modelin eksik kısımlarını (Arka plan verileri) biz dolduruyoruz.
            NewProject.EmployerId = employerId;
            NewProject.Status = "Açık"; // İlan boşta
            NewProject.CreatedAt = DateTime.Now;

            // [CRUD - CREATE İşlemi] - İlanı veritabanına ekle
            _context.Projects.Add(NewProject);

            // [EKSTRA ÖZELLİK] - Bütçe Rezervasyonu (Escrow Mantığı)
            // İlan yayınlandığı anda bütçe tutarı şirketin mevcut cüzdanından düşülür.
            employer.WalletBalance -= NewProject.Budget;

            // Yapılan iki değişikliği (İlan ekleme + Bakiye düşme) tek bir Transaction (İşlem) halinde kaydet.
            await _context.SaveChangesAsync();

            // [MİMARİ] - Aktivite Loglama (Ayrı Veritabanına yazılır)
            _logContext.Logs.Add(new ActivityLog
            {
                UserId = employerId,
                Action = "Yeni İlan Yayınlandı",
                Details = $"'{NewProject.Title}' ilanı yayınlandı. Bütçe olarak ₺{NewProject.Budget.ToString("N0")} cüzdanınızdan rezerve edildi.",
                Timestamp = DateTime.Now
            });
            await _logContext.SaveChangesAsync();

            // İşlem başarılı, Dashboard'a geri dön
            return RedirectToPage("/Dashboard");
        }
    }
}