using System.ComponentModel.DataAnnotations;

namespace TravelWeb_API.QueryParameters.ActivityQueryParameters
{
    public class ActivityInfoParameters
    {
        public string? Keyword { get; set; }

        public string[]? Type { get; set; } = null;
        public string[]? Region { get; set; } = null;
        public DateOnly? Start { get; set; } = null;
        public DateOnly? End { get; set; } = null;

        [Range(1, int.MaxValue, ErrorMessage = "頁碼必須大於 0")]
        public int PageNumber { get; set; } = 1;

        [Range(1, 50, ErrorMessage = "每頁筆數限制在 1-50 之間")]
        public int PageSize { get; set; } = 12;

        public bool OrderByPopularity { get; set; } = false;
        public bool IsLatest { get; set; } = false;
        public bool IsObsolete { get; set; } = false;
    }
}
