using System.ComponentModel.DataAnnotations;

namespace E_Commerce_BackendAPI.Model
{
    public class Product:BaseEntity
    {
        public string Name { get; set; }    
        public string? Description { get; set; }
        [Required]
        [Range(0.01, double.MaxValue)] 
        public decimal Price { get; set; }
        [Required]
        public int Stock { get; set; }  
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public List<CartItem> CartItems { get; set; }




    }
}
