using System.ComponentModel.DataAnnotations;

namespace E_Commerce_BackendAPI.Dtos
{
    public class CreateCategoryDto
    {
        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class UpdateCategoryDto
    {
        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
