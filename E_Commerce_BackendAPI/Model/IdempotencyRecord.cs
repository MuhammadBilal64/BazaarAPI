using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Commerce_BackendAPI.Model
{
    public class IdempotencyRecord
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string IdempotencyKey { get; set; } = string.Empty;

        [Required]
        [MaxLength(64)]  
        public string RequestHash { get; set; } = string.Empty;

        [Required]
        public string ResponseBody { get; set; } = string.Empty;

        [Required]
        public int ResponseStatusCode { get; set; }  // ← Fixed: int, not string

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ProcessedAt { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public int? OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order? Order { get; set; }
    }
}