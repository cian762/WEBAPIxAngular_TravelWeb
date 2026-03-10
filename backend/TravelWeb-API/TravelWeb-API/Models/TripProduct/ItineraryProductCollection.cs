using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.TripProduct;

public partial class ItineraryProductCollection
{
    public int FavoriteProductId { get; set; }

    public string? MemberId { get; set; }

    public int? TripProductId { get; set; }

    public virtual MemberInformation? Member { get; set; }

    public virtual TripProduct? TripProduct { get; set; }
}
