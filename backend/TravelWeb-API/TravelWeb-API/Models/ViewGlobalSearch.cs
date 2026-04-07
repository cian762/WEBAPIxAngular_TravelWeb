namespace TravelWeb_API.Models
{
    public class ViewGlobalSearch
    {
        // 雖然 View 沒有 Primary Key，但 EF 建議類別名稱與屬性要對齊
        public string? Id { get; set; }

        public string? Title { get; set; }
        public string? Category { get; set; }
        public string? ImageUrl { get; set; }
    }
}
