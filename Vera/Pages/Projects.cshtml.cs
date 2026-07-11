using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Vera.Data;
using Vera.Models;

namespace Vera.Pages
{
    public class ProjectsModel : PageModel
    {
        private readonly CoreDbContext _context;
        public ProjectsModel(CoreDbContext context) { _context = context; }

        public List<Project> ProjectList { get; set; } = new List<Project>();

        // HTML'deki asp-for="Search" vb. alanların çalışması için bu özellikler ŞART:
        [BindProperty(SupportsGet = true)]
        public string Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Sort { get; set; } // "price_asc", "price_desc", "newest"

        [BindProperty(SupportsGet = true)]
        public string Type { get; set; } // "hourly", "fixed"

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            // Sadece "Açık" olan projeleri getiriyoruz
            IQueryable<Project> query = _context.Projects.Where(p => p.Status == "Açık");

            // 🔍 Arama Filtresi
            if (!string.IsNullOrEmpty(Search))
            {
                query = query.Where(p => p.Title.Contains(Search) || p.Description.Contains(Search));
            }

            // 🕒 Ücret Tipi Filtresi (Saatlik mi Sabit mi?)
            if (Type == "hourly") query = query.Where(p => p.IsHourly);
            else if (Type == "fixed") query = query.Where(p => !p.IsHourly);

            // ↕️ Sıralama Mantığı
            query = Sort switch
            {
                "price_asc" => query.OrderBy(p => p.Budget),
                "price_desc" => query.OrderByDescending(p => p.Budget),
                _ => query.OrderByDescending(p => p.CreatedAt) // Varsayılan: En Yeni
            };

            // 📄 Sayfalama (Emir'in istediği gibi her sayfada max 6 tane)
            int pageSize = 6;
            var totalItems = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            ProjectList = await query
                .Skip((CurrentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}