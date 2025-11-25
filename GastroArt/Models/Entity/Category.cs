using System.ComponentModel.DataAnnotations;

namespace GastroArt.Models.Entity
{
    public class Category
    {
        [Key] 
        public int Id { get; set; }
        public string Name { get; set; }
        public string ColorCode { get; set; } 
        public string? Description { get; set; } 
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }

        public List<Product> Products { get; set; }
        public string NameEn { get; set; }
    }
}
