using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Linq;
using Vera.Data;
using Vera.Models;

namespace Vera.Pages
{
    /// <summary>
    /// [GÜVENLİK & YETKİLENDİRME - ADMİN PANELİ]
    /// Platformun tüm yönetim işlemlerini tek bir merkezden kontrol etmeyi sağlayan Admin Panel sayfası.
    /// Sadece "Admin" rolüne sahip kullanıcılar erişebilir; diğer tüm roller (Freelancer, Employer)
    /// bu sayfaya ulaşmaya çalışırsa otomatik olarak Dashboard'a yönlendirilir.
    /// 
    /// Bu sınıf CRUD işlemlerinin tamamını (Create, Read, Update, Delete) içerir:
    /// - READ   : Tüm kullanıcı, proje ve log verilerini listeler.
    /// - UPDATE : Kullanıcı rollerini, bakiyelerini ve proje durumlarını günceller.
    /// - DELETE : Kullanıcıları ve projeleri sistemden kalıcı olarak siler.
    /// </summary>
    public class AdminModel : PageModel
    {
        private readonly CoreDbContext _coreContext;
        private readonly LogDbContext _logContext;

        // [MİMARİ] - Dependency Injection ile hem ana veritabanı hem log veritabanı enjekte ediliyor.
        // İki farklı DbContext kullanmamız "Separation of Concerns" (Kaygıların Ayrılması) prensibine uygundur.
        public AdminModel(CoreDbContext coreContext, LogDbContext logContext)
        {
            _coreContext = coreContext;
            _logContext = logContext;
        }

        // ============================
        // İSTATİSTİK VERİLERİ (Dashboard Kartları)
        // ============================
        public int TotalUsers { get; set; }
        public int TotalFreelancers { get; set; }
        public int TotalEmployers { get; set; }
        public int TotalProjects { get; set; }
        public int OpenProjects { get; set; }
        public int ActiveProjects { get; set; }
        public int CompletedProjects { get; set; }
        public decimal TotalRevenue { get; set; }          // Platformdaki toplam işlem hacmi
        public int TotalLogs { get; set; }

        // ============================
        // LİSTELEME VERİLERİ (Tablolar)
        // ============================
        public List<User> AllUsers { get; set; } = new();
        public List<Project> AllProjects { get; set; } = new();
        public List<ActivityLog> RecentLogs { get; set; } = new();

        // Proje sahibi isimlerini gösterebilmek için kullanıcı ID -> İsim eşleştirmesi
        public Dictionary<int, string> UserNames { get; set; } = new();

        // Bildirim ve hata mesajları
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        // ============================
        // SIRALAMA PARAMETRELERİ (Query String)
        // ============================
        [BindProperty(SupportsGet = true)]
        public string UserSort { get; set; } = "DateDesc"; // Varsayılan: Kayıt Tarihi (Yeni)

        [BindProperty(SupportsGet = true)]
        public string ProjectSort { get; set; } = "DateDesc"; // Varsayılan: Oluşturma Tarihi (Yeni)

        [BindProperty(SupportsGet = true)]
        public string LogSort { get; set; } = "DateDesc"; // Varsayılan: İşlem Tarihi (Yeni)

        [BindProperty(SupportsGet = true)]
        public string ActiveTab { get; set; } = "users"; // Varsayılan sekme: Kullanıcılar

        /// <summary>
        /// [CRUD - READ] Sayfa ilk yüklendiğinde tüm verileri toplar ve arayüze sunar.
        /// Backend'de "Admin" rolü kontrolü yapılarak güvenlik sağlanır.
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            // [GÜVENLİK] - Sadece "Admin" rolüne sahip kullanıcılar bu panele erişebilir.
            // Yetkisiz erişim denemelerinde kullanıcı Dashboard'a yönlendirilir.
            if (!User.Identity.IsAuthenticated || User.FindFirstValue(ClaimTypes.Role) != "Admin")
                return RedirectToPage("/Dashboard");

            await LoadDashboardData();
            return Page();
        }

        /// <summary>
        /// [CRUD - DELETE] Bir kullanıcıyı ve ona ait tüm projeleri sistemden kalıcı olarak siler.
        /// Cascade mantığıyla çalışır: Kullanıcı silinince projeleri de uçar (Veri bütünlüğü korunur).
        /// </summary>
        public async Task<IActionResult> OnPostDeleteUserAsync(int userId)
        {
            // [GÜVENLİK] - POST işlemlerinde de rol kontrolü tekrar yapıyoruz.
            // Böylece Postman veya dış kaynaklardan atılan sahte istekler engellenir.
            if (!User.Identity.IsAuthenticated || User.FindFirstValue(ClaimTypes.Role) != "Admin")
                return RedirectToPage("/Dashboard");

            var user = await _coreContext.Users.FindAsync(userId);
            if (user != null)
            {
                // Admin kendi hesabını silemez (Güvenlik önlemi)
                var currentUserId = int.Parse(User.FindFirstValue("UserId") ?? "0");
                if (user.Id == currentUserId)
                {
                    ErrorMessage = "Kendi admin hesabınızı silemezsiniz!";
                    await LoadDashboardData();
                    return Page();
                }

                // Kullanıcıya ait projeleri de temizle (Orphan Data oluşmasını engelle)
                var userProjects = await _coreContext.Projects
                    .Where(p => p.EmployerId == userId || p.FreelancerId == userId)
                    .ToListAsync();
                _coreContext.Projects.RemoveRange(userProjects);

                // Kullanıcıya ait logları da temizle
                var userLogs = await _logContext.Logs.Where(l => l.UserId == userId).ToListAsync();
                _logContext.Logs.RemoveRange(userLogs);

                // Kullanıcının mesajlarını da temizle
                var userMessages = await _logContext.Messages
                    .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                    .ToListAsync();
                _logContext.Messages.RemoveRange(userMessages);

                _coreContext.Users.Remove(user);
                await _coreContext.SaveChangesAsync();
                await _logContext.SaveChangesAsync();

                // Admin aktivite logu oluştur
                var adminId = int.Parse(User.FindFirstValue("UserId") ?? "0");
                _logContext.Logs.Add(new ActivityLog
                {
                    UserId = adminId,
                    Action = "Kullanıcı Silindi",
                    Details = $"'{user.FullName}' ({user.Email}) hesabı admin tarafından silindi.",
                    Timestamp = DateTime.Now
                });
                await _logContext.SaveChangesAsync();

                SuccessMessage = $"'{user.FullName}' adlı kullanıcı ve tüm ilişkili verileri başarıyla silindi.";
            }

            await LoadDashboardData();
            return Page();
        }

        /// <summary>
        /// [CRUD - DELETE] Bir projeyi sistemden kalıcı olarak siler.
        /// Proje bütçesi işverene iade edilir (Finansal bütünlük korunur).
        /// </summary>
        public async Task<IActionResult> OnPostDeleteProjectAsync(int projectId)
        {
            if (!User.Identity.IsAuthenticated || User.FindFirstValue(ClaimTypes.Role) != "Admin")
                return RedirectToPage("/Dashboard");

            var project = await _coreContext.Projects.FindAsync(projectId);
            if (project != null)
            {
                // [İŞ MANTIĞI] - Eğer proje henüz açık veya aktifse, bütçeyi işverene geri iade et.
                // Böylece haksız bakiye kaybının önüne geçilmiş olur.
                if (project.Status == "Açık" || project.Status == "Aktif")
                {
                    var employer = await _coreContext.Users.FindAsync(project.EmployerId);
                    if (employer != null)
                    {
                        employer.WalletBalance += project.Budget;
                    }
                }

                _coreContext.Projects.Remove(project);
                await _coreContext.SaveChangesAsync();

                // Admin aktivite logu
                var adminId = int.Parse(User.FindFirstValue("UserId") ?? "0");
                _logContext.Logs.Add(new ActivityLog
                {
                    UserId = adminId,
                    Action = "Proje Silindi",
                    Details = $"'{project.Title}' projesi admin tarafından silindi. Bütçe: ₺{project.Budget:N0}",
                    Timestamp = DateTime.Now
                });
                await _logContext.SaveChangesAsync();

                SuccessMessage = $"'{project.Title}' projesi başarıyla silindi.";
            }

            await LoadDashboardData();
            return Page();
        }

        /// <summary>
        /// [CRUD - UPDATE] Bir kullanıcının rolünü değiştirir (Freelancer ↔ Employer).
        /// Admin rolüne yükseltme bu metottan yapılamaz (Güvenlik prensibi).
        /// </summary>
        public async Task<IActionResult> OnPostChangeRoleAsync(int userId, string newRole)
        {
            if (!User.Identity.IsAuthenticated || User.FindFirstValue(ClaimTypes.Role) != "Admin")
                return RedirectToPage("/Dashboard");

            // [GÜVENLİK] - Sadece izin verilen roller atanabilir. Admin rolü bu şekilde verilemez.
            if (newRole != "Freelancer" && newRole != "Employer")
            {
                ErrorMessage = "Geçersiz rol seçimi!";
                await LoadDashboardData();
                return Page();
            }

            var user = await _coreContext.Users.FindAsync(userId);
            if (user != null && user.Role != "Admin")
            {
                var oldRole = user.Role;
                user.Role = newRole;
                await _coreContext.SaveChangesAsync();

                // Admin aktivite logu
                var adminId = int.Parse(User.FindFirstValue("UserId") ?? "0");
                _logContext.Logs.Add(new ActivityLog
                {
                    UserId = adminId,
                    Action = "Rol Değiştirildi",
                    Details = $"'{user.FullName}' kullanıcısının rolü '{oldRole}' → '{newRole}' olarak güncellendi.",
                    Timestamp = DateTime.Now
                });
                await _logContext.SaveChangesAsync();

                SuccessMessage = $"'{user.FullName}' kullanıcısının rolü '{newRole}' olarak güncellendi.";
            }

            await LoadDashboardData();
            return Page();
        }

        /// <summary>
        /// [CRUD - UPDATE] Bir kullanıcının cüzdan bakiyesini manuel olarak günceller.
        /// Negatif bakiye de atanabilir (Borçlu hesaplar için).
        /// </summary>
        public async Task<IActionResult> OnPostUpdateBalanceAsync(int userId, decimal newBalance)
        {
            if (!User.Identity.IsAuthenticated || User.FindFirstValue(ClaimTypes.Role) != "Admin")
                return RedirectToPage("/Dashboard");

            var user = await _coreContext.Users.FindAsync(userId);
            if (user != null)
            {
                var oldBalance = user.WalletBalance;
                user.WalletBalance = newBalance;
                await _coreContext.SaveChangesAsync();

                // Admin aktivite logu
                var adminId = int.Parse(User.FindFirstValue("UserId") ?? "0");
                _logContext.Logs.Add(new ActivityLog
                {
                    UserId = adminId,
                    Action = "Bakiye Güncellendi",
                    Details = $"'{user.FullName}' bakiyesi ₺{oldBalance:N0} → ₺{newBalance:N0} olarak güncellendi.",
                    Timestamp = DateTime.Now
                });
                await _logContext.SaveChangesAsync();

                SuccessMessage = $"'{user.FullName}' kullanıcısının bakiyesi ₺{newBalance:N0} olarak güncellendi.";
            }

            await LoadDashboardData();
            return Page();
        }

        /// <summary>
        /// Tüm istatistikleri ve listeleri tek seferde çeken yardımcı metot.
        /// DRY (Don't Repeat Yourself) prensibi gereği her handler bu metodu çağırır.
        /// </summary>
        private async Task LoadDashboardData()
        {
            // İstatistikler
            TotalUsers = await _coreContext.Users.CountAsync(u => u.Role != "Admin");
            TotalFreelancers = await _coreContext.Users.CountAsync(u => u.Role == "Freelancer");
            TotalEmployers = await _coreContext.Users.CountAsync(u => u.Role == "Employer");
            TotalProjects = await _coreContext.Projects.CountAsync();
            OpenProjects = await _coreContext.Projects.CountAsync(p => p.Status == "Açık");
            ActiveProjects = await _coreContext.Projects.CountAsync(p => p.Status == "Aktif");
            CompletedProjects = await _coreContext.Projects.CountAsync(p => p.Status == "Tamamlandı");
            TotalRevenue = await _coreContext.Projects.SumAsync(p => p.Budget);
            TotalLogs = await _logContext.Logs.CountAsync();

            // [KULLANICI SIRALAMA MANTIĞI]
            var userQuery = _coreContext.Users.Where(u => u.Role != "Admin");
            userQuery = (UserSort ?? "DateDesc") switch
            {
                "Id" => userQuery.OrderBy(u => u.Id),
                "IdDesc" => userQuery.OrderByDescending(u => u.Id),
                "Name" => userQuery.OrderBy(u => u.FullName),
                "NameDesc" => userQuery.OrderByDescending(u => u.FullName),
                "Email" => userQuery.OrderBy(u => u.Email),
                "Role" => userQuery.OrderBy(u => u.Role),
                "Balance" => userQuery.OrderBy(u => u.WalletBalance),
                "BalanceDesc" => userQuery.OrderByDescending(u => u.WalletBalance),
                "Date" => userQuery.OrderBy(u => u.CreatedAt),
                "DateDesc" => userQuery.OrderByDescending(u => u.CreatedAt),
                _ => userQuery.OrderByDescending(u => u.CreatedAt)
            };
            AllUsers = await userQuery.ToListAsync();

            // [PROJE SIRALAMA MANTIĞI]
            var projectQuery = _coreContext.Projects.AsQueryable();
            projectQuery = (ProjectSort ?? "DateDesc") switch
            {
                "Id" => projectQuery.OrderBy(p => p.Id),
                "IdDesc" => projectQuery.OrderByDescending(p => p.Id),
                "Title" => projectQuery.OrderBy(p => p.Title),
                "TitleDesc" => projectQuery.OrderByDescending(p => p.Title),
                "Budget" => projectQuery.OrderBy(p => p.Budget),
                "BudgetDesc" => projectQuery.OrderByDescending(p => p.Budget),
                "Status" => projectQuery.OrderBy(p => p.Status),
                "Date" => projectQuery.OrderBy(p => p.CreatedAt),
                "DateDesc" => projectQuery.OrderByDescending(p => p.CreatedAt),
                _ => projectQuery.OrderByDescending(p => p.CreatedAt)
            };
            AllProjects = await projectQuery.Take(50).ToListAsync();

            // [LOG SIRALAMA MANTIĞI]
            var logQuery = _logContext.Logs.AsQueryable();
            logQuery = (LogSort ?? "DateDesc") switch
            {
                "Id" => logQuery.OrderBy(l => l.Id),
                "Action" => logQuery.OrderBy(l => l.Action),
                "ActionDesc" => logQuery.OrderByDescending(l => l.Action),
                "Date" => logQuery.OrderBy(l => l.Timestamp),
                "DateDesc" => logQuery.OrderByDescending(l => l.Timestamp),
                _ => logQuery.OrderByDescending(l => l.Timestamp)
            };
            RecentLogs = await logQuery.Take(20).ToListAsync();

            // İsim eşleştirme sözlüğü (ID -> FullName)
            var allUsersList = await _coreContext.Users.ToListAsync();
            UserNames = allUsersList.ToDictionary(u => u.Id, u => u.FullName);
        }
    }
}
