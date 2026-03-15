using System.ComponentModel.DataAnnotations;

namespace E_Commerce_BackendAPI.Dtos
{
    public class AddToCartRequest
    {
        [Required]
        public int ProductId { get; set; }
        [Required, Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }

    public class UpdateCartItemRequest
    {
        [Required, Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
