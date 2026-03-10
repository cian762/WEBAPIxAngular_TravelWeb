using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.ActivityModel;

public partial class Activity
{
    public int ActivityId { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateOnly? StartTime { get; set; }

    public DateOnly? EndTime { get; set; }

    public string? Address { get; set; }

    public string? OfficialLink { get; set; }

    public DateTime? UpdateAt { get; set; }

    public bool? SoftDelete { get; set; }

    public virtual ActivityAnalytic? ActivityAnalytic { get; set; }

    public virtual ICollection<ActivityEditLog> ActivityEditLogs { get; set; } = new List<ActivityEditLog>();

    public virtual ICollection<ActivityImage> ActivityImages { get; set; } = new List<ActivityImage>();

    public virtual ICollection<ActivityNotification> ActivityNotifications { get; set; } = new List<ActivityNotification>();

    public virtual ActivityPublishStatus? ActivityPublishStatus { get; set; }

    public virtual ICollection<ActivityTicketDetail> ActivityTicketDetails { get; set; } = new List<ActivityTicketDetail>();

    public virtual ICollection<PersonalizedRecommendation> PersonalizedRecommendations { get; set; } = new List<PersonalizedRecommendation>();

    public virtual ICollection<UserFavorite> UserFavorites { get; set; } = new List<UserFavorite>();

    public virtual ICollection<TagsRegion> Regions { get; set; } = new List<TagsRegion>();

    public virtual ICollection<TagsActivityType> Types { get; set; } = new List<TagsActivityType>();
}
