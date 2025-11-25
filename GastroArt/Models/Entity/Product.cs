using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GastroArt.Models.Entity
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")] 
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        public int Stock { get; set; }
        public bool TrackStock { get; set; } = false;

        public string NameEn { get; set; } 
        public string DescriptionEn { get; set; } 
    }
}
