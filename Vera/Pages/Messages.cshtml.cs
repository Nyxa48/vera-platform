using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Vera.Data;
using Vera.Models;

namespace Vera.Pages
{
    public class MessagesModel : PageModel
    {
        private readonly LogDbContext _logContext;
        private readonly CoreDbContext _coreContext;

        public MessagesModel(LogDbContext logContext, CoreDbContext coreContext)
        {
            _logContext = logContext;
            _coreContext = coreContext;
        }

        public List<Message> MyMessages { get; set; }
        public Dictionary<int, string> UserNames { get; set; } = new Dictionary<int, string>();
        public int CurrentUserId { get; set; }

        // Konuşma listesi: her kişiyle son mesaj + okunmamış sayısı
        public List<ConversationSummary> Conversations { get; set; } = new List<ConversationSummary>();

        // Aktif sohbet
        public int? ActiveChatUserId { get; set; }
        public string ActiveChatUserName { get; set; }
        public List<Message> ChatMessages { get; set; }

        // Mesaj gönderme
        [BindProperty]
        public string NewMessageContent { get; set; }

        public async Task<IActionResult> OnGetAsync(int? chatWith)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToPage("/Login");

            CurrentUserId = int.Parse(User.FindFirstValue("UserId") ?? "0");

            // Tüm mesajları çek
            MyMessages = await _logContext.Messages
                .Where(m => m.ReceiverId == CurrentUserId || m.SenderId == CurrentUserId)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            // Kullanıcı isimlerini bul
            var userIds = MyMessages.Select(m => m.SenderId)
                .Concat(MyMessages.Select(m => m.ReceiverId))
                .Distinct().ToList();
            var users = await _coreContext.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();
            foreach (var u in users) { UserNames[u.Id] = u.FullName; }

            // Konuşma listesi oluştur (her kişiyle son mesaj)
            var conversationPartners = userIds.Where(id => id != CurrentUserId).Distinct();
            foreach (var partnerId in conversationPartners)
            {
                var lastMsg = MyMessages
                    .Where(m => (m.SenderId == partnerId && m.ReceiverId == CurrentUserId) ||
                                (m.SenderId == CurrentUserId && m.ReceiverId == partnerId))
                    .OrderByDescending(m => m.SentAt)
                    .FirstOrDefault();

                var unreadCount = MyMessages
                    .Count(m => m.SenderId == partnerId && m.ReceiverId == CurrentUserId && !m.IsRead);

                if (lastMsg != null)
                {
                    Conversations.Add(new ConversationSummary
                    {
                        UserId = partnerId,
                        UserName = UserNames.ContainsKey(partnerId) ? UserNames[partnerId] : "Bilinmeyen",
                        LastMessage = lastMsg.Content,
                        LastMessageTime = lastMsg.SentAt,
                        UnreadCount = unreadCount
                    });
                }
            }
            Conversations = Conversations.OrderByDescending(c => c.LastMessageTime).ToList();

            // Aktif sohbet varsa mesajları yükle
            if (chatWith.HasValue && chatWith.Value > 0)
            {
                ActiveChatUserId = chatWith.Value;
                ActiveChatUserName = UserNames.ContainsKey(chatWith.Value) ? UserNames[chatWith.Value] : "Bilinmeyen";

                // Eğer isim bulunamadıysa, DB'den çek
                if (ActiveChatUserName == "Bilinmeyen")
                {
                    var chatUser = await _coreContext.Users.FindAsync(chatWith.Value);
                    if (chatUser != null)
                    {
                        ActiveChatUserName = chatUser.FullName;
                        UserNames[chatWith.Value] = chatUser.FullName;
                    }
                }

                ChatMessages = MyMessages
                    .Where(m => (m.SenderId == chatWith.Value && m.ReceiverId == CurrentUserId) ||
                                (m.SenderId == CurrentUserId && m.ReceiverId == chatWith.Value))
                    .OrderBy(m => m.SentAt)
                    .ToList();

                // Bu sohbetteki okunmamış mesajları okundu yap
                var unread = ChatMessages.Where(m => m.ReceiverId == CurrentUserId && !m.IsRead).ToList();
                if (unread.Any())
                {
                    foreach (var m in unread) m.IsRead = true;
                    await _logContext.SaveChangesAsync();
                }
            }
            else if (Conversations.Any())
            {
                // Otomatik olarak ilk konuşmayı aç
                return RedirectToPage(new { chatWith = Conversations.First().UserId });
            }

            return Page();
        }

        public async Task<IActionResult> OnPostSendMessageAsync(int chatWith)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToPage("/Login");
            if (string.IsNullOrWhiteSpace(NewMessageContent)) return RedirectToPage(new { chatWith });

            var senderId = int.Parse(User.FindFirstValue("UserId") ?? "0");

            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = chatWith,
                Content = NewMessageContent.Trim(),
                SentAt = DateTime.Now,
                IsRead = false
            };

            _logContext.Messages.Add(message);
            await _logContext.SaveChangesAsync();

            return RedirectToPage(new { chatWith });
        }

        // Konuşma özet sınıfı
        public class ConversationSummary
        {
            public int UserId { get; set; }
            public string UserName { get; set; }
            public string LastMessage { get; set; }
            public DateTime LastMessageTime { get; set; }
            public int UnreadCount { get; set; }
        }
    }
}