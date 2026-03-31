using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.attraction;

public partial class Attraction
{
    public int AttractionId { get; set; }

    public string Name { get; set; } = null!;

    public string? Address { get; set; }

    public int RegionId { get; set; }

    public string? AreaId { get; set; }

    public int? ApprovalStatus { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public string? OpendataId { get; set; }

    public string? Phone { get; set; }

    public string? Website { get; set; }

    public string? BusinessHours { get; set; }

    public string? GooglePlaceId { get; set; }

    public string? ClosedDaysNote { get; set; }

    public string? TransportInfo { get; set; }

    public string? Description { get; set; }  // 景點介紹文字

    public string? ActivityIntro { get; set; }  // 售票區活動介紹文字

    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    // ✅ 新增：點擊數
    public int ViewCount { get; set; }

    public virtual ICollection<AttractionProduct> AttractionProducts { get; set; } = new List<AttractionProduct>();

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();

    // ✅ 新增：按讚導覽屬性
    public virtual ICollection<AttractionLike> AttractionLikes { get; set; } = new List<AttractionLike>();

    public virtual TagsRegion Region { get; set; } = null!;
}
