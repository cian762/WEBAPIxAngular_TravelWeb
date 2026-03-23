using Microsoft.AspNetCore.Http.Metadata;

namespace TravelWeb_API.DTO.ActivityDTO
{
    public class ReviewEditRequestDTO
    {
        public int ReviewId { get; set; }

        public required string Title { get; set; }

        public string? Comment { get; set; }

        public int Rating { get; set; }

        public List<IFormFile>? NewImages { get; set; }

        public List<string>? DeletedImageUrls { get; set; }

    }
}
