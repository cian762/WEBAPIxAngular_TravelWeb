using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.ActivityModel;

public partial class PersonalizedRecommendation
{
    public int RecommendId { get; set; }

    public int UserId { get; set; }

    public int? ActivityId { get; set; }

    public string? PreferenceBehavior { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Activity? Activity { get; set; }
}
