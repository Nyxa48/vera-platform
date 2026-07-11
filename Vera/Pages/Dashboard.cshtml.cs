using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Vera.Data;
using Vera.Models;

namespace Vera.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly CoreDbContext _coreContext;
        private readonly LogDbContext _logContext;

        public DashboardModel(CoreDbContext coreContext, LogDbContext logContext)
        {
            _coreContext = coreContext;
            _logContext = logContext;
        }

        public User CurrentUser { get; set; }

        // --- FREELANCER İÇİN DEĞİŞKENLER ---
        public List<Project> ActiveProjects { get; set; }
        public int TotalActiveJobs { get; set; }
        public int TotalUnreadMsgs { get; set; }
        public Project MyActiveProject { get; set; } // Freelancer'ın üzerinde çalıştığı aktif proje
        public int MyActiveProjectProgress { get; set; } // İlerleme yüzdesi

        // --- İŞVEREN (EMPLOYER) İÇİN DEĞİŞKENLER ---
        public List<Project> MyPostedProjects { get; set; }
        public int TotalApplicants { get; set; }
        public decimal TotalSpent { get; set; } // Gerçek toplam harcama
        public Dictionary<int, int> ProjectProgress { get; set; } = new Dictionary<int, int>(); // İşveren için projelerin yüzde ilerlemesi

        // --- ORTAK DEĞİŞKENLER ---
        public List<Message> RecentMessages { get; set; }
        public Dictionary<int, string> SenderNames { get; set; } = new Dictionary<int, string>();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!User.Identity.IsAuthenticated) return RedirectToPage("/Login");

            var email = User.FindFirstValue(ClaimTypes.Email);
            CurrentUser = await _coreContext.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (CurrentUser == null) return RedirectToPage("/Logout");

            // Yönetici dashboard'a girmek yerine her zaman yönetim paneline yönlendirilir
            if (CurrentUser.Role == "Admin") return RedirectToPage("/Admin");

            // ORTAK VERİ: Mesajlar ve Okunmamış Mesaj Sayısı
            RecentMessages = await _logContext.Messages
                .Where(m => m.ReceiverId == CurrentUser.Id)
                .OrderByDescending(m => m.SentAt).Take(3).ToListAsync();

            // Okunmamış mesaj sayısını hesaplıyoruz
            TotalUnreadMsgs = await _logContext.Messages
                .CountAsync(m => m.ReceiverId == CurrentUser.Id && !m.IsRead);

            var senderIds = RecentMessages.Select(m => m.SenderId).Distinct().ToList();
            var senders = await _coreContext.Users.Where(u => senderIds.Contains(u.Id)).ToListAsync();
            foreach (var sender in senders) { SenderNames[sender.Id] = sender.FullName; }

            // ROLE GÖRE ÖZEL VERİ ÇEKİMİ
            if (CurrentUser.Role == "Employer")
            {
                // İşverenin kendi paylaştığı ilanlar
                MyPostedProjects = await _coreContext.Projects
                    .Where(p => p.EmployerId == CurrentUser.Id)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                TotalApplicants = 12; // Şimdilik sembolik başvuru sayısı

                // Gerçek toplam harcama: Tamamlanan projelerin bütçe toplamı
                TotalSpent = await _coreContext.Projects
                    .Where(p => p.EmployerId == CurrentUser.Id && p.Status == "Tamamlandı")
                    .SumAsync(p => p.Budget);

                // İşverenin aktif projelerinin ilerleme yüzdelerini hesaplıyoruz
                foreach (var project in MyPostedProjects)
                {
                    if (project.Status == "Aktif")
                    {
                        var totalStages = await _coreContext.ProjectStages.CountAsync(s => s.ProjectId == project.Id);
                        var completedStages = await _coreContext.ProjectStages.CountAsync(s => s.ProjectId == project.Id && s.IsCompleted);
                        ProjectProgress[project.Id] = totalStages > 0 ? (int)Math.Round((double)completedStages / totalStages * 100) : 0;
                    }
                }
            }
            else // Freelancer
            {
                // Freelancer için genel aktif işler
                ActiveProjects = await _coreContext.Projects
                    .Where(p => p.Status == "Açık")
                    .OrderByDescending(p => p.CreatedAt).Take(2).ToListAsync();

                // Toplam aktif iş sayısını hesaplıyoruz
                TotalActiveJobs = await _coreContext.Projects
                    .CountAsync(p => p.FreelancerId == CurrentUser.Id && p.Status == "Aktif");

                // Freelancer'ın üzerinde çalıştığı aktif proje
                MyActiveProject = await _coreContext.Projects
                    .FirstOrDefaultAsync(p => p.FreelancerId == CurrentUser.Id && p.Status == "Aktif");

                if (MyActiveProject != null)
                {
                    var totalStages = await _coreContext.ProjectStages.CountAsync(s => s.ProjectId == MyActiveProject.Id);
                    var completedStages = await _coreContext.ProjectStages.CountAsync(s => s.ProjectId == MyActiveProject.Id && s.IsCompleted);
                    MyActiveProjectProgress = totalStages > 0 ? (int)Math.Round((double)completedStages / totalStages * 100) : 0;
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAcceptJobAsync(int projectId)
        {
            var userId = int.Parse(User.FindFirstValue("UserId") ?? "0");

            // Zaten aktif işi var mı?
            var hasActiveJob = await _coreContext.Projects.AnyAsync(p => p.FreelancerId == userId && p.Status == "Aktif");

            if (hasActiveJob) return RedirectToPage(); // Varsa yeni iş alamaz.

            var project = await _coreContext.Projects.FindAsync(projectId);
            if (project != null && project.Status == "Açık")
            {
                project.FreelancerId = userId;
                project.Status = "Aktif";
                await _coreContext.SaveChangesAsync();

                // Otomatik varsayılan aşamalar oluştur
                var defaultStages = new[] { "Analiz & Planlama", "Tasarım", "Geliştirme", "Test & QA", "Teslim" };
                for (int i = 0; i < defaultStages.Length; i++)
                {
                    _coreContext.ProjectStages.Add(new ProjectStage
                    {
                        ProjectId = project.Id,
                        Title = defaultStages[i],
                        IsCompleted = false,
                        Order = i + 1
                    });
                }
                await _coreContext.SaveChangesAsync();

                // Bildirim oluştur
                _logContext.Logs.Add(new ActivityLog
                {
                    UserId = userId,
                    Action = "Yeni İş Kabul Edildi",
                    Details = $"{project.Title} projesi kabul edildi. Çalışma alanınız hazır!",
                    Timestamp = DateTime.Now
                });
                await _logContext.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}