using System.ComponentModel.DataAnnotations;

namespace Vera.Models
{
    public class WorkLog
    {
        [Key]
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public double Hours { get; set; }
        public string Description { get; set; }
    }
}