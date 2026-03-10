using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.TripProduct;

public partial class TripProduct
{
    public int TripProductId { get; set; }

    public string? ProductName { get; set; }

    public string? Description { get; set; }

    public int? DurationDays { get; set; }

    public string? CoverImage { get; set; }

    public decimal? DisplayPrice { get; set; }

    public int? ClickTimes { get; set; }

    public string? Status { get; set; }

    public int? PolicyId { get; set; }

    public int? RegionId { get; set; }

    public virtual ICollection<ItineraryProductCollection> ItineraryProductCollections { get; set; } = new List<ItineraryProductCollection>();

    public virtual CancellationPolicy? Policy { get; set; }

    public virtual TripRegion? Region { get; set; }

    public virtual ICollection<TripItineraryItem> TripItineraryItems { get; set; } = new List<TripItineraryItem>();

    public virtual ICollection<TripSchedule> TripSchedules { get; set; } = new List<TripSchedule>();

    public virtual ICollection<TravelTag> TravelTags { get; set; } = new List<TravelTag>();
}
