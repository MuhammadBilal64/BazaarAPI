using System.ComponentModel.DataAnnotations;

namespace E_Commerce_BackendAPI.Dtos
{
    public class UpdateOrderStatusRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty; // Created, Paid, Shipped, Cancelled
    }
}
