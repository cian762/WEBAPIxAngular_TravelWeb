using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.TripProduct;

public partial class TripItineraryItem
{
    public int ItineraryItemId { get; set; }

    public int TripProductId { get; set; }

    public int? DayNumber { get; set; }

    public int? SortOrder { get; set; }

    public int? AttractionId { get; set; }

    public int? ActivityId { get; set; }

    public int? ResourceId { get; set; }

    public string? CustomText { get; set; }

    public virtual Activity? Activity { get; set; }

    public virtual Attraction? Attraction { get; set; }

    public virtual Resource? Resource { get; set; }

    public virtual TripProduct TripProduct { get; set; } = null!;
}
