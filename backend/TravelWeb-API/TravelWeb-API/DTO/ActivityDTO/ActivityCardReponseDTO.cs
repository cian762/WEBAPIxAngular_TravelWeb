namespace TravelWeb_API.DTO.ActivityDTO
{
    public class ActivityCardReponseDTO
    {
        public int ActivityId { get; set; }
        public string? Title { get; set; }
        public IEnumerable<string?>? Type { get; set; }
        public IEnumerable<string>? Region { get; set; }
        public DateOnly? Start { get; set; }
        public DateOnly? End { get; set; }
        public string? CoverImageUrl { get; set; }
        public int ReferencePrice { get; set; }


        //待加入的欄位
        public int ViewCount { get; set; }
        public int SellCount { get; set; }
        public int CommentCount { get; set; }
        public float AverageRating { get; set; }
    }
}
