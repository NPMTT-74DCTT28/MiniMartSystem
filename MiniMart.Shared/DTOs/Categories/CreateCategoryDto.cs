using System.ComponentModel.DataAnnotations;

namespace MiniMart.Shared.DTOs.Categories
{
    public class CreateCategoryDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;
    }
}
