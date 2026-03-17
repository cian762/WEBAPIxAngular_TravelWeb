using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.ActivityModel;

public partial class ReviewImage
{
    public int ImageSetId { get; set; }

    public int ReviewId { get; set; }

    public string PublicId { get; set; } = null!;

    public string ImageUrl { get; set; } = null!;

    public virtual ProductReview Review { get; set; } = null!;
}
