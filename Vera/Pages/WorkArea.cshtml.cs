using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Vera.Data;
using Vera.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vera.Pages
{
    public class WorkAreaModel : PageModel
    {
        private readonly CoreDbContext _context;
        private readonly LogDbContext _logContext;

        public WorkAreaModel(CoreDbContext context, LogDbContext logContext)
        {
            _context = context;
            _logContext = logContext;
        }

        public Project CurrentProject { get; set; }
        public List<ProjectStage> Stages { get; set; }
        public List<WorkLog> Logs { get; set; }
        public double TotalWorkedHours { get; set; }
        public int CompletedStages { get; set; }
        public int TotalStages { get; set; }
        public int ProgressPercent { get; set; }

        [BindProperty]
        public double NewLogHours { get; set; }
        [BindProperty]
        public string NewLogDescription { get; set; }
        [BindProperty]
        public string NewStageTitle { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToPage("/Login");
            if (User.FindFirstValue(ClaimTypes.Role) != "Freelancer") return RedirectToPage("/Dashboard");

            if (id.HasValue && id.Value > 0)
                CurrentProject = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id.Value);
            else
                CurrentProject = await _context.Projects.FirstOrDefaultAsync(p => p.FreelancerId == int.Parse(User.FindFirstValue("UserId")) && p.Status == "Aktif");

            if (CurrentProject == null) return RedirectToPage("/Dashboard");

            Stages = await _context.ProjectStages.Where(s => s.ProjectId == CurrentProject.Id).OrderBy(s => s.Order).ToListAsync();
            Logs = await _context.WorkLogs.Where(l => l.ProjectId == CurrentProject.Id).OrderByDescending(l => l.Date).ToListAsync();
            TotalWorkedHours = Logs.Sum(l => l.Hours);

            // İlerleme hesaplama
            TotalStages = Stages.Count;
            CompletedStages = Stages.Count(s => s.IsCompleted);
            ProgressPercent = TotalStages > 0 ? (int)Math.Round((double)CompletedStages / TotalStages * 100) : 0;

            return Page();
        }

        // 1. MESAİ EKLEME
        public async Task<IActionResult> OnPostAddLogAsync(int id)
        {
            if (NewLogHours <= 0 || string.IsNullOrEmpty(NewLogDescription))
                return RedirectToPage(new { id = id });

            var log = new WorkLog
            {
                ProjectId = id,
                UserId = int.Parse(User.FindFirstValue("UserId") ?? "0"),
                Date = DateTime.Now,
                Hours = NewLogHours,
                Description = NewLogDescription
            };

            _context.WorkLogs.Add(log);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = id });
        }

        // 2. AŞAMA İŞARETLEME (TOGGLE)
        public async Task<IActionResult> OnPostToggleStageAsync(int id, int stageId)
        {
            var stage = await _context.ProjectStages.FirstOrDefaultAsync(s => s.Id == stageId && s.ProjectId == id);
            if (stage != null)
            {
                stage.IsCompleted = !stage.IsCompleted;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage(new { id = id });
        }

        // 3. YENİ AŞAMA EKLEME
        public async Task<IActionResult> OnPostAddStageAsync(int id)
        {
            if (string.IsNullOrWhiteSpace(NewStageTitle))
                return RedirectToPage(new { id = id });

            var maxOrder = await _context.ProjectStages
                .Where(s => s.ProjectId == id)
                .MaxAsync(s => (int?)s.Order) ?? 0;

            var stage = new ProjectStage
            {
                ProjectId = id,
                Title = NewStageTitle.Trim(),
                IsCompleted = false,
                Order = maxOrder + 1
            };

            _context.ProjectStages.Add(stage);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = id });
        }

        // 4. AŞAMA SİLME
        public async Task<IActionResult> OnPostDeleteStageAsync(int id, int stageId)
        {
            var stage = await _context.ProjectStages.FirstOrDefaultAsync(s => s.Id == stageId && s.ProjectId == id);
            if (stage != null)
            {
                _context.ProjectStages.Remove(stage);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage(new { id = id });
        }

        // 5. İŞİ TESLİM ETME VE ÖDEME
        public async Task<IActionResult> OnPostCompleteJobAsync(int id)
        {
            if (User.FindFirstValue(ClaimTypes.Role) != "Freelancer") return RedirectToPage("/Dashboard");

            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (project == null || project.Status == "Tamamlandı") return RedirectToPage(new { id = id });

            var freelancer = await _context.Users.FindAsync(project.FreelancerId);
            var employer = await _context.Users.FindAsync(project.EmployerId);

            // Bütçeyi Hesapla (Saatlik mi Sabit mi?)
            decimal totalAmount = project.IsHourly ? project.Budget * (decimal)_context.WorkLogs.Where(l => l.ProjectId == id).Sum(l => l.Hours) : project.Budget;

            // 1. Projeyi Tamamla
            project.Status = "Tamamlandı";

            // 2. Parayı Freelancer'ın Cüzdanına Ekle
            if (freelancer != null) freelancer.WalletBalance += totalAmount;

            await _context.SaveChangesAsync();

            // 3. Bildirimleri (Log) Oluştur — Details alanı da doldurularak
            if (freelancer != null)
            {
                _logContext.Logs.Add(new ActivityLog
                {
                    UserId = freelancer.Id,
                    Action = "Hesaba Geçti",
                    Details = $"{project.Title} projesi başarıyla tamamlandı. ₺{totalAmount:N0} kazancınız bakiyenize eklendi.",
                    Timestamp = DateTime.Now
                });
            }
            if (employer != null)
            {
                _logContext.Logs.Add(new ActivityLog
                {
                    UserId = employer.Id,
                    Action = "Ödeme Yapıldı",
                    Details = $"{project.Title} projesi için freelancer'a ₺{totalAmount:N0} ödeme transfer edildi.",
                    Timestamp = DateTime.Now
                });
            }
            await _logContext.SaveChangesAsync();

            return RedirectToPage(new { id = id });
        }
    }
}