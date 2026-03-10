using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.TripProduct;

public partial class Resource
{
    public int ResourceId { get; set; }

    public string? ResourceName { get; set; }

    public string? ShortDescription { get; set; }

    public string? DefaultDescription { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public virtual ICollection<ResourcesImage> ResourcesImages { get; set; } = new List<ResourcesImage>();

    public virtual ICollection<TripItineraryItem> TripItineraryItems { get; set; } = new List<TripItineraryItem>();
}
