using System;
using System.Collections.Generic;

namespace TravelWeb_API.Models.TripProduct;

public partial class TravelTag
{
    public int TravelTagid { get; set; }

    public string? TravelTagName { get; set; }

    public virtual ICollection<TripProduct> TripProducts { get; set; } = new List<TripProduct>();
}
