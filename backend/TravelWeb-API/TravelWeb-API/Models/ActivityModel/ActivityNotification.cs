using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.ActivityModel;

public partial class ActivityNotification
{
    public int NotificationId { get; set; }

    public int? UserId { get; set; }

    public int? ActivityId { get; set; }

    public string? NotificaitonType { get; set; }

    public string? SendStatus { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Activity? Activity { get; set; }
}
