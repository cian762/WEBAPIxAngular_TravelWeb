namespace TravelWeb_API.DTO.ActivityDTO
{
    public class ReviewPackageResponseDTO
    {
        public int ActivityId { get; set; }

        public List<ReviewResponseDTO>? Reviews { get; set; }

        public decimal AverageRating { get; set; }

        public int CommentCount { get; set; }

    }
}
