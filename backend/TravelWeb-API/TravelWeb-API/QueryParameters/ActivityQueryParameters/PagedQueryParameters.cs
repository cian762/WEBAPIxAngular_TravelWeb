using System.ComponentModel.DataAnnotations;

namespace TravelWeb_API.QueryParameters.ActivityQueryParameters
{
    public class PagedQueryParameters
    {
        [Range(1, int.MaxValue, ErrorMessage = "頁碼必須大於 0")]
        public int PageNumber { get; set; } = 1;

        [Range(1, 50, ErrorMessage = "每頁筆數限制在 1-50 之間")]
        public int PageSize { get; set; } = 12;

    }
}
