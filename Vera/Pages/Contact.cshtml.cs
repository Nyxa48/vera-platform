using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Vera.Data;
using Vera.Models;

namespace Vera.Pages
{
    public class ContactModel : PageModel
    {
        private readonly CoreDbContext _coreContext;
        private readonly LogDbContext _logContext;

        public ContactModel(CoreDbContext coreContext, LogDbContext logContext)
        {
            _coreContext = coreContext;
            _logContext = logContext;
        }

        [BindProperty]
        public string Subject { get; set; }

        [BindProperty]
        public string MessageBody { get; set; }

        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            // Kullanıcı girişi zorunlu kılıyoruz
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Login");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToPage("/Login");

            if (string.IsNullOrWhiteSpace(Subject) || string.IsNullOrWhiteSpace(MessageBody))
            {
                ErrorMessage = "Lütfen tüm alanları doldurunuz.";
                return Page();
            }

            // Admin hesabını buluyoruz
            var adminUser = await _coreContext.Users.FirstOrDefaultAsync(u => u.Role == "Admin");
            
            if (adminUser == null)
            {
                ErrorMessage = "Sistemde kayıtlı bir yönetici bulunamadı. Lütfen daha sonra tekrar deneyin.";
                return Page();
            }

            var currentUserId = int.Parse(User.FindFirstValue("UserId") ?? "0");

            // Mesajı oluştur ve Log veritabanındaki Messages tablosuna ekle
            var message = new Message
            {
                SenderId = currentUserId,
                ReceiverId = adminUser.Id,
                Content = $"[DESTEK TALEBİ] Konu: {Subject}\n\n{MessageBody}",
                SentAt = DateTime.Now,
                IsRead = false
            };

            _logContext.Messages.Add(message);
            await _logContext.SaveChangesAsync();

            // [BİLDİRİM] - Yöneticiye bildirim gönder (ActivityLog)
            _logContext.Logs.Add(new ActivityLog
            {
                UserId = adminUser.Id,
                Action = "Yeni Destek Talebi",
                Details = $"{User.Identity.Name} kullanıcısından yeni bir mesajınız var: {Subject}",
                Timestamp = DateTime.Now
            });
            await _logContext.SaveChangesAsync();

            SuccessMessage = "Mesajınız başarıyla yöneticimize iletilmiştir. Size en kısa sürede 'Mesajlar' bölümünden dönüş yapılacaktır.";
            
            // Formu temizle
            Subject = "";
            MessageBody = "";

            return Page();
        }
    }
}
