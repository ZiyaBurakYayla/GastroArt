using GastroArt.Models.Entity;
using System.ComponentModel.DataAnnotations;

namespace GastroArt.Models.Data
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        public string TableNo { get; set; }
        public string? CustomerNote { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public bool IsCompleted { get; set; } = false;
        public int? WorkPeriodId { get; set; } 
        public OrderState OrderState { get; set; } = OrderState.Pending; // Enum
        public List<OrderItem> OrderItems { get; set; }
    }
}