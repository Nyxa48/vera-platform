using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vera.Data;
using Vera.Models;

namespace Vera.Pages
{
    public class FreelancersModel : PageModel
    {
        private readonly CoreDbContext _context;

        public FreelancersModel(CoreDbContext context)
        {
            _context = context;
        }

        public List<User> FreelancerList { get; set; } = new List<User>();

        // Her freelancer için: tamamlanan proje sayısı
        public Dictionary<int, int> CompletedProjectCounts { get; set; } = new Dictionary<int, int>();
        public Dictionary<int, int> ActiveProjectCounts { get; set; } = new Dictionary<int, int>();

        [BindProperty(SupportsGet = true)]
        public string Search { get; set; }

        public async Task OnGetAsync()
        {
            // Sadece Freelancer ve İşverenleri çek, Admin gizli kalsın
            IQueryable<User> query = _context.Users.Where(u => u.Role != "Admin").AsQueryable();

            // Arama filtresi
            if (!string.IsNullOrWhiteSpace(Search))
            {
                query = query.Where(u =>
                    u.FullName.Contains(Search) ||
                    (u.Title != null && u.Title.Contains(Search)) ||
                    (u.About != null && u.About.Contains(Search)) ||
                    (u.Skills != null && u.Skills.Contains(Search))
                );
            }

            FreelancerList = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();

            // Tek seferde tüm freelancerların proje istatistiklerini çek (N+1 fix)
            var freelancerIds = FreelancerList.Select(f => f.Id).ToList();

            var completedFreelancerCounts = await _context.Projects
                .Where(p => freelancerIds.Contains(p.FreelancerId ?? 0) && p.Status == "Tamamlandı")
                .GroupBy(p => p.FreelancerId)
                .Select(g => new { UserId = g.Key.Value, Count = g.Count() })
                .ToListAsync();

            var completedEmployerCounts = await _context.Projects
                .Where(p => freelancerIds.Contains(p.EmployerId) && p.Status == "Tamamlandı")
                .GroupBy(p => p.EmployerId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToListAsync();

            var activeFreelancerCounts = await _context.Projects
                .Where(p => freelancerIds.Contains(p.FreelancerId ?? 0) && p.Status == "Aktif")
                .GroupBy(p => p.FreelancerId)
                .Select(g => new { UserId = g.Key.Value, Count = g.Count() })
                .ToListAsync();

            var activeEmployerCounts = await _context.Projects
                .Where(p => freelancerIds.Contains(p.EmployerId) && (p.Status == "Aktif" || p.Status == "Açık"))
                .GroupBy(p => p.EmployerId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToListAsync();

            // Tüm istatistikleri Dictionary'e aktar
            foreach (var item in completedFreelancerCounts) CompletedProjectCounts[item.UserId] = item.Count;
            foreach (var item in completedEmployerCounts) CompletedProjectCounts[item.UserId] = item.Count;

            foreach (var item in activeFreelancerCounts) ActiveProjectCounts[item.UserId] = item.Count;
            foreach (var item in activeEmployerCounts) ActiveProjectCounts[item.UserId] = item.Count;
        }
    }
}
