using System.ComponentModel.DataAnnotations;

namespace E_Commerce_BackendAPI.Dtos
{
    public class CreateProductDto
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required, Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }
        [Required, Range(0, int.MaxValue)]
        public int Stock { get; set; }
        [Required]
        public int CategoryId { get; set; }
    }

    public class UpdateProductDto
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required, Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }
        [Required, Range(0, int.MaxValue)]
        public int Stock { get; set; }
        [Required]
        public int CategoryId { get; set; }
    }
}
