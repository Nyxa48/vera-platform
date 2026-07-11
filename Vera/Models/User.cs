using System.ComponentModel.DataAnnotations;

namespace Vera.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } // Admin, Freelancer, Employer
        public decimal WalletBalance { get; set; }
        public string? Title { get; set; }
        public string? About { get; set; }
        public string? Skills { get; set; } // Virgülle ayrılmış uzmanlıklar: "C#, .NET, SQL"
        public string? IBAN { get; set; } // Banka bilgisi (Opsiyonel)
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}