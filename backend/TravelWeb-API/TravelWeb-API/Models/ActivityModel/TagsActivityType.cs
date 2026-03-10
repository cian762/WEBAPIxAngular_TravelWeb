using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.ActivityModel;

public partial class TagsActivityType
{
    public int TypeId { get; set; }

    public string? ActivityType { get; set; }

    public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();
}
