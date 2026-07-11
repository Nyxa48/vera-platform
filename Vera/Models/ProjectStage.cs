using System.ComponentModel.DataAnnotations;

namespace Vera.Models
{
    public class ProjectStage
    {
        [Key]
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Title { get; set; } // Örn: Tasarım, Kodlama, Test
        public bool IsCompleted { get; set; }
        public int Order { get; set; } // Aşamaların sırası
    }
}