using System.ComponentModel.DataAnnotations;

namespace TravelWeb_API.DTO.ActivityDTO
{
    public class ReviewRequestDTO
    {
        public int ActivityId { get; set; }

        [Required]
        public required string Title { get; set; }
        
        public string? Comment { get; set; }

        [Required]
        public int Rating { get; set; }

        public List<IFormFile>? ReviewImages { get; set; }
    }
}
