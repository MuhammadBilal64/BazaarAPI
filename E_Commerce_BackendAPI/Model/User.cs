using System.ComponentModel.DataAnnotations;
using E_Commerce_BackendAPI.Utilities;

namespace E_Commerce_BackendAPI.Model
{
    public class User:BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public String Name { get; set; }
        [Required]
        [EmailAddress]
        public String Email { get; set; }
        [Required]
        [MinLength(6)]
        public String Password { get; set; }
        [Required]
        public UserRole Role { get; set; } = UserRole.Customer;
        public List<CartItem> CartItems { get; set; }
        public List<Order> Orders { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; }



    }
}
