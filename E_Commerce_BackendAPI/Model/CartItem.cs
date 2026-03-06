using System.ComponentModel.DataAnnotations;
using E_Commerce_BackendAPI.Utilities;

namespace E_Commerce_BackendAPI.Model
{
    public class CartItem:BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        public CartStatus Status { get; set; } = CartStatus.Active;

    }
}
