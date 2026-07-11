using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Vera.Data;
using Vera.Models;

namespace Vera.Pages
{
    public class ProjectDetailModel : PageModel
    {
        private readonly CoreDbContext _context;
        private readonly LogDbContext _logContext;

        public ProjectDetailModel(CoreDbContext context, LogDbContext logContext)
        {
            _context = context;
            _logContext = logContext;
        }

        public Project CurrentProject { get; set; }
        public User Employer { get; set; }
        public bool HasActiveJob { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Veritabanından projeyi bul
            CurrentProject = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (CurrentProject == null) return RedirectToPage("/Projects");

            // İlanı veren işvereni bul
            Employer = await _context.Users.FirstOrDefaultAsync(u => u.Id == CurrentProject.EmployerId);

            // Giriş yapan Freelancer ise başka aktif işi var mı kontrol et
            if (User.Identity.IsAuthenticated && User.FindFirstValue(ClaimTypes.Role) == "Freelancer")
            {
                var userId = int.Parse(User.FindFirstValue("UserId") ?? "0");
                HasActiveJob = await _context.Projects.AnyAsync(p => p.FreelancerId == userId && p.Status == "Aktif");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAcceptJobAsync(int id)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToPage("/Login");

            var userId = int.Parse(User.FindFirstValue("UserId") ?? "0");

            // KONTROL: Zaten işin var mı? (Akıl sağlığı kuralı)
            var activeJobCount = await _context.Projects.CountAsync(p => p.FreelancerId == userId && p.Status == "Aktif");
            if (activeJobCount > 0) return RedirectToPage();

            var project = await _context.Projects.FindAsync(id);
            if (project != null && project.Status == "Açık")
            {
                project.FreelancerId = userId;
                project.Status = "Aktif";
                await _context.SaveChangesAsync();

                // Otomatik varsayılan aşamalar oluştur
                var defaultStages = new[] { "Analiz & Planlama", "Tasarım", "Geliştirme", "Test & QA", "Teslim" };
                for (int i = 0; i < defaultStages.Length; i++)
                {
                    _context.ProjectStages.Add(new ProjectStage
                    {
                        ProjectId = project.Id,
                        Title = defaultStages[i],
                        IsCompleted = false,
                        Order = i + 1
                    });
                }
                await _context.SaveChangesAsync();

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

            return RedirectToPage("/WorkArea", new { id = id });
        }
    }
}