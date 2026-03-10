using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.ActivityModel;

public partial class ActivityImage
{
    public int ImageSetId { get; set; }

    public int? ActivityId { get; set; }

    public string? PublicId { get; set; }

    public string? ImageUrl { get; set; }

    public bool? IsCoverImage { get; set; }

    public virtual Activity? Activity { get; set; }
}
