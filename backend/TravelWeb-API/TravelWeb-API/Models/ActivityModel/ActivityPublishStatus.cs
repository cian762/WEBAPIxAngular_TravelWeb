using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.ActivityModel;

public partial class ActivityPublishStatus
{
    public int ActivityId { get; set; }

    public DateTime? PublishTime { get; set; }

    public DateTime? UnPublishTime { get; set; }

    public string? Status { get; set; }

    public virtual Activity Activity { get; set; } = null!;
}
