using System.ComponentModel.DataAnnotations;

namespace E_Commerce_BackendAPI.Model
{
    public class Category:BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public string Description { get; set; }
        public List<Product> Products { get; set; } = new();

    }
}
