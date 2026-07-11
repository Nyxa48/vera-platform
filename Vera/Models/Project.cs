using System.ComponentModel.DataAnnotations;

namespace Vera.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Budget { get; set; }
        public bool IsHourly { get; set; }
        public int EmployerId { get; set; }
        public int? FreelancerId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? Deadline { get; set; }         // Son Başvuru / Teslim Tarihi
        public string? RequiredLevel { get; set; }      // Giriş, Orta, Uzman
        public string? Tags { get; set; }               // Etiketler (virgülle)
    }
}