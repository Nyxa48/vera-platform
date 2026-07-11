using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Vera.Data;
using Vera.Models;

namespace Vera.Pages
{
    public class FreelancerDetailModel : PageModel
    {
        private readonly CoreDbContext _context;
        private readonly LogDbContext _logContext;

        public FreelancerDetailModel(CoreDbContext context, LogDbContext logContext)
        {
            _context = context;
            _logContext = logContext;
        }

        public User ProfileUser { get; set; }
        public bool IsOwnProfile { get; set; }

        // İstatistikler
        public int CompletedProjectsCount { get; set; }
        public int ActiveProjectsCount { get; set; }
        public decimal TotalEarnings { get; set; }
        public List<Project> CompletedProjects { get; set; } = new List<Project>();
        public List<Project> EmployerOpenProjects { get; set; } = new List<Project>();

        // Mesaj Gönderme
        [BindProperty]
        public string MessageContent { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToPage("/Login");

            var loggedInEmail = User.FindFirstValue(ClaimTypes.Email);

            if (id.HasValue)
            {
                ProfileUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id.Value);
            }
            else
            {
                ProfileUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == loggedInEmail);
            }

            if (ProfileUser == null) return RedirectToPage("/Dashboard");

            IsOwnProfile = (ProfileUser.Email == loggedInEmail);

            // İstatistikleri hesapla
            if (ProfileUser.Role == "Employer")
            {
                CompletedProjectsCount = await _context.Projects.CountAsync(p => p.EmployerId == ProfileUser.Id && p.Status == "Tamamlandı");
                ActiveProjectsCount = await _context.Projects.CountAsync(p => p.EmployerId == ProfileUser.Id && (p.Status == "Aktif" || p.Status == "Açık"));
                TotalEarnings = await _context.Projects.Where(p => p.EmployerId == ProfileUser.Id && p.Status == "Tamamlandı").SumAsync(p => p.Budget);
                
                EmployerOpenProjects = await _context.Projects
                    .Where(p => p.EmployerId == ProfileUser.Id && p.Status == "Açık")
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(4)
                    .ToListAsync();
            }
            else
            {
                CompletedProjectsCount = await _context.Projects.CountAsync(p => p.FreelancerId == ProfileUser.Id && p.Status == "Tamamlandı");
                ActiveProjectsCount = await _context.Projects.CountAsync(p => p.FreelancerId == ProfileUser.Id && p.Status == "Aktif");
                TotalEarnings = await _context.Projects.Where(p => p.FreelancerId == ProfileUser.Id && p.Status == "Tamamlandı").SumAsync(p => p.Budget);
                CompletedProjects = await _context.Projects
                    .Where(p => p.FreelancerId == ProfileUser.Id && p.Status == "Tamamlandı")
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(4)
                    .ToListAsync();
            }

            return Page();
        }

        // MESAJ GÖNDERME
        public async Task<IActionResult> OnPostSendMessageAsync(int id)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToPage("/Login");
            if (string.IsNullOrWhiteSpace(MessageContent)) return RedirectToPage(new { id });

            var senderId = int.Parse(User.FindFirstValue("UserId") ?? "0");

            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = id,
                Content = MessageContent.Trim(),
                SentAt = DateTime.Now,
                IsRead = false
            };

            _logContext.Messages.Add(message);
            await _logContext.SaveChangesAsync();

            return RedirectToPage(new { id });
        }
    }
}