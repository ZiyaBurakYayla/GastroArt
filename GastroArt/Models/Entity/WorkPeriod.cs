using System.ComponentModel.DataAnnotations;

namespace GastroArt.Models.Data
{
    public class WorkPeriod
    {
        [Key]
        public int Id { get; set; }
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime? EndTime { get; set; }
        public decimal TotalRevenue { get; set; }
        public bool IsActive { get; set; } = true;
    }
}