using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.TripProduct;

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

    public string? Propaganda { get; set; }

    public decimal? Longitude { get; set; }

    public decimal? Latitude { get; set; }

    public int ViewCount { get; set; }

    public virtual ICollection<ActivityImage> ActivityImages { get; set; } = new List<ActivityImage>();

    public virtual ICollection<TripItineraryItem> TripItineraryItems { get; set; } = new List<TripItineraryItem>();
}
