using System.ComponentModel.DataAnnotations;

namespace Vera.Models
{
    public class ActivityLog
    {
        [Key]
        public int Id { get; set; }
        public string Action { get; set; } // Örn: "Teklif Verildi"
        public string Details { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public int UserId { get; set; }
    }
}