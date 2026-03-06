using System.ComponentModel.DataAnnotations;

namespace E_Commerce_BackendAPI.Model
{
    public class OrderItem:BaseEntity
    {
        public int OrderId { get; set; }
        public Order Order { get; set; }    
        public int ProductId { get; set; }
        public Product Product { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; }
    }
}
