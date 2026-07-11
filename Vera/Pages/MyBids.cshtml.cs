using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Vera.Data;
using Vera.Models;

namespace Vera.Pages
{
    public class MyBidsModel : PageModel
    {
        private readonly CoreDbContext _context;

        public MyBidsModel(CoreDbContext context)
        {
            _context = context;
        }

        public List<Project> ActiveProjects { get; set; } = new List<Project>();
        public List<Project> CompletedProjects { get; set; } = new List<Project>();
        public Dictionary<int, string> UserNames { get; set; } = new Dictionary<int, string>(); // Employer için freelancer, Freelancer için employer isimleri
        public int TotalEarningsOrSpent { get; set; }
        public string UserRole { get; set; }

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; } = "All"; // All, Open, Active, Completed, Cancelled


        public async Task<IActionResult> OnGetAsync()
        {
            if (!User.Identity.IsAuthenticated) return RedirectToPage("/Login");

            var userId = int.Parse(User.FindFirstValue("UserId") ?? "0");

            UserRole = User.FindFirstValue(ClaimTypes.Role);

            if (UserRole == "Employer")
            {
                // İşverenin tüm projelerini çek (iptaller dahil)
                var query = _context.Projects.Where(p => p.EmployerId == userId).AsQueryable();

                // Filtreleme
                if (StatusFilter == "Open") query = query.Where(p => p.Status == "Açık");
                else if (StatusFilter == "Active") query = query.Where(p => p.Status == "Aktif");
                else if (StatusFilter == "Completed") query = query.Where(p => p.Status == "Tamamlandı");
                else if (StatusFilter == "Cancelled") query = query.Where(p => p.Status == "İptal Edildi");
                else if (StatusFilter == "ActiveOrOpen") query = query.Where(p => p.Status == "Açık" || p.Status == "Aktif"); // Kart sayacı için

                var allEmployerProjects = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();

                // Görünüm için kategorize et
                ActiveProjects = allEmployerProjects.Where(p => p.Status == "Açık" || p.Status == "Aktif").ToList();
                CompletedProjects = allEmployerProjects.Where(p => p.Status == "Tamamlandı").ToList();

                // Eğer "All" veya başka bir filtre seçiliyse, UI'da hepsini Active listesi üzerinden de gösterebiliriz, 
                // ama listelemeyi daha düzgün yapmak için ActiveProjects'i "Gösterilecek Projeler" olarak kullanalım.
                ActiveProjects = allEmployerProjects; // Liste gösterimi için

                // Gerçek sayaçlar için ayrı sorgu (kartlardaki sayıları bozmamak için)
                var rawActive = await _context.Projects.CountAsync(p => p.EmployerId == userId && (p.Status == "Açık" || p.Status == "Aktif"));
                var rawCompleted = await _context.Projects.CountAsync(p => p.EmployerId == userId && p.Status == "Tamamlandı");
                
                ViewData["RawActiveCount"] = rawActive;
                ViewData["RawCompletedCount"] = rawCompleted;

                // Freelancer isimlerini çek
                var freelancerIds = allEmployerProjects.Where(p => p.FreelancerId != null).Select(p => p.FreelancerId.Value).Distinct().ToList();
                var freelancers = await _context.Users.Where(u => freelancerIds.Contains(u.Id)).ToListAsync();
                foreach (var f in freelancers) { UserNames[f.Id] = f.FullName; }

                // Toplam Harcanan
                // Toplam Harcanan
                TotalEarningsOrSpent = (int)(await _context.Projects.Where(p => p.EmployerId == userId && p.Status == "Tamamlandı").SumAsync(p => p.Budget));
            }
            else
            {
                // Freelancer'a atanmış aktif projeler
                ActiveProjects = await _context.Projects
                    .Where(p => p.FreelancerId == userId && p.Status == "Aktif")
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                // Freelancer'ın tamamladığı projeler
                CompletedProjects = await _context.Projects
                    .Where(p => p.FreelancerId == userId && p.Status == "Tamamlandı")
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                // Employer isimlerini çek
                var allProjects = ActiveProjects.Concat(CompletedProjects).ToList();
                var employerIds = allProjects.Select(p => p.EmployerId).Distinct().ToList();
                var employers = await _context.Users.Where(u => employerIds.Contains(u.Id)).ToListAsync();
                foreach (var e in employers) { UserNames[e.Id] = e.FullName; }

                // Toplam Kazanç
                TotalEarningsOrSpent = (int)CompletedProjects.Sum(p => p.Budget);
            }

            return Page();
        }

        // İPTAL ETME METODU
        public async Task<IActionResult> OnPostCancelProjectAsync(int id)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToPage("/Login");

            var userId = int.Parse(User.FindFirstValue("UserId") ?? "0");
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole != "Employer") return Unauthorized();

            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id && p.EmployerId == userId);
            
            if (project != null && (project.Status == "Açık" || project.Status == "Aktif"))
            {
                project.Status = "İptal Edildi";
                await _context.SaveChangesAsync();
            }

            return RedirectToPage(new { StatusFilter = StatusFilter });
        }
    }
}
