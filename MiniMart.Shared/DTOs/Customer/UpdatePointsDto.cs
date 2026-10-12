using System.ComponentModel.DataAnnotations;

namespace MiniMart.Shared.DTOs.Customer
{
    public class UpdatePointsDto
    {
        [Required]
        public int TotalPoints { get; set; }
    }
}
