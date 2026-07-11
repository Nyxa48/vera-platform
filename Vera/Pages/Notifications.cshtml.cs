using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Vera.Data;
using Vera.Models;

namespace Vera.Pages
{
    public class NotificationsModel : PageModel
    {
        private readonly LogDbContext _logContext;

        public NotificationsModel(LogDbContext logContext)
        {
            _logContext = logContext;
        }

        // View'a göndereceğimiz bildirimler listesi
        public List<ActivityLog> Logs { get; set; }
        public int TotalCount { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!User.Identity.IsAuthenticated) return RedirectToPage("/Login");

            var userId = int.Parse(User.FindFirstValue("UserId") ?? "0");

            // Sadece giriş yapan kullanıcının bildirimlerini çekiyoruz
            Logs = await _logContext.Logs
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();

            TotalCount = Logs.Count;

            return Page();
        }

        // Tek bildirim silme
        public async Task<IActionResult> OnPostDeleteAsync(int logId)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToPage("/Login");

            var userId = int.Parse(User.FindFirstValue("UserId") ?? "0");
            var log = await _logContext.Logs.FirstOrDefaultAsync(l => l.Id == logId && l.UserId == userId);

            if (log != null)
            {
                _logContext.Logs.Remove(log);
                await _logContext.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        // Tüm bildirimleri temizle
        public async Task<IActionResult> OnPostClearAllAsync()
        {
            if (!User.Identity.IsAuthenticated) return RedirectToPage("/Login");

            var userId = int.Parse(User.FindFirstValue("UserId") ?? "0");
            var logs = await _logContext.Logs.Where(l => l.UserId == userId).ToListAsync();

            if (logs.Any())
            {
                _logContext.Logs.RemoveRange(logs);
                await _logContext.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}