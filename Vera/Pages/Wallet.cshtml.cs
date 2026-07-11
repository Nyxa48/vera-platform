using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Vera.Data;
using Vera.Models;

namespace Vera.Pages
{
    public class WalletModel : PageModel
    {
        private readonly CoreDbContext _coreContext;
        private readonly LogDbContext _logContext;

        public WalletModel(CoreDbContext coreContext, LogDbContext logContext)
        {
            _coreContext = coreContext;
            _logContext = logContext;
        }

        public User CurrentUser { get; set; }
        public List<ActivityLog> FinancialLogs { get; set; }

        // --- ORTAK / İŞVEREN METRİKLERİ ---
        public decimal TotalEarnedOrSpent { get; set; }
        public decimal EscrowBalance { get; set; } // İşverenin içerideki (güvenli havuz) bekleyen parası
        public int ActiveEscrowProjects { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!User.Identity.IsAuthenticated) return RedirectToPage("/Login");

            var email = User.FindFirstValue(ClaimTypes.Email);
            CurrentUser = await _coreContext.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (CurrentUser == null) return RedirectToPage("/Logout");

            // Geçmiş işlemlere sadece finansal hareket içeren Log'ları çekiyoruz
            FinancialLogs = await _logContext.Logs
                .Where(l => l.UserId == CurrentUser.Id && (l.Action.Contains("Ödeme") || l.Action.Contains("Hesaba Geçti") || l.Action.Contains("Teklif") || l.Action.Contains("Fatura") || l.Action.Contains("Bakiye")))
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();

            if (CurrentUser.Role == "Employer")
            {
                // İşveren için TotalSpent: Tamamlanan projelerin bütçeleri
                TotalEarnedOrSpent = await _coreContext.Projects
                    .Where(p => p.EmployerId == CurrentUser.Id && p.Status == "Tamamlandı")
                    .SumAsync(p => p.Budget);

                // Escrow: Aktif/Açık projelerin bütçeleri (güvenli havuzda bekleyen)
                EscrowBalance = await _coreContext.Projects
                    .Where(p => p.EmployerId == CurrentUser.Id && (p.Status == "Aktif" || p.Status == "Açık"))
                    .SumAsync(p => p.Budget);

                ActiveEscrowProjects = await _coreContext.Projects
                    .CountAsync(p => p.EmployerId == CurrentUser.Id && (p.Status == "Aktif" || p.Status == "Açık"));
            }
            else
            {
                // Freelancer için TotalEarned: Tamamlanan projelerin bütçeleri
                TotalEarnedOrSpent = await _coreContext.Projects
                    .Where(p => p.FreelancerId == CurrentUser.Id && p.Status == "Tamamlandı")
                    .SumAsync(p => p.Budget);

                // Escrow: Aktif projelerin bütçeleri
                EscrowBalance = await _coreContext.Projects
                    .Where(p => p.FreelancerId == CurrentUser.Id && p.Status == "Aktif")
                    .SumAsync(p => p.Budget);

                ActiveEscrowProjects = await _coreContext.Projects
                    .CountAsync(p => p.FreelancerId == CurrentUser.Id && p.Status == "Aktif");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAddBalanceAsync(decimal amount)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToPage("/Login");

            if (amount <= 0 || amount > 1000000)
            {
                // Geçersiz miktar
                return RedirectToPage("/Wallet");
            }

            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _coreContext.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null) return RedirectToPage("/Logout");

            // Bakiyeyi ekle
            user.WalletBalance += amount;
            await _coreContext.SaveChangesAsync();

            // Log kaydı oluştur
            var newLog = new ActivityLog
            {
                UserId = user.Id,
                Action = "Bakiye Yüklendi",
                Details = $"Cüzdanınıza ₺{amount.ToString("N0")} bakiye başarıyla yüklendi.",
                Timestamp = DateTime.Now
            };

            _logContext.Logs.Add(newLog);
            await _logContext.SaveChangesAsync();

            return RedirectToPage("/Wallet");
        }
    }
}