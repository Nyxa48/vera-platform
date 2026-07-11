using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Vera.Data;
using Vera.Models;

namespace Vera.Pages
{
    public class InvoiceModel : PageModel
    {
        private readonly CoreDbContext _context;

        public InvoiceModel(CoreDbContext context)
        {
            _context = context;
        }

        public Project InvoiceProject { get; set; }
        public User Freelancer { get; set; }
        public User Employer { get; set; }
        public List<WorkLog> WorkLogs { get; set; } // Detaylı çalışma kayıtları
        public double TotalHours { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; } // %20 KDV hesaplayacağız

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToPage("/Login");

            // Projeyi Bul
            InvoiceProject = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (InvoiceProject == null) return RedirectToPage("/Dashboard");

            // Freelancer'ı (Giriş Yapanı) Bul
            var email = User.FindFirstValue(ClaimTypes.Email);
            Freelancer = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            // Müşteriyi (İşvereni) Bul
            Employer = await _context.Users.FirstOrDefaultAsync(u => u.Id == InvoiceProject.EmployerId);

            // Tüm Çalışma Kayıtlarını Çek (Fatura satırları için)
            WorkLogs = await _context.WorkLogs
                .Where(l => l.ProjectId == id)
                .OrderBy(l => l.Date)
                .ToListAsync();

            TotalHours = WorkLogs.Sum(l => l.Hours);

            // Toplam tutarı projenin tipine göre (IsHourly) hesaplıyoruz
            if (InvoiceProject.IsHourly)
            {
                // Saatlik Ücret (Budget) * Toplam Çalışılan Saat
                TotalAmount = InvoiceProject.Budget * (decimal)TotalHours;
            }
            else
            {
                // Sabit Bütçeli Proje
                TotalAmount = InvoiceProject.Budget;
            }

            TaxAmount = TotalAmount * 0.20m; // %20 KDV hesapla

            return Page();
        }
    }
}