using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.ActivityModel;

public partial class ProductReview
{
    public int ReviewId { get; set; }

    public string MemberId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Comment { get; set; }

    public int Rating { get; set; }

    public DateOnly CreateDate { get; set; }

    public virtual ICollection<ReviewImage> ReviewImages { get; set; } = new List<ReviewImage>();

    public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();
}
